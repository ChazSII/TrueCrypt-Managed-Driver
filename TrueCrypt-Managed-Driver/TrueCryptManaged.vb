Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports CS2_Software.TrueCryptManagedDriver.Common.Native
Imports CS2_Software.TrueCryptManagedDriver.Driver
Imports CS2_Software.TrueCryptManagedDriver.Security

Public Class TrueCryptManaged
    Implements IDisposable

    Private Property ManagedDriver As ManagedDriverInstance

    Private _Driver32bitLocation As String
    Private _Driver64bitLocation As String

    Public Sub New(driverLocation64bit As String) ', Optional driverLocation32bit As String = Nothing)
        If Not Environment.Is64BitProcess = Environment.Is64BitOperatingSystem Then
            Throw New PlatformNotSupportedException("TrueCryptAPI needs a 64 bit process to run correctly")
        End If

        '_Driver32bitLocation = If(driverLocation32bit Is Nothing, Nothing, Path.GetFullPath(driverLocation32bit))
        _Driver64bitLocation = Path.GetFullPath(driverLocation64bit)

        ManagedDriver = New ManagedDriverInstance(_Driver32bitLocation)
        'If(Environment.Is64BitProcess, _Driver64bitLocation, _Driver32bitLocation))
    End Sub

#Region "Mount"
    Public Function MountContainer(containerPath As String, password As String, driveLetter As Char,
                                   Optional [readOnly] As Boolean = False,
                                   Optional removable As Boolean = False,
                                   Optional preserveTimestamp As Boolean = False,
                                   Optional useBackupHeader As Boolean = False,
                                   Optional recoveryMode As Boolean = False,
                                   Optional keyFileNames As String() = Nothing) As ErrorCodes

        Dim mountOptions As New Common.MountOptions With {
            .DriveLetter = driveLetter,
            .ReadOnly = [readOnly],
            .Removable = removable,
            .PreserveTimestamp = preserveTimestamp,
            .UseBackupHeader = useBackupHeader,
            .RecoveryMode = recoveryMode
        }

        Return MountContainer(containerPath, password, mountOptions, keyFileNames)
    End Function

    Public Function MountContainer(containerPath As String, password As String, options As Common.MountOptions, Optional keyFileNames As String() = Nothing) As ErrorCodes
        Dim mountResult As ErrorCodes
        Dim mountPassword As New Password(password, keyFileNames)

        Dim mountOptions As New MOUNT_OPTIONS With {
            .ReadOnly = options.[ReadOnly],
            .Removable = options.Removable,
            .PreserveTimestamp = options.PreserveTimestamp,
            .UseBackupHeader = options.UseBackupHeader,
            .RecoveryMode = options.RecoveryMode
        }

        mountResult = MountVolume(Asc(options.DriveLetter) - Asc("A"), containerPath, mountPassword, False, False, mountOptions, False)

        Return mountResult
    End Function
#End Region

#Region "Dismount"
    Public Function Dismount(driveLetter As Char, force As Boolean) As ErrorCodes
        Return UnmountVolume(Asc(driveLetter) - Asc("A"), force)
    End Function

    Public Function DismountAll(force As Boolean) As Boolean
        Dim mountList As MOUNT_LIST_STRUCT
        Dim unmount As UNMOUNT_STRUCT
        Dim dwResult As UInteger
        Dim bResult As Boolean
        Dim prevMountedDrives As UInteger
        Dim dismountMaxRetries As Integer = 3

        mountList = Nothing
        bResult = GetMountedVolume(mountList)

        If mountList.ulMountedDrives = 0 Then Return True

        BroadcastDeviceChange(DBT_DEVICE.REMOVEPENDING, 0, mountList.ulMountedDrives)

        prevMountedDrives = mountList.ulMountedDrives

        unmount.nDosDriveNo = 0
        unmount.ignoreOpenFiles = force

        Do
            bResult = DeviceIoControlUnmount(ManagedDriver.DriverHandle, IoCtrlCodes.UNMOUNT_ALL_VOLUMES, unmount, Marshal.SizeOf(unmount), unmount, Marshal.SizeOf(unmount), dwResult, Nothing)

            If Not bResult Then Return False

            If unmount.nReturnCode = ErrorCodes.SUCCESS And unmount.HiddenVolumeProtectionTriggered Then
                unmount.HiddenVolumeProtectionTriggered = False
            ElseIf unmount.nReturnCode = ErrorCodes.FILES_OPEN Then
                Thread.Sleep(500)
            End If

            dismountMaxRetries -= 1
        Loop While dismountMaxRetries > 0

        bResult = GetMountedVolume(mountList)
        BroadcastDeviceChange(DBT_DEVICE.REMOVECOMPLETE, 0, prevMountedDrives And mountList.ulMountedDrives)

        If Not unmount.nReturnCode = ErrorCodes.SUCCESS Then
            If unmount.nReturnCode = ErrorCodes.FILES_OPEN Then Return False

            Return False
        End If

        Return True
    End Function
#End Region

    '#Region "Portable Mode"

    'Private _IsPortableMode As Boolean = False

    'Public Property IsPortableMode As Boolean
    '    Friend Set(value As Boolean)
    '        _IsPortableMode = value
    '    End Set
    '    Get
    '        Return _IsPortableMode
    '    End Get
    'End Property

    '    Private Function IsNonIntallMode() As Boolean
    '        Dim dw As UInteger

    '        If _IsPortableMode Then Return True

    '        If DeviceIoControl(ManagedDriver.DriverHandle, IoCtrlCodes.GET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dw, Nothing) Then
    '            _IsPortableMode = True
    '            Return True
    '        Else
    '            Return False
    '        End If
    '    End Function

    '    Private Sub NotifyDriverOfPortableMode()
    '        Dim dwResult As UInteger

    '        If Not ManagedDriver.DriverHandle = INVALID_HANDLE_VALUE Then
    '            DeviceIoControl(ManagedDriver.DriverHandle, IoCtrlCodes.SET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dwResult, Nothing)
    '        End If
    '    End Sub
    '#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                ManagedDriver.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
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