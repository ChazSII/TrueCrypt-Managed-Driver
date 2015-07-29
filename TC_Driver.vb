Imports System.IO
Imports System.Threading
Imports System.ServiceProcess
Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Driver.Constants
Imports CS2_Software.TrueCryptManagedDriver.Driver.Enums
Imports CS2_Software.TrueCryptManagedDriver.Driver.Structures
Imports CS2_Software.TrueCryptManagedDriver.Common.Enums
Imports CS2_Software.TrueCryptManagedDriver.Common.NativeMethods
Imports CS2_Software.TrueCryptManagedDriver.Common
Imports System.ComponentModel
Imports System.Configuration.Install
Imports Microsoft.Win32
Imports CS2_Software.TrueCryptManagedDriver.Common.NativeCallWrappers
Imports CS2_Software.TrueCryptManagedDriver.Security
Imports CS2_Software.TrueCryptManagedDriver.Common.Structures

Public Class TC_Driver
    Implements IDisposable

    Const CURRENT_VER As Integer = &H71A

    Private pDriver32bitLocation As String
    Private pDriver64bitLocation As String
    Private pIsPortableMode As Boolean = False

    Private Property ManagedDriver As ManagedDriver
    Private DriverSetupMutex As Mutex

    Public Property IsPortableMode As Boolean
        Friend Set(value As Boolean)
            pIsPortableMode = value
        End Set
        Get
            Return pIsPortableMode
        End Get
    End Property

    Public Sub New(ByVal DriverLocation64bit As String)
        If Not Environment.Is64BitProcess = Environment.Is64BitOperatingSystem Then
            Throw New PlatformNotSupportedException("TrueCryptAPI needs a 64 bit process to run correctly")
        End If

        'pDriver32bitLocation = Path.GetFullPath(".\Resources\truecrypt.sys")
        pDriver64bitLocation = Path.GetFullPath(DriverLocation64bit)

        Dim DriverStatus As TC_ERROR
        Dim DriverLoadAttempts As Integer = 0

        ManagedDriver = New ManagedDriver

        Do
            DriverStatus = StartDriver()
            DriverLoadAttempts += 1
        Loop While DriverStatus = TC_ERROR.FILES_OPEN_LOCK AndAlso DriverLoadAttempts < 3

        If DriverStatus <> TC_ERROR.SUCCESS Then
            Me.Dispose()
            Throw New ArgumentException("Driver not loaded. Error:" & DriverStatus)
        End If
    End Sub


#Region "Mount"
    Public Function MountContainer(ByVal FileName As String, ByVal DriveLetter As Char, ByVal Password As Password, ByVal Options As MOUNT_OPTIONS) As TC_ERROR
        Dim status As Boolean = False
        Dim mounted As TC_ERROR
        Dim tmp As String = ""

        If Not Password.ApplyKeyFile(tmp) Then Return False

        mounted = MountVolume(Asc(DriveLetter) - Asc("A"), FileName, tmp, False, False, Options, False)

        tmp = ""
        Options.ProtectedHidVolPassword = Nothing

        Return mounted
    End Function
#End Region

#Region "Dismount"
    Public Function Dismount(ByVal DriveLetter As Char, ByVal Force As Boolean) As TC_ERROR
        Return UnmountVolume(Asc(DriveLetter) - Asc("A"), Force)
    End Function

    Public Function DismountAll(ByVal Force As Boolean) As Boolean
        Dim mountList As MOUNT_LIST_STRUCT, unmount As UNMOUNT_STRUCT
        Dim dwResult As UInteger, bResult As Boolean
        Dim prevMountedDrives As UInteger, dismountMaxRetries As Integer = 3

        mountList = Nothing
        bResult = GetMountedVolume(mountList)

        If mountList.ulMountedDrives = 0 Then Return True

        BroadcastDeviceChange(DBT_DEVICE.REMOVEPENDING, 0, mountList.ulMountedDrives)

        prevMountedDrives = mountList.ulMountedDrives

        unmount.nDosDriveNo = 0
        unmount.ignoreOpenFiles = Force

        Do
            bResult = DeviceIoControlUnmount(ManagedDriver.hDriver, TC_IOCTL.UNMOUNT_ALL_VOLUMES, unmount, Marshal.SizeOf(unmount), unmount, Marshal.SizeOf(unmount), dwResult, Nothing)

            If Not bResult Then Return False

            If unmount.nReturnCode = TC_ERROR.SUCCESS And unmount.HiddenVolumeProtectionTriggered Then
                unmount.HiddenVolumeProtectionTriggered = False
            ElseIf unmount.nReturnCode = TC_ERROR.FILES_OPEN Then
                Thread.Sleep(500)
            End If

            dismountMaxRetries -= 1
        Loop While dismountMaxRetries > 0

        bResult = GetMountedVolume(mountList)
        BroadcastDeviceChange(DBT_DEVICE.REMOVECOMPLETE, 0, prevMountedDrives And mountList.ulMountedDrives)

        If Not unmount.nReturnCode = TC_ERROR.SUCCESS Then
            If unmount.nReturnCode = TC_ERROR.FILES_OPEN Then Return False

            Return False
        End If

        Return True
    End Function
#End Region


#Region "Driver Loading"

    Private Function StartDriver() As TC_ERROR
            If Not ManagedDriver.IsLoaded Then
                If CreateDriverSetupMutex() Then                                ' No other instance is currently attempting to install, register or start the driver
                    Dim Status As TC_ERROR = DriverLoad()                       ' Attempt to load the driver (non-install/portable mode)

                    CloseDriverSetupMutex()

                    If Status <> TC_ERROR.SUCCESS Then Return Status ' Check if load succeeded and return load error if applicable

                    pIsPortableMode = True
                Else
                    'Another instance is already attempting to install, register or start the driver
                    While Not CreateDriverSetupMutex()
                        Thread.Sleep(100) 'Wait until the other instance finisces
                    End While

                    CloseDriverSetupMutex()

                    Return TC_ERROR.FILES_OPEN_LOCK
                End If
            ElseIf Not ManagedDriver.DriverVersion = CURRENT_VER Then
                DriverUnload()
                ManagedDriver.Dispose()

                Return TC_ERROR.DRIVER_VERSION
            End If


            Return TC_ERROR.SUCCESS
    End Function

    Private Function DriverLoad() As TC_ERROR
        Dim driverPath As String = If(WinUtils.Is64Bit, pDriver64bitLocation, pDriver32bitLocation)
        Dim RegistryService As RegistryKey = My.Computer.Registry.LocalMachine.OpenSubKey(TC_SERVICE_REG_KEY)

        If Not File.Exists(driverPath) Then Return TC_ERROR.DONT_REPORT
        If Not IsNothing(RegistryService) AndAlso RegistryService.GetValue("Start") = SERVICE_START_TYPE.AUTO_START Then
            Return TC_ERROR.PARAMETER_INCORRECT
        End If

        Using ServiceManager As New ServiceManager("TrueCrypt", "TrueCrypt Driver", driverPath)
            If Not ServiceManager.ServiceExists Then
                ServiceManager.CreateWin32Service()
            End If

            If ServiceManager.StartWin32Service() Then
                ServiceManager.RemoveWin32Service()

                Return TC_ERROR.SUCCESS
            End If
        End Using

        Return TC_ERROR.OS_ERROR
    End Function

    Private Function DriverUnload() As Boolean
        Dim driverUnloaded As Boolean = False

        ' Driver unloaded or invalidated
        If Not ManagedDriver.IsLoaded Then Return True

        Try
            ' Test for any drives or applications attached to driver
            If ManagedDriver.VolumeCount = 0 Or ManagedDriver.HasAttachedApplications Then
                Return False                            ' Drives still mounted or other applications attached to driver
            End If


            'Stop driver services
            Using TC_SVC As New ServiceController("TrueCrypt")
                For i As Integer = 1 To 10
                    If Not TC_SVC.Status = ServiceControllerStatus.Stopped Then
                        TC_SVC.WaitForStatus(ServiceControllerStatus.Stopped, New TimeSpan(1000))
                    Else
                        driverUnloaded = True
                        Exit For
                    End If
                Next
            End Using

            Return driverUnloaded
        Catch ex As Exception
            'Servizio inesistente
            Dim tmp = ex
        Finally
            If driverUnloaded Then
                ManagedDriver.Dispose()
            End If
        End Try

        Return False
    End Function

#End Region

#Region "Setup Mutex"
    Const TC_MUTEX_NAME_SYSENC As String = "Global\TrueCrypt System Encryption Wizard"
    Const TC_MUTEX_NAME_NONSYS_INPLACE_ENC As String = "Global\TrueCrypt In-Place Encryption Wizard"
    Const TC_MUTEX_NAME_APP_SETUP As String = "Global\TrueCrypt Setup"
    Const TC_MUTEX_NAME_DRIVER_SETUP As String = "Global\TrueCrypt Driver Setup"

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

#Region "Portable Mode"
    Private Function IsNonIntallMode() As Boolean
        Dim dw As UInteger

        If pIsPortableMode Then Return True

        If DeviceIoControl(ManagedDriver.hDriver, TC_IOCTL.GET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dw, Nothing) Then
            pIsPortableMode = True
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub NotifyDriverOfPortableMode()
        Dim dwResult As UInteger

        If Not ManagedDriver.hDriver = INVALID_HANDLE_VALUE Then
            DeviceIoControl(ManagedDriver.hDriver, TC_IOCTL.SET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dwResult, Nothing)
        End If
    End Sub
#End Region


#Region "IDisposable Support"
    Private disposedValue As Boolean 'Per rilevare chiamate ridondanti

    'IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                'TODO: eliminare stato gestito (oggetti gestiti).
            End If

            ManagedDriver.Dispose()

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


'        Public Function DriverAttach() As TC_ERROR
'            Dim loadResult As TC_ERROR
'            Dim nLoadRetryCount As Integer = 0

'            'Try to open a handle to the device driver. It will be closed later.
'            hDriver = OpenDriverHandle()

'            If hDriver = INVALID_HANDLE_VALUE Then
'                If CreateDriverSetupMutex() Then
'                    'No other instance is currently attempting to install, register or start the driver
'                    'Attempt to load the driver (non-install/portable mode)

'load:               loadResult = DriverLoad()

'                    CloseDriverSetupMutex()

'                    If loadResult = TC_ERROR.SUCCESS Then
'                        pIsPortableMode = True
'                        hDriver = OpenDriverHandle()
'                    Else
'                        Return loadResult
'                    End If

'                    If hDriver = INVALID_HANDLE_VALUE Then Return TC_ERROR.OS_ERROR
'                Else
'                    'Another instance is already attempting to install, register or start the driver
'                    While Not CreateDriverSetupMutex()
'                        Thread.Sleep(100) 'Wait until the other instance finisces
'                    End While
'                End If
'            End If

'            CloseDriverSetupMutex()

'            If Not hDriver = IntPtr.Zero Then
'                If Not GetDriverVersion() Then
'                    Return TC_ERROR.OS_ERROR
'                ElseIf Not DriverVersion = CURRENT_VER Then
'                    'Unload an incompatbile version of the driver loaded in non-install mode and load the required version
'                    nLoadRetryCount += 1

'                    If IsNonIntallMode() And CreateDriverSetupMutex() And DriverUnload() And nLoadRetryCount < 3 Then
'                        GoTo load
'                    End If

'                    CloseDriverSetupMutex()
'                    CloseHandle(hDriver)
'                    hDriver = INVALID_HANDLE_VALUE

'                    Return TC_ERROR.DRIVER_VERSION
'                End If
'            End If

'            Return TC_ERROR.SUCCESS
'        End Function











'hManager = OpenSCManager(Nothing, Nothing, SC_MANAGER_ALL_ACCESS)

'If hManager = IntPtr.Zero Then
'    If Marshal.GetLastWin32Error = SYSTEM_ERROR.ACCESS_DENIED Then Return TC_ERROR.DONT_REPORT

'    Return TC_ERROR.OS_ERROR
'End If


'hService = OpenService(hManager, "TrueCrypt", SERVICE_ALL_ACCESS)


'If Not hService = IntPtr.Zero Then
'    'Remove stale service (driver is not loaded but service exists)
'    DeleteService(hService)
'    CloseServiceHandle(hService)
'    Thread.Sleep(500)
'End If

'hService = CreateService(hManager, "TrueCrypt", "TrueCrypt", SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER,
'                         SERVICE_START_TYPE.DEMAND_START, SERVICE_ERROR_NORMAL, driverPath,
'                         Nothing, Nothing, Nothing, Nothing, Nothing)

'If hService = IntPtr.Zero Then
'    CloseServiceHandle(hManager)
'    Return TC_ERROR.OS_ERROR
'End If

'res = StartService(hService, 0, Nothing)
'DeleteService(hService)

'CloseServiceHandle(hManager)
'CloseServiceHandle(hService)