Imports System.Runtime.InteropServices

Namespace Common.Native
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
        Private Property SvcManagerHandle As IntPtr = IntPtr.Zero
        Private Property ServiceHandle As IntPtr = IntPtr.Zero
        Private Property ServiceName As String
        Private Property DisplayName As String
        'Private Property DriverPath As String

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


        Protected Friend Sub New(serviceName As String, displayName As String) ', driverLocation As String)
            Me.ServiceName = serviceName
            Me.DisplayName = displayName
            'DriverPath = driverLocation

            SvcManagerHandle = OpenSCManager(Nothing, Nothing, SC_MANAGER_ALL_ACCESS)

            If SvcManagerHandle = IntPtr.Zero Then
                _LastWin32Error = Marshal.GetLastWin32Error()
                Dim tmp = "Couldn't open service manager"
            Else
                ServiceHandle = OpenService(SvcManagerHandle, Me.ServiceName, SERVICE_ALL_ACCESS)
            End If

            If ServiceHandle.Equals(IntPtr.Zero) Then
                _ServiceExists = False
            Else
                _ServiceExists = True
            End If
        End Sub

        Protected Friend Function CreateWin32Service(driverPath As String) As Boolean
            ServiceHandle = CreateService(SvcManagerHandle,
                                          ServiceName,
                                          DisplayName,
                                          SERVICE_ALL_ACCESS,
                                          SERVICE_KERNEL_DRIVER,
                                          SERVICE_START_TYPE.DEMAND_START,
                                          SERVICE_ERROR_NORMAL,
                                          driverPath,
                                          Nothing, 0, Nothing, Nothing, Nothing)

            If ServiceHandle.Equals(IntPtr.Zero) Then
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
                Dim intReturnVal As Integer = StartService(ServiceHandle, 0, Nothing)

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
            Using serviceCtrl As New ServiceProcess.ServiceController(ServiceName)
                For i As Integer = 1 To 10
                    If Not serviceCtrl.Status = ServiceProcess.ServiceControllerStatus.Stopped Then
                        serviceCtrl.WaitForStatus(ServiceProcess.ServiceControllerStatus.Stopped, New TimeSpan(1000))
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
                Dim intReturnVal As Integer = DeleteService(ServiceHandle)

                ' If the value i is zero, then there was an error starting the service.
                ' note: error may arise if the service is already running or some other problem.
                If intReturnVal = 0 Then
                    RemoveWin32Service = True
                    CloseServiceHandle(ServiceHandle)
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

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.

                CloseServiceHandle(ServiceHandle)
                CloseHandle(SvcManagerHandle)

                SvcManagerHandle = IntPtr.Zero
                ServiceHandle = IntPtr.Zero

                _ServiceExists = Nothing
                _LastWin32Error = Nothing

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