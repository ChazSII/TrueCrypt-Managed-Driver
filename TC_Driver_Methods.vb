Imports System.IO
Imports System.Threading
Imports System.Runtime.InteropServices
Imports System.ServiceProcess
Imports TrueCryptDriver.Security
Imports TrueCryptDriver.Common
Imports TrueCryptDriver.Common.Enums
Imports TrueCryptDriver.Common.Structures

Public Class TC_Driver_Methods
    Implements IDisposable

    Private Const CURRENT_VER As Integer = &H71A

    Private Shared pDriver64bitLocation As String
    Private Shared pDriver32bitLocation As String
    Private Shared pIsPortableMode As Boolean

    Private DriverSetupMutex As Mutex
    Private DriverVersion As Int32

    Public Sub New()
        If Not Environment.Is64BitProcess = Environment.Is64BitOperatingSystem Then
            Throw New PlatformNotSupportedException("TrueCryptAPI needs a 64 bit process to run correctly")
        End If

        pIsPortableMode = False
    End Sub

#Region "Shared"
    Public Shared Property Driver64bitLocation As String
        Set(value As String)
            pDriver64bitLocation = Path.GetFullPath(value)
        End Set
        Get
            Return pDriver64bitLocation
        End Get
    End Property

    Public Shared Property Driver32bitLocation As String
        Set(value As String)
            pDriver32bitLocation = Path.GetFullPath(value)
        End Set
        Get
            Return pDriver32bitLocation
        End Get
    End Property

    Public Shared Property IsPortableMode As Boolean
        Friend Set(value As Boolean)
            pIsPortableMode = value
        End Set
        Get
            Return pIsPortableMode
        End Get
    End Property
#End Region

#Region "Driver"
    Public Function DriverAttach() As TC_ERROR
        'Try to open a handle to the device driver. It will be closed later.

        Dim res As TC_ERROR
        Dim nLoadRetryCount As Integer = 0

        hDriver = CreateFile(WIN32_ROOT_PREFIX, 0, FileShare.Read Or FileShare.Write, Nothing, OPEN_EXISTING, 0, Nothing)

        If hDriver = INVALID_HANDLE_VALUE Then
            If Not CreateDriverSetupMutex() Then
                'Another instance is already attempting to install, register or start the driver

                While Not CreateDriverSetupMutex()
                    Thread.Sleep(100) 'Wait until the other instance finisces
                End While
            Else
                'No other instance is currently attempting to install, register or start the driver
                'Attempt to load the driver (non-install/portable mode)

load:           res = DriverLoad()

                CloseDriverSetupMutex()

                If Not res = TC_ERROR.SUCCESS Then Return res

                pIsPortableMode = True

                hDriver = CreateFile(WIN32_ROOT_PREFIX, 0, FileShare.Read Or FileShare.Write, Nothing, OPEN_EXISTING, 0, Nothing)

                If hDriver = INVALID_HANDLE_VALUE Then Return 1
            End If
        End If

        CloseDriverSetupMutex()

        If Not hDriver = IntPtr.Zero Then
            Dim dwResult As UInteger
            Dim bResult As Boolean

            bResult = DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_GET_DRIVER_VERSION, Nothing, 0, DriverVersion, Marshal.SizeOf(DriverVersion), dwResult, Nothing)

            If Not bResult Then
                bResult = DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_LEGACY_GET_DRIVER_VERSION, Nothing, 0, DriverVersion, Marshal.SizeOf(DriverVersion), dwResult, Nothing)
            End If

            If Not bResult Then
                Return 1
            ElseIf Not DriverVersion = CURRENT_VER Then
                'Unload an incompatbile version of the driver loaded in non-install mode and load the required version

                nLoadRetryCount += 1
                If IsNonIntallMode() And CreateDriverSetupMutex() And DriverUnload() And nLoadRetryCount < 3 Then
                    GoTo load
                End If

                CloseDriverSetupMutex()
                CloseHandle(hDriver)
                hDriver = INVALID_HANDLE_VALUE

                Return TC_ERROR.DRIVER_VERSION
            End If
        End If

        Return TC_ERROR.SUCCESS
    End Function

    Private Function DriverLoad() As TC_ERROR
        Dim hManager As IntPtr, hService As IntPtr
        Dim driverPath As String
        Dim res As Boolean

        Dim RegistryService As Microsoft.Win32.RegistryKey = My.Computer.Registry.LocalMachine.OpenSubKey("SYSTEM\CurrentControlSet\Services\TrueCrypt")

        If Not IsNothing(RegistryService) AndAlso RegistryService.GetValue("Start") = SERVICE_START_TYPE.AUTO_START Then
            Return TC_ERROR.PARAMETER_INCORRECT
        End If

        driverPath = If(WinUtils.Is64Bit, pDriver64bitLocation, pDriver32bitLocation)
        If Not File.Exists(driverPath) Then Return TC_ERROR.DONT_REPORT

        hManager = OpenSCManager(Nothing, Nothing, SC_MANAGER_ALL_ACCESS)
        If hManager = IntPtr.Zero Then
            If Marshal.GetLastWin32Error = SYSTEM_ERROR.ACCESS_DENIED Then Return TC_ERROR.DONT_REPORT

            Return TC_ERROR.OS_ERROR
        End If

        hService = OpenService(hManager, "TrueCrypt", SERVICE_ALL_ACCESS)
        If Not hService = IntPtr.Zero Then
            Dim ServiceStatus As SERVICE_STATE
            ControlService(hService, SERVICE_CONTROL.INTERROGATE, ServiceStatus)

            ControlService(hService, SERVICE_CONTROL.STOP, ServiceStatus)

            'Remove stale service (driver is not loaded but service exists)
            DeleteService(hService)
            CloseServiceHandle(hService)
            Thread.Sleep(500)
        End If

        hService = CreateService(hManager, "TrueCrypt", "TrueCrypt", SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER,
                                 SERVICE_START_TYPE.DEMAND_START, SERVICE_ERROR_NORMAL, driverPath, Nothing, Nothing, Nothing, Nothing, Nothing)

        If hService = IntPtr.Zero Then
            CloseServiceHandle(hManager)
            Return TC_ERROR.OS_ERROR
        End If

        res = StartService(hService, 0, Nothing)
        DeleteService(hService)

        CloseServiceHandle(hManager)
        CloseServiceHandle(hService)

        Return If(Not res, TC_ERROR.OS_ERROR, TC_ERROR.SUCCESS)
    End Function

    Private Function DriverUnload() As Boolean
        Dim driver As MOUNT_LIST_STRUCT
        Dim refCount As Integer, volumesMounted As Integer
        Dim dwResult As UInteger, bResult As Boolean
        Dim driverUnloaded As Boolean = False

        If hDriver = INVALID_HANDLE_VALUE Then Return True

        'Boot partition control

        'Test for mounted volumes
        bResult = DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_IS_ANY_VOLUME_MOUNTED, Nothing, 0, volumesMounted, Marshal.SizeOf(volumesMounted), dwResult, Nothing)

        If Not bResult Then
            driver = New MOUNT_LIST_STRUCT
            bResult = DeviceIoControlListMounted(hDriver, TC_IOCTL.TC_IOCTL_LEGACY_GET_MOUNTED_VOLUMES, Nothing, 0, driver, Marshal.SizeOf(driver), dwResult, Nothing)

            If bResult Then volumesMounted = driver.ulMountedDrives
        End If

        If bResult Then
            If Not volumesMounted = 0 Then Return False
        Else
            Return True
        End If

        'Test for any applications attached to driver
        refCount = GetDriverRefCount()

        If refCount > 1 Then Return False

        CloseHandle(hDriver)
        hDriver = INVALID_HANDLE_VALUE

        'Stop driver services
        Try
            Dim Svc As New ServiceController("TrueCrypt")

            For i As Integer = 1 To 10
                If Not Svc.Status = ServiceControllerStatus.Stopped Then
                    Svc.Stop()
                Else
                    driverUnloaded = True
                    Exit For
                End If
            Next

            Svc.Dispose()
        Catch ex As Exception
            'Servizio inesistente
        End Try

[error]:
        If driverUnloaded Then
            hDriver = INVALID_HANDLE_VALUE
            Return True
        End If

        Return False
    End Function

    Private Function GetDriverRefCount() As Integer
        Dim dwResult As UInteger, bResult As UInteger, refCount As Integer

        bResult = DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_GET_DEVICE_REFCOUNT, refCount, Marshal.SizeOf(refCount), refCount, Marshal.SizeOf(refCount), dwResult, Nothing)

        If bResult Then
            Return refCount
        Else
            Return -1
        End If
    End Function

#Region "Portable Mode"
    Private Function IsNonIntallMode() As Boolean
        Dim dw As UInteger

        If pIsPortableMode Then Return True

        If DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_GET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dw, Nothing) Then
            pIsPortableMode = True
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub NotifyDriverOfPortableMode()
        Dim dwResult As UInteger

        If Not hDriver = INVALID_HANDLE_VALUE Then
            DeviceIoControl(hDriver, TC_IOCTL.TC_IOCTL_SET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dwResult, Nothing)
        End If
    End Sub
#End Region

    Private Function CreateDriverSetupMutex() As Boolean
        Try
            DriverSetupMutex = New Mutex(True, TC_MUTEX_NAME_DRIVER_SETUP)

            Return True
        Catch ex As Exception
            DriverSetupMutex = Nothing

            Return False
        End Try
    End Function

    Private Function CloseDriverSetupMutex() As Boolean
        Try
            If DriverSetupMutex IsNot Nothing Then
                DriverSetupMutex.Close()
                DriverSetupMutex.Dispose()
                DriverSetupMutex = Nothing
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
#End Region

#Region "Mount"
    Public Function MountContainer(ByVal FileName As String, ByVal DriveLetter As Char, ByVal Password As Password, ByVal Options As MOUNT_OPTIONS) As Boolean
        Dim status As Boolean = False
        Dim mounted As TC_ERROR
        Dim tmp As String = ""

        If Not Password.ApplyKeyFile(tmp) Then Return False

        mounted = MountVolume(Asc(DriveLetter) - Asc("A"), FileName, tmp, False, False, Options, False)

        tmp = ""
        Options.ProtectedHidVolPassword = Nothing

        Return mounted = TC_ERROR.SUCCESS
    End Function
#End Region

#Region "Dismount"
    Public Function Dismount(ByVal DriveLetter As Char, ByVal Force As Boolean) As Boolean
        Return UnmountVolume(Asc(DriveLetter) - Asc("A"), Force) = TC_ERROR.SUCCESS
    End Function

    Public Function DismountAll(ByVal Force As Boolean) As Boolean
        Dim mountList As MOUNT_LIST_STRUCT, unmount As UNMOUNT_STRUCT
        Dim dwResult As UInteger, bResult As Boolean
        Dim prevMountedDrives As UInteger, dismountMaxRetries As Integer = 3

        mountList = Nothing
        bResult = GetMountedVolume(mountList)

        If mountList.ulMountedDrives = 0 Then Return True

        BroadcastDeviceChange(DBT_DEVICEREMOVEPENDING, 0, mountList.ulMountedDrives)

        prevMountedDrives = mountList.ulMountedDrives

        unmount.nDosDriveNo = 0
        unmount.ignoreOpenFiles = Force

        Do
            bResult = DeviceIoControlUnmount(hDriver, TC_IOCTL.TC_IOCTL_UNMOUNT_ALL_VOLUMES, unmount, Marshal.SizeOf(unmount), unmount, Marshal.SizeOf(unmount), dwResult, Nothing)

            If Not bResult Then Return False

            If unmount.nReturnCode = TC_ERROR.SUCCESS And unmount.HiddenVolumeProtectionTriggered Then
                unmount.HiddenVolumeProtectionTriggered = False
            ElseIf unmount.nReturnCode = TC_ERROR.FILES_OPEN Then
                Thread.Sleep(500)
            End If

            dismountMaxRetries -= 1
        Loop While dismountMaxRetries > 0

        bResult = GetMountedVolume(mountList)
        BroadcastDeviceChange(DBT_DEVICEREMOVECOMPLETE, 0, prevMountedDrives And mountList.ulMountedDrives)

        If Not unmount.nReturnCode = TC_ERROR.SUCCESS Then
            If unmount.nReturnCode = TC_ERROR.FILES_OPEN Then Return False

            Return False
        End If

        Return True
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean 'Per rilevare chiamate ridondanti

    'IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                'TODO: eliminare stato gestito (oggetti gestiti).
            End If

            CloseHandle(hDriver)
            hDriver = INVALID_HANDLE_VALUE

            'TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del seguente Finalize().
            'TODO: impostare campi di grandi dimensioni su null.
        End If

        Me.disposedValue = True
    End Sub

    'TODO: eseguire l'override di Finalize() solo se Dispose(ByVal disposing As Boolean) dispone del codice per liberare risorse non gestite.
    Protected Overrides Sub Finalize()
        'Non modificare questo codice. Inserire il codice di pulizia in Dispose(ByVal disposing As Boolean).
        Dispose(False)
        MyBase.Finalize()
    End Sub

    'Questo codice è aggiunto da Visual Basic per implementare in modo corretto il modello Disposable.
    Public Sub Dispose() Implements IDisposable.Dispose
        'Non modificare questo codice. Inserire il codice di pulizia in Dispose(disposing As Boolean).
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class