Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports TrueCryptDriver.Common
Imports TrueCryptDriver.Common.Enums
Imports TrueCryptDriver.Common.Structures
Imports TrueCryptDriver.Driver.Constants
Imports TrueCryptDriver.Driver.Enums
Imports TrueCryptDriver.Driver.Structures

Partial Public Class TC_Driver

    Friend Function MountVolume(ByVal driveNo As Integer, ByVal volumePath As String, ByVal password As String, ByVal cachePassword As Boolean, ByVal sharedAccess As Boolean, ByRef MountOption As MOUNT_OPTIONS, ByVal quiet As Boolean) As TC_ERROR
        Dim mount As MOUNT_STRUCT
        Dim dwResult As UInteger
        Dim bResult As Boolean, bDevice As Boolean
        Dim favoriteMountOnArrivalRetryCount As Integer = 0

        If IsMountedVolume(volumePath) Then Return TC_ERROR.VOL_ALREADY_MOUNTED
        If Not VolumePathExists(volumePath) Then Return TC_ERROR.FILES_OPEN

        mount = New MOUNT_STRUCT
        mount.VolumePassword = New PASSWORD_STUCT
        mount.ProtectedHidVolPassword = New PASSWORD_STUCT

        mount.bExclusiveAccess = Not sharedAccess
        mount.SystemFavorite = False
        mount.UseBackupHeader = MountOption.UseBackupHeader
        mount.RecoveryMode = MountOption.RecoveryMode

retry:
        mount.nDosDriveNo = driveNo
        mount.bCache = cachePassword

        mount.bPartitionInInactiveSysEncScope = False

        If StringLen(password) > 0 Then
            mount.VolumePassword = New PASSWORD_STUCT

            mount.VolumePassword.Text = password.PadRight(MAX_PASSWORD + 1, Chr(0))
            mount.VolumePassword.Length = StringLen(password)
            mount.VolumePassword.Pad = "".PadRight(3, Chr(0))
        Else
            mount.VolumePassword = New PASSWORD_STUCT

            mount.VolumePassword.Text = "".PadRight(MAX_PASSWORD + 1, Chr(0))
            mount.VolumePassword.Length = 0
            mount.VolumePassword.Pad = "".PadRight(3, Chr(0))
        End If

        If (Not MountOption.ReadOnly) And MountOption.ProtectHiddenVolume Then
            mount.ProtectedHidVolPassword = New PASSWORD_STUCT

            mount.ProtectedHidVolPassword.Pad = "".PadRight(3, Chr(0))

            MountOption.ProtectedHidVolPassword.ApplyKeyFile(mount.ProtectedHidVolPassword.Text)
            mount.ProtectedHidVolPassword.Length = StringLen(mount.ProtectedHidVolPassword.Text)

            mount.bProtectHiddenVolume = True
        Else
            mount.ProtectedHidVolPassword = New PASSWORD_STUCT
            mount.ProtectedHidVolPassword.Length = 0
            mount.ProtectedHidVolPassword.Text = "".PadRight(MAX_PASSWORD + 1, Chr(0))
            mount.ProtectedHidVolPassword.Pad = "".PadRight(3, Chr(0))

            mount.bProtectHiddenVolume = False
        End If

        mount.bMountReadOnly = MountOption.ReadOnly
        mount.bMountRemovable = MountOption.Removable
        mount.bPreserveTimestamp = MountOption.PreserveTimestamp

        mount.bMountManager = True

        If volumePath.Contains("\\?\") Then volumePath = volumePath.Substring(4)

        If volumePath.Contains("Volume{") And volumePath.LastIndexOf("}\") = volumePath.Length - 2 Then
            Dim resolvedPath As String = VolumeGuidPathToDevicePath(volumePath)

            If Not resolvedPath = "" Then volumePath = resolvedPath
        End If

        mount.wszVolume = volumePath.PadRight(TC_MAX_PATH, Chr(0))

        If Not bDevice Then
            'UNC
            If volumePath.StartsWith("\\") Then
                'Bla bla
            End If

            Dim bps As UInteger, flags As UInteger, d As UInteger
            If GetDiskFreeSpace(Path.GetPathRoot(volumePath), d, bps, d, d) Then
                mount.BytesPerSector = bps
            End If

            If (Not mount.bMountReadOnly) And GetVolumeInformation(Path.GetPathRoot(volumePath), Nothing, 0, Nothing, d, flags, Nothing, 0) Then
                mount.bMountReadOnly = Not (flags And FILE_READ_ONLY_VOLUME) = 0
            End If
        End If

        bResult = DeviceIoControlMount(ManagedDriver.hDriver, TC_IOCTL.MOUNT_VOLUME, mount, Marshal.SizeOf(mount), mount, Marshal.SizeOf(mount), dwResult, Nothing)

        mount.VolumePassword = Nothing
        mount.ProtectedHidVolPassword = Nothing

        If Not bResult Then
            If Marshal.GetLastWin32Error = SYSTEM_ERROR.SHARING_VIOLATION Then
                'TODO

                If Not mount.bExclusiveAccess Then
                    Return TC_ERROR.FILES_OPEN_LOCK
                Else
                    mount.bExclusiveAccess = False
                    GoTo retry
                End If

                Return TC_ERROR.ACCESS_DENIED
            End If

            Return TC_ERROR.GENERIC
        End If

        If Not mount.nReturnCode = 0 Then Return mount.nReturnCode

        'Mount successful
        BroadcastDeviceChange(DBT_DEVICE.ARRIVAL, driveNo, 0)

        If Not mount.bExclusiveAccess Then Return TC_ERROR.OUTOFMEMORY

        Return mount.nReturnCode
    End Function


    Friend Function UnmountVolume(ByVal nDosDriveNo As Integer, ByVal forceUnmount As Boolean) As TC_ERROR
        Dim result As TC_ERROR
        Dim forced As Boolean = forceUnmount
        Dim dismountMaxRetries As Integer = 30

retry:
        BroadcastDeviceChange(DBT_DEVICE.REMOVEPENDING, nDosDriveNo, 0)

        Do
            result = DriverUnmountVolume(nDosDriveNo, forced)

            If result = TC_ERROR.FILES_OPEN Then 'ERR_FILES_OPEN
                Threading.Thread.Sleep(50)
            Else
                Exit Do
            End If

            dismountMaxRetries -= 1
        Loop While (dismountMaxRetries > 0)

        If Not result = 0 Then
            Return result
        End If

        BroadcastDeviceChange(DBT_DEVICE.REMOVECOMPLETE, nDosDriveNo, 0)

        Return result
    End Function

    Friend Function DriverUnmountVolume(ByVal nDosDriveNo As Integer, ByVal forced As Boolean) As TC_ERROR
        Dim unmount As UNMOUNT_STRUCT
        Dim dwResult As UInteger
        Dim bResult As Boolean

        unmount = New UNMOUNT_STRUCT
        unmount.nDosDriveNo = nDosDriveNo
        unmount.ignoreOpenFiles = forced

        bResult = DeviceIoControlUnmount(ManagedDriver.hDriver, TC_IOCTL.UNMOUNT_VOLUME, unmount, Marshal.SizeOf(unmount), unmount, Marshal.SizeOf(unmount), dwResult, Nothing)

        If Not bResult Then Return TC_ERROR.OS_ERROR

        Return unmount.nReturnCode
    End Function

    Friend Function GetMountedVolume(ByRef mountList As MOUNT_LIST_STRUCT) As Boolean
        Dim dwResult As UInteger

        mountList = New MOUNT_LIST_STRUCT

        Return DeviceIoControlListMounted(ManagedDriver.hDriver, TC_IOCTL.GET_MOUNTED_VOLUMES, mountList, Marshal.SizeOf(mountList), mountList, Marshal.SizeOf(mountList), dwResult, Nothing)
    End Function


    Friend Function IsMountedVolume(ByVal volname As String) As Boolean
        Dim mlist As MOUNT_LIST_STRUCT
        Dim dwResult As UInteger
        Dim i As Integer
        Dim volume As String

        volume = volname

        If Not volume.StartsWith("\Device\") Then volume = "\??\" & volume

        Dim resolvedPath As String = VolumeGuidPathToDevicePath(volname)
        If Not resolvedPath = "" Then volume = resolvedPath

        mlist = New MOUNT_LIST_STRUCT

        DeviceIoControlListMounted(ManagedDriver.hDriver, TC_IOCTL.GET_MOUNTED_VOLUMES, mlist, Marshal.SizeOf(mlist), mlist, Marshal.SizeOf(mlist), dwResult, Nothing)

        For i = 0 To 25
            If mlist.wszVolume(i).wszVolume = volume Then Return True
        Next

        Return False
    End Function

    Friend Function VolumeGuidPathToDevicePath(ByVal volumeGuidPath As String) As String
        If Not volumeGuidPath.StartsWith("\\?\") Then volumeGuidPath = volumeGuidPath.Substring(4)

        If volumeGuidPath.Contains("Volume{") Or Not volumeGuidPath.LastIndexOf("}\") = volumeGuidPath.Length - 2 Then Return ""

        Dim volDevPath As String = ""
        If Not QueryDosDevice(volumeGuidPath.Substring(0, volumeGuidPath.Length - 1), volDevPath, TC_MAX_PATH) Then Return ""

        'Dim partitionPath As String = HarddiskVolumePathToPartitionPath(volDevPath)

        'Return If(partitionPath = "", volDevPath, partitionPath)

        Return volDevPath
    End Function

    Friend Function VolumePathExists(ByVal volumePath As String) As Boolean
        Dim openTest As New OPEN_TEST_STRUCT
        Dim upperCasePath As String, devicePath As String = ""
        Dim hFile As IntPtr

        upperCasePath = volumePath.ToUpper

        If upperCasePath.Contains("\DEVICE\") Then Return OpenDevice(volumePath, openTest, False)

        If volumePath.Contains("\\?\Volume{") And volumePath.LastIndexOf("}\") = volumePath.Length - 2 Then
            If Not QueryDosDevice(volumePath.Substring(4, volumePath.Length - 5), devicePath, TC_MAX_PATH) = 0 Then Return True
        End If

        hFile = CreateFile(volumePath, FileAccess.Read, FileShare.ReadWrite, Nothing, FileMode.Open, OPEN_EXISTING, Nothing)

        If hFile = INVALID_HANDLE_VALUE Or hFile = IntPtr.Zero Then
            Return False
        Else
            CloseHandle(hFile)

            Return True
        End If
    End Function

    Friend Function OpenDevice(ByVal lpszPath As String, ByRef driver As OPEN_TEST_STRUCT, ByVal detectFilesystem As Boolean) As Boolean
        Dim dwResult As UInteger, bResult As Boolean

        driver = New OPEN_TEST_STRUCT

        driver.wszFileName = lpszPath
        driver.bDetectTCBootLoader = False
        driver.DetectFilesystem = detectFilesystem

        bResult = DeviceIoControlOpenTest(ManagedDriver.hDriver, TC_IOCTL.OPEN_TEST,
                                          driver, Marshal.SizeOf(driver),
                                          driver, Marshal.SizeOf(driver),
                                          dwResult, Nothing)

        If Not bResult Then
            dwResult = Marshal.GetLastWin32Error

            If dwResult = SYSTEM_ERROR.SHARING_VIOLATION Or dwResult = SYSTEM_ERROR.NOT_READY Then
                driver.TCBootLoaderDetected = False
                driver.TCBootLoaderDetected = False

                Return True
            Else
                Return False
            End If
        End If

        Return True
    End Function


    Friend Sub BroadcastDeviceChange(message As UInteger, nDosDriveNo As Integer, driveMap As UInteger)
        Dim dbv As DEV_BROADCAST_VOLUME
        Dim dwResult As IntPtr
        Dim eventId As Int32
        Dim i As Integer

        If message = DBT_DEVICE.ARRIVAL Then
            eventId = SHCNE_DRIVERADD
        ElseIf message = DBT_DEVICE.REMOVECOMPLETE Then
            eventId = SHCNE_DRIVERREMOVED
        ElseIf Environment.OSVersion.Version >= New Version("6.1") And message = DBT_DEVICE.REMOVEPENDING Then
            eventId = SHCNE_DRIVERREMOVED
        End If

        If driveMap = 0 Then driveMap = (1 << nDosDriveNo)

        If Not eventId = 0 Then
            For i = 0 To 25
                If driveMap And (1 << i) Then
                    Dim root As String = Chr(i + Asc("A")) & ":\"

                    SHChangeNotify(eventId, SHCNF_PATH, root, Nothing)

                    Exit For
                End If
            Next
        End If

        dbv = New DEV_BROADCAST_VOLUME
        dbv.dbcv_size = Marshal.SizeOf(dbv)
        dbv.dbcv_devicetype = DBT_DEVTYP_VOLUME
        dbv.dbcv_reserved = 0
        dbv.dbcv_unitmask = driveMap
        dbv.dbcv_flags = 0

        Dim timeOut As UInteger = 1000

        If Environment.OSVersion.Version.Major >= 6 Then timeOut = 100

        SendMessageTimeout(HWND_BROADCAST, WM_DEVICECHANGE, message, dbv.StructureToPointer, SMTO_ABORTIFHUNG, timeOut, dwResult)
    End Sub

    'Friend Function GetModeOfOperationByDriveNo(ByVal nDosDriveNo As Integer) As Integer
    '    Dim prop As VOLUME_PROPERTIES_STRUCT, dwResult As UInteger

    '    prop = New VOLUME_PROPERTIES_STRUCT
    '    prop.driveNo = nDosDriveNo

    '    If DeviceIoControlVolProp(Driver.hDriver, TC_IOCTL.GET_VOLUME_PROPERTIES, prop, Marshal.SizeOf(prop), prop, Marshal.SizeOf(prop), dwResult, Nothing) Then
    '        Return prop.mode
    '    End If

    '    Return 0
    'End Function

End Class