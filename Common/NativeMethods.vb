Imports System.IO
Imports System.Text
Imports System.Runtime.InteropServices
Imports TrueCryptDriver.Common.Enums
Imports TrueCryptDriver.Driver.Enums
Imports TrueCryptDriver.Driver.Constants
Imports TrueCryptDriver.Driver.Structures

Namespace Common
    Friend Module NativeMethods

#Region "SHELL32"

        <DllImport("shell32.dll")> _
        Public Sub SHChangeNotify(ByVal wEventID As Integer, ByVal uFlags As Integer, ByVal dwItem1 As String, ByVal dwItem2 As String)
        End Sub

#End Region

#Region "USER32"

        <DllImport("user32.dll", SetLastError:=True)> _
        Public Sub SendMessageTimeout(ByVal windowHandle As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr, ByVal flags As Integer, ByVal timeout As Integer, ByRef result As IntPtr)
        End Sub

        <DllImport("user32.dll", EntryPoint:="MessageBoxW", SetLastError:=True, Charset:=CharSet.Unicode)> _
        Public Function MessageBox(hwnd As IntPtr, <MarshalAs(UnmanagedType.LPTStr)> lpText As String, <MarshalAs(UnmanagedType.LPTStr)> lpCaption As String, <MarshalAs(UnmanagedType.U4)> uType As Integer) As <MarshalAs(UnmanagedType.U4)> Integer
        End Function

#End Region

#Region "KERNEL32"

        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function CreateFile(lpFileName As String, <MarshalAs(UnmanagedType.U4)> dwDesiredAccess As FileAccess, <MarshalAs(UnmanagedType.U4)> dwShareMode As FileShare, lpSecurityAttributes As IntPtr, <MarshalAs(UnmanagedType.U4)> dwCreationDisposition As FileMode, <MarshalAs(UnmanagedType.U4)> dwFlagsAndAttributes As FileAttributes, hTemplateFile As IntPtr) As IntPtr
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Public Function CloseHandle(ByVal hObject As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
        Public Function GetDiskFreeSpace(lpRootPathName As String, ByRef lpSectorsPerCluster As UInteger, ByRef lpBytesPerSector As UInteger, ByRef lpNumberOfFreeClusters As UInteger, ByRef lpTotalNumberOfClusters As UInteger) As Boolean
        End Function

        <DllImport("Kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
        Public Function GetVolumeInformation(RootPathName As String, VolumeNameBuffer As StringBuilder, VolumeNameSize As Integer, ByRef VolumeSerialNumber As UInteger, ByRef MaximumComponentLength As UInteger, ByRef FileSystemFlags As FileSystemFeature, FileSystemNameBuffer As StringBuilder, nFileSystemNameSize As Integer) As Boolean
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Function GetFileTime(ByVal hFile As IntPtr, <Out()> ByRef lpCreationTime As Long, <Out()> ByRef lpLastAccessTime As Long, <Out()> ByRef lpLastWriteTime As Long) As Boolean
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Function SetFileTime(ByVal hFile As IntPtr, ByRef lpCreationTime As Long, ByRef lpLastAccessTime As Long, ByRef lpLastWriteTime As Long) As Boolean
        End Function

        <DllImport("kernel32.dll")> _
        Public Function QueryDosDevice(lpDeviceName As String, lpTargetPath As IntPtr, ucchMax As UInteger) As UInteger
        End Function

        'Public Declare Auto Function QueryDosDevice Lib "kernel32.dll" (ByVal lpDeviceName As String, ByRef lpTargetPath As String, ByVal ucchMax As UInteger) As UInteger
        'Public Declare Auto Function GetDiskFreeSpace Lib "kernel32.dll" (ByVal lpRootPathName As String, ByRef lpSectorsPerCluster As UInt32, ByRef lpBytesPerSector As UInt32, ByRef lpNumberOfFreeClusters As UInt32, ByRef lpTotalNumberOfClusters As UInt32) As Integer
        'Public Declare Auto Function GetVolumeInformation Lib "kernel32.dll" (ByVal RootPathName As String, ByVal VolumeNameBuffer As StringBuilder, ByVal VolumeNameSize As UInt32, ByRef VolumeSerialNumber As UInt32, ByRef MaximumComponentLength As UInt32, ByRef FileSystemFlags As UInt32, ByVal FileSystemNameBuffer As StringBuilder, ByVal FileSystemNameSize As UInt32) As UInt32

#Region "DeviceIoControl"
        <DllImport("kernel32.dll", ExactSpelling:=True, SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControl(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As IntPtr, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As IntPtr, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", EntryPoint:="DeviceIoControl", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControlMount(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As MOUNT_STRUCT, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As MOUNT_STRUCT, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", EntryPoint:="DeviceIoControl", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControlUnmount(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As UNMOUNT_STRUCT, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As UNMOUNT_STRUCT, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", EntryPoint:="DeviceIoControl", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControlListMounted(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As MOUNT_LIST_STRUCT, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As MOUNT_LIST_STRUCT, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", EntryPoint:="DeviceIoControl", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControlOpenTest(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As OPEN_TEST_STRUCT, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As OPEN_TEST_STRUCT, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll", EntryPoint:="DeviceIoControl", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Public Function DeviceIoControlVolProp(ByVal hDevice As IntPtr, ByVal dwIoControlCode As TC_IOCTL, ByVal lpInBuffer As VOLUME_PROPERTIES_STRUCT, ByVal nInBufferSize As UInteger, ByRef lpOutBuffer As VOLUME_PROPERTIES_STRUCT, ByVal nOutBufferSize As UInteger, ByRef lpBytesReturned As UInteger, ByVal lpOverlapped As IntPtr) As Boolean
        End Function
#End Region

#End Region

#Region "ADVAPI32"
        <DllImport("advapi32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Function OpenSCManager(ByVal machineName As String, ByVal databaseName As String, ByVal desiredAccess As Int32) As IntPtr
        End Function

        <DllImport("advapi32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Function CreateService(ByVal hSCManager As IntPtr, ByVal serviceName As String, ByVal displayName As String, ByVal desiredAccess As Int32, ByVal serviceType As Int32, ByVal startType As Int32, ByVal errorcontrol As Int32, ByVal binaryPathName As String, ByVal loadOrderGroup As String, ByVal TagBY As Int32, ByVal dependencides As String, ByVal serviceStartName As String, ByVal password As String) As IntPtr
        End Function

        <DllImport("advapi32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Public Function OpenService(ByVal hSCManager As IntPtr, ByVal lpServiceName As String, ByVal dwDesiredAccess As Int32) As IntPtr
        End Function

        <DllImport("advapi32", SetLastError:=True)> _
        Public Function StartService(hService As IntPtr, dwNumServiceArgs As Integer, lpServiceArgVectors As String()) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("advapi32.dll", SetLastError:=True)> _
        Public Function ControlService(ByVal hService As IntPtr, ByVal dwControl As SERVICE_CONTROL, ByRef lpServiceStatus As SERVICE_STATE) As Boolean
        End Function

        <DllImport("advapi32.dll", SetLastError:=True)>
        Public Function CloseServiceHandle(ByVal serviceHandle As IntPtr) As Boolean
        End Function

        <DllImport("advapi32.dll", SetLastError:=True)>
        Public Function DeleteService(ByVal hService As IntPtr) As Boolean
        End Function

        'Declare Function StartService Lib "advapi32.dll" Alias "StartServiceA" (ByVal hService As IntPtr, ByVal dwNumServiceArgs As Long, ByVal lpServiceArgVectors As String) As Boolean

#End Region

    End Module
End Namespace