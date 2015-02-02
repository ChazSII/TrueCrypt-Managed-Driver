Imports System.IO
Imports System.Threading
Imports System.Runtime.InteropServices
Imports TrueCryptDriver.Driver.Enums
Imports TrueCryptDriver.Driver.Constants
Imports TrueCryptDriver.Common.Enums
Imports TrueCryptDriver.Common.NativeMethods
Imports TrueCryptDriver.Driver.Structures

Namespace Common
    Namespace NativeCallWrappers

        Friend Class ServiceManager
            Implements IDisposable

            ' Service Constants
            Private Const SC_MANAGER_ALL_ACCESS As Integer = &HF003F 'Service Control Manager object specific access types
            Private Const SC_MANAGER_CREATE_SERVICE As Integer = &H2 'Service Control Manager create service access
            Private Const SERVICE_ALL_ACCESS As Integer = &HF01FF    'Service object specific access type
            Private Const SERVICE_KERNEL_DRIVER As Integer = &H1     'Service Type
            Private Const SERVICE_ERROR_NORMAL As Integer = &H1      'Error control type
            Private Const GENERIC_WRITE As Integer = &H40000000
            Private Const DELETE As Integer = &H10000

            ' Service Handles
            Private Property hSCManager As IntPtr = IntPtr.Zero
            Private Property hService As IntPtr = IntPtr.Zero
            Private Property Service As String
            Private Property Display As String
            Private Property DriverPath As String

            ' Private properties
            Private Property _ServiceExists As Boolean = False
            Private Property _LastWin32Error As Integer = 0

            ' Public properties
            Public ReadOnly Property ServiceExists As Boolean
                Get
                    Return _ServiceExists
                End Get
            End Property

            Public ReadOnly Property LastWin32Error As Integer
                Get
                    Return _LastWin32Error
                End Get
            End Property



            Protected Friend Sub New(ByVal ServiceName As String, ByVal DisplayName As String, ByVal DriverLocation As String)
                Service = ServiceName
                Display = DisplayName
                DriverPath = DriverLocation

                hSCManager = OpenSCManager(Nothing, Nothing, SC_MANAGER_ALL_ACCESS)

                If hSCManager = IntPtr.Zero Then
                    _LastWin32Error = Marshal.GetLastWin32Error()
                    Dim tmp = "Couldn't open service manager"
                Else
                    hService = OpenService(hSCManager, Service, SERVICE_ALL_ACCESS)
                End If

                If hService.Equals(IntPtr.Zero) Then
                    _ServiceExists = False
                Else
                    _ServiceExists = True
                End If
            End Sub

            Protected Friend Function CreateWin32Service() As Boolean
                hService = CreateService(hSCManager, Service, Display,
                                            SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER,
                                            SERVICE_START_TYPE.DEMAND_START, SERVICE_ERROR_NORMAL, DriverPath,
                                            Nothing, 0, Nothing, Nothing, Nothing)

                If hService.Equals(IntPtr.Zero) Then
                    _LastWin32Error = Marshal.GetLastWin32Error()
                    _ServiceExists = False

                    Dim tmp = "Couldn't create service"
                Else
                    _ServiceExists = True

                    Return True
                End If

                Return False
            End Function

            Protected Friend Function StartWin32Service() As Boolean
                If ServiceExists Then
                    'now trying to start the service
                    Dim intReturnVal As Integer = StartService(hService, 0, Nothing)

                    ' If the value i is zero, then there was an error starting the service.
                    ' note: error may arise if the service is already running or some other problem.
                    If intReturnVal = 0 Then
                        _LastWin32Error = Marshal.GetLastWin32Error()
                        StartWin32Service = False
                    Else
                        StartWin32Service = True
                    End If
                End If

                Return StartWin32Service
            End Function

            Protected Friend Function StopWin32Service() As Boolean
                Using TC_SVC As New ServiceProcess.ServiceController("TrueCrypt")
                    For i As Integer = 1 To 10
                        If Not TC_SVC.Status = ServiceProcess.ServiceControllerStatus.Stopped Then
                            TC_SVC.WaitForStatus(ServiceProcess.ServiceControllerStatus.Stopped, New TimeSpan(1000))
                        Else
                            Return True
                        End If
                    Next
                End Using

                Return False
                'If _ServiceExists Then
                '    'now trying to start the service
                '    Dim intReturnVal As Integer = StopService(hService, 0, Nothing)

                '    ' If the value i is zero, then there was an error starting the service.
                '    ' note: error may arise if the service is already running or some other problem.
                '    If intReturnVal = 0 Then
                '        StopWin32Service = True
                '    Else
                '        _LastWin32Error = Marshal.GetLastWin32Error()
                '        StopWin32Service = False
                '    End If
                'End If

                'Return StopWin32Service
            End Function

            Protected Friend Function RemoveWin32Service() As Boolean
                If ServiceExists Then
                    'now trying to start the service
                    Dim intReturnVal As Integer = DeleteService(hService)

                    ' If the value i is zero, then there was an error starting the service.
                    ' note: error may arise if the service is already running or some other problem.
                    If intReturnVal = 0 Then
                        RemoveWin32Service = True
                        CloseServiceHandle(hService)
                    Else
                        _LastWin32Error = Marshal.GetLastWin32Error()
                        RemoveWin32Service = False
                    End If
                End If

                Return StartWin32Service()
            End Function

#Region "IDisposable Support"
            Private disposedValue As Boolean ' To detect redundant calls

            ' IDisposable
            Protected Overridable Sub Dispose(disposing As Boolean)
                If Not Me.disposedValue Then
                    If disposing Then
                        ' TODO: dispose managed state (managed objects).
                    End If

                    CloseServiceHandle(hService)
                    CloseHandle(hSCManager)

                    hSCManager = IntPtr.Zero
                    hService = IntPtr.Zero

                    _ServiceExists = Nothing
                    _LastWin32Error = Nothing


                    ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    ' TODO: set large fields to null.
                End If
                Me.disposedValue = True
            End Sub

            ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
            'Protected Overrides Sub Finalize()
            '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            '    Dispose(False)
            '    MyBase.Finalize()
            'End Sub

            ' This code added by Visual Basic to correctly implement the disposable pattern.
            Public Sub Dispose() Implements IDisposable.Dispose
                ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
#End Region

        End Class

        Friend Class ManagedDriver
            Implements IDisposable

            Const TC_UNIQUE_ID_PREFIX As String = "TrueCryptVolume"
            Const TC_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"

            Const NT_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"
            Const NT_ROOT_PREFIX As String = "\Device\TrueCrypt"
            Const DOS_MOUNT_PREFIX As String = "\\DosDevices\"
            Const DOS_ROOT_PREFIX As String = "\DosDevices\TrueCrypt"
            Const WIN32_ROOT_PREFIX As String = "\\.\TrueCrypt"

            Private Property _hDriver As IntPtr
            Private Property _DriverVersion As Int32
            Private Property _VolumeCount As Integer


            Public ReadOnly Property IsLoaded As Boolean
                Get
                    Return Not (_hDriver = INVALID_HANDLE_VALUE)
                End Get
            End Property

            Public ReadOnly Property hDriver As IntPtr
                Get
                    If Not IsLoaded Then
                        _hDriver = OpenDriverHandle()
                    End If

                    Return _hDriver
                End Get
            End Property

            Public ReadOnly Property DriverVersion As Int32
                Get
                    If IsLoaded Then
                        If _DriverVersion = 0 Then
                            GetDriverVersion()
                        End If

                        Return _DriverVersion
                    Else
                        Return 0
                    End If
                End Get
            End Property

            Public ReadOnly Property HasAttachedApplications As Boolean
                Get
                    If IsLoaded Then
                        Return GetDriverRefCount() > 0
                    End If

                    Return Nothing
                End Get
            End Property

            Public ReadOnly Property VolumeCount As Integer
                Get
                    If IsLoaded Then
                        GetMountedVolumeCount()
                        Return _VolumeCount
                    Else
                        Return -1
                    End If
                End Get
            End Property



            Public Sub New()
                _hDriver = OpenDriverHandle()
            End Sub



            Private Function OpenDriverHandle() As IntPtr
                Return CreateFile(WIN32_ROOT_PREFIX, 0,
                                  FileShare.Read Or FileShare.Write,
                                  Nothing, FileMode.Open, 0, Nothing)
            End Function

            Private Function GetDriverVersion() As Boolean
                    Dim dwResult As UInteger
                    Dim MethodSucceeded As Boolean

                    MethodSucceeded = DeviceIoControl(hDriver, TC_IOCTL.GET_DRIVER_VERSION,
                                                      Nothing, 0, _DriverVersion,
                                                      Marshal.SizeOf(_DriverVersion), dwResult, Nothing)

                    If Not MethodSucceeded Then
                        MethodSucceeded = DeviceIoControl(hDriver, TC_IOCTL.LEGACY_GET_DRIVER_VERSION,
                                                          Nothing, 0, _DriverVersion,
                                                          Marshal.SizeOf(_DriverVersion), dwResult, Nothing)
                    End If

                    Return MethodSucceeded
            End Function

            Private Function GetMountedVolumeCount() As Boolean
                Dim dwResult As UInteger
                Dim MethodSucceeded As Boolean

                MethodSucceeded = DeviceIoControl(hDriver, TC_IOCTL.IS_ANY_VOLUME_MOUNTED,
                                          Nothing, 0, _VolumeCount,
                                          Marshal.SizeOf(_VolumeCount),
                                          dwResult, Nothing)

                If Not MethodSucceeded Then
                    Dim Driver As New MOUNT_LIST_STRUCT
                    MethodSucceeded = DeviceIoControlListMounted(hDriver, TC_IOCTL.LEGACY_GET_MOUNTED_VOLUMES,
                                                         Nothing, 0, Driver,
                                                         Marshal.SizeOf(Driver),
                                                         dwResult, Nothing)

                    _VolumeCount = Driver.ulMountedDrives
                End If

                Return MethodSucceeded
            End Function

            Private Function GetDriverRefCount() As Integer
                Dim dwResult As UInteger, bResult As UInteger, refCount As Integer

                bResult = DeviceIoControl(hDriver, TC_IOCTL.GET_DEVICE_REFCOUNT,
                                          refCount, Marshal.SizeOf(refCount),
                                          refCount, Marshal.SizeOf(refCount),
                                          dwResult, Nothing)

                If bResult Then
                    Return refCount
                Else
                    Return -1
                End If
            End Function


#Region "IDisposable Support"
            Private disposedValue As Boolean ' To detect redundant calls

            ' IDisposable
            Protected Overridable Sub Dispose(disposing As Boolean)
                If Not Me.disposedValue Then
                    If disposing Then
                        ' TODO: dispose managed state (managed objects).
                    End If

                    ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    ' TODO: set large fields to null.

                    CloseHandle(hDriver)
                End If
                Me.disposedValue = True
            End Sub

            ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
            'Protected Overrides Sub Finalize()
            '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            '    Dispose(False)
            '    MyBase.Finalize()
            'End Sub

            ' This code added by Visual Basic to correctly implement the disposable pattern.
            Public Sub Dispose() Implements IDisposable.Dispose
                ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
#End Region

        End Class

    End Namespace
End Namespace