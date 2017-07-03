Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports CS2_Software.TrueCryptManagedDriver.Common
Imports CS2_Software.TrueCryptManagedDriver.Common.Native
Imports Microsoft.Win32

Namespace Driver
    Friend Class ManagedDriverInstance
        Implements IDisposable

        Const CURRENT_VER As Integer = &H71A

        Const TC_UNIQUE_ID_PREFIX As String = "TrueCryptVolume"
        Const TC_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"

        Const NT_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"
        Const NT_ROOT_PREFIX As String = "\Device\TrueCrypt"
        Const DOS_MOUNT_PREFIX As String = "\\DosDevices\"
        Const DOS_ROOT_PREFIX As String = "\DosDevices\TrueCrypt"
        Const WIN32_ROOT_PREFIX As String = "\\.\TrueCrypt"

        Const TC_MUTEX_NAME_SYSENC As String = "Global\TrueCrypt System Encryption Wizard"
        Const TC_MUTEX_NAME_NONSYS_INPLACE_ENC As String = "Global\TrueCrypt In-Place Encryption Wizard"
        Const TC_MUTEX_NAME_APP_SETUP As String = "Global\TrueCrypt Setup"
        Const TC_MUTEX_NAME_DRIVER_SETUP As String = "Global\TrueCrypt Driver Setup"

        Private Property _DriverHandle As IntPtr = -1
        Private Property _DriverVersion As Int32 = 0
        Private Property _VolumeCount As Integer = 0

        Private ReadOnly Property _DriverPath As String

        Private Property ServiceManager As ServiceManager
        Private Property DriverSetupMutex As Mutex


        Public ReadOnly Property IsLoaded As Boolean
            Get
                Return Not (_DriverHandle = Driver.INVALID_HANDLE_VALUE)
            End Get
        End Property

        Public ReadOnly Property DriverHandle As IntPtr
            Get
                If Not IsLoaded Then
                    _DriverHandle = OpenDriverHandle()
                End If

                Return _DriverHandle
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


        Public Sub New(Optional driverPath As String = Nothing)
            _DriverPath = driverPath

            ServiceManager = New ServiceManager("TrueCrypt", "TrueCrypt Driver") ', driverPath)

            '' Check if driver is already loaded
            _DriverHandle = OpenDriverHandle()

            If Not IsLoaded Then
                Dim DriverStatus As ErrorCodes
                Dim DriverLoadAttempts As Integer = 0

                Do
                    DriverStatus = StartDriver()
                    DriverLoadAttempts += 1
                Loop While DriverStatus = ErrorCodes.FILES_OPEN_LOCK AndAlso DriverLoadAttempts < 3

                If DriverStatus <> ErrorCodes.SUCCESS Then
                    Me.Dispose()

                    Throw New Exception("Driver not loaded. Error:" & DriverStatus)
                End If

                _DriverHandle = OpenDriverHandle()
            End If
        End Sub


#Region "Driver Calls"

        Private Function OpenDriverHandle() As IntPtr
            Return CreateFile(WIN32_ROOT_PREFIX,
                              0,
                              FileShare.ReadWrite,
                              Nothing,
                              FileMode.Open,
                              0,
                              Nothing)
        End Function

        Private Function GetDriverVersion() As UInteger
            Dim dwResult As UInteger = 0
            Dim methodSucceeded As Boolean

            methodSucceeded = DeviceIoControl(DriverHandle,
                                              IoCtrlCodes.GET_DRIVER_VERSION,
                                              Nothing,
                                              0,
                                              _DriverVersion,
                                              Marshal.SizeOf(_DriverVersion),
                                              dwResult,
                                              Nothing)

            If Not methodSucceeded Then
                methodSucceeded = DeviceIoControl(DriverHandle,
                                                  IoCtrlCodes.LEGACY_GET_DRIVER_VERSION,
                                                  Nothing,
                                                  0,
                                                  _DriverVersion,
                                                  Marshal.SizeOf(_DriverVersion),
                                                  dwResult,
                                                  Nothing)
            End If

            Return dwResult
        End Function

        Private Function GetMountedVolumeCount() As Boolean
            Dim dwResult As UInteger
            Dim MethodSucceeded As Boolean

            MethodSucceeded = DeviceIoControl(DriverHandle,
                                              IoCtrlCodes.IS_ANY_VOLUME_MOUNTED,
                                              Nothing,
                                              0,
                                              _VolumeCount,
                                              Marshal.SizeOf(_VolumeCount),
                                              dwResult,
                                              Nothing)

            If Not MethodSucceeded Then
                Dim mntList As New MOUNT_LIST_STRUCT
                MethodSucceeded = DeviceIoControlListMounted(DriverHandle,
                                                             IoCtrlCodes.LEGACY_GET_MOUNTED_VOLUMES,
                                                             Nothing,
                                                             0,
                                                             mntList,
                                                             Marshal.SizeOf(mntList),
                                                             dwResult,
                                                             Nothing)

                _VolumeCount = mntList.ulMountedDrives
            End If

            Return MethodSucceeded
        End Function

        Private Function GetDriverRefCount() As Integer
            Dim dwResult As UInteger
            Dim resultSuccess As Boolean
            Dim refCount As Integer

            resultSuccess = DeviceIoControl(DriverHandle,
                                            IoCtrlCodes.GET_DEVICE_REFCOUNT,
                                            refCount,
                                            Marshal.SizeOf(refCount),
                                            refCount,
                                            Marshal.SizeOf(refCount),
                                            dwResult,
                                            Nothing)

            If resultSuccess Then
                Return refCount
            Else
                Return -1
            End If
        End Function

#End Region


#Region "Driver Loading"

        Private Function StartDriver() As ErrorCodes
            If Not IsLoaded Then
                If CreateDriverSetupMutex() Then                        ' No other instance is currently attempting to install, register or start the driver
                    Dim Status As ErrorCodes = DriverLoad()             ' Attempt to load the driver (non-install/portable mode)

                    CloseDriverSetupMutex()

                    If Status <> ErrorCodes.SUCCESS Then Return Status  ' Check if load succeeded and return load error if applicable

                    '_IsPortableMode = True
                Else
                    'Another instance is already attempting to install, register or start the driver
                    While Not CreateDriverSetupMutex()
                        Thread.Sleep(100) 'Wait until the other instance finisces
                    End While

                    CloseDriverSetupMutex()

                    Return ErrorCodes.FILES_OPEN_LOCK
                End If
            ElseIf Not DriverVersion = CURRENT_VER Then
                DriverUnload()

                Dispose()

                Return ErrorCodes.DRIVER_VERSION
            End If

            Return ErrorCodes.SUCCESS
        End Function

        Private Function DriverLoad() As ErrorCodes
            Dim RegistryService As RegistryKey = My.Computer.Registry.LocalMachine.OpenSubKey(TC_SERVICE_REG_KEY)

            If Not IsNothing(RegistryService) AndAlso RegistryService.GetValue("Start") = SERVICE_START_TYPE.AUTO_START Then
                Return ErrorCodes.PARAMETER_INCORRECT
            End If

            If Not ServiceManager.ServiceExists Then
                If _DriverPath IsNot Nothing AndAlso File.Exists(_DriverPath) Then
                    ServiceManager.CreateWin32Service(_DriverPath)
                ElseIf _DriverPath Is Nothing Then
                    Throw New Exception("Driver not installed on system and driver path not provided.")
                ElseIf Not File.Exists(_DriverPath) Then
                    Throw New FileNotFoundException("Driver not installed on system and driver path not found.", _DriverPath)
                End If
            End If

            If ServiceManager.StartWin32Service() Then
                ServiceManager.RemoveWin32Service()

                Return ErrorCodes.SUCCESS
            End If

            Return ErrorCodes.OS_ERROR
        End Function

        Private Function DriverUnload() As Boolean
            Dim driverUnloaded As Boolean = False

            ' Driver unloaded or invalidated
            If Not IsLoaded Then Return True

            Try
                ' Test for any drives or applications attached to driver
                If VolumeCount = 0 Or HasAttachedApplications Then
                    Return False    ' Drives still mounted or other applications attached to driver
                End If

                If ServiceManager.ServiceExists Then
                    driverUnloaded = ServiceManager.StopWin32Service()
                End If

                Return driverUnloaded
            Catch ex As Exception
                'Servizio inesistente
                Dim tmp = ex
            Finally
                If driverUnloaded Then
                    Dispose()
                End If
            End Try

            Return False
        End Function

#End Region

#Region "Setup Mutex"

        ''' <summary>
        ''' Mutex handling To prevent multiple instances Of the wizard Or main app from trying To install
        ''' Or register the driver Or from trying to launch it in portable mode at the same time.
        ''' Returns TRUE if the mutex Is (Or had been) successfully acquired (otherwise FALSE).
        ''' </summary> 
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

        Private _IsPortableMode As Boolean = False

        Public Property IsPortableMode As Boolean
            Friend Set(value As Boolean)
                _IsPortableMode = value
            End Set
            Get
                Return _IsPortableMode
            End Get
        End Property

        Private Function IsNonIntallMode() As Boolean
            Dim dw As UInteger

            If _IsPortableMode Then Return True

            If DeviceIoControl(DriverHandle, IoCtrlCodes.GET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dw, Nothing) Then
                _IsPortableMode = True
                Return True
            Else
                Return False
            End If
        End Function

        Private Sub NotifyDriverOfPortableMode()
            Dim dwResult As UInteger

            If Not DriverHandle = INVALID_HANDLE_VALUE Then
                DeviceIoControl(DriverHandle, IoCtrlCodes.SET_PORTABLE_MODE_STATUS, Nothing, 0, Nothing, 0, dwResult, Nothing)
            End If
        End Sub
#End Region


#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If DriverSetupMutex IsNot Nothing Then DriverSetupMutex.Dispose()

                    ServiceManager.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.

                CloseHandle(DriverHandle)
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace