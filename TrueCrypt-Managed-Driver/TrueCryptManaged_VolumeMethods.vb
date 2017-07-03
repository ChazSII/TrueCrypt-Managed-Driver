Imports System.IO
Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Common
Imports CS2_Software.TrueCryptManagedDriver.Driver
Imports CS2_Software.TrueCryptManagedDriver.Common.Native

Partial Public Class TrueCryptManaged

    Private Function MountVolume(driveNo As Integer,
                                 volumePath As String,
                                 password As Security.Password,
                                 cachePassword As Boolean,
                                 sharedAccess As Boolean,
                                 mountOption As MOUNT_OPTIONS,
                                 quiet As Boolean) As ErrorCodes

        Dim dwResult As UInteger
        Dim bDevice As Boolean

        Dim ioCtrlResult As Boolean

        If IsMountedVolume(volumePath) Then Return ErrorCodes.VOL_ALREADY_MOUNTED
        If Not VolumePathExists(volumePath) Then Return ErrorCodes.FILES_OPEN

        Dim mount As New MOUNT_STRUCT With {
            .VolumePassword = New PASSWORD_STUCT,
            .ProtectedHidVolPassword = New PASSWORD_STUCT,
            .ExclusiveAccess = Not sharedAccess,
            .SystemFavorite = False,
            .UseBackupHeader = mountOption.UseBackupHeader,
            .RecoveryMode = mountOption.RecoveryMode
        }

retry:
        mount.DosDriveNumber = driveNo
        mount.Cache = cachePassword

        mount.PartitionInInactiveSysEncScope = False

        If password.KeyFiles.Count > 0 AndAlso Not password.TryApplyKeyFiles() Then Return ErrorCodes.PASSWORD_WRONG

        If password.Text.Length() > 0 Then
            mount.VolumePassword = New PASSWORD_STUCT With {
                .Text = password.Text.PadRight(MAX_PASSWORD + 1, Chr(0)),
                .Length = password.Text.Length(),
                .Pad = "".PadRight(3, Chr(0))
            }
        Else
            mount.VolumePassword = New PASSWORD_STUCT With {
                .Text = "".PadRight(MAX_PASSWORD + 1, Chr(0)),
                .Length = 0,
                .Pad = "".PadRight(3, Chr(0))
            }
        End If

        '' Not sure what this does yet
        'If (Not mountOption.ReadOnly) And mountOption.ProtectHiddenVolume Then
        '    mount.ProtectedHidVolPassword = New PASSWORD_STUCT With {
        '        .Pad = "".PadRight(3, Chr(0))
        '    }

        '    '*'MountOption.ProtectedHidVolPassword.ApplyKeyFile(mount.ProtectedHidVolPassword.Text)

        '    mount.ProtectedHidVolPassword.Length = StringLen(mount.ProtectedHidVolPassword.Text)

        '    mount.ProtectHiddenVolume = True
        'Else
        mount.ProtectedHidVolPassword = New PASSWORD_STUCT With {
                .Length = 0,
                .Text = "".PadRight(MAX_PASSWORD + 1, Chr(0)),
                .Pad = "".PadRight(3, Chr(0))
            }

        mount.ProtectHiddenVolume = False
        'End If

        mount.MountReadOnly = mountOption.ReadOnly
        mount.MountRemovable = mountOption.Removable
        mount.PreserveTimestamp = mountOption.PreserveTimestamp

        mount.MountManager = True

        If volumePath.Contains("\\?\") Then volumePath = volumePath.Substring(4)

        If volumePath.Contains("Volume{") And volumePath.LastIndexOf("}\") = volumePath.Length - 2 Then
            Dim resolvedPath As String = VolumeGuidPathToDevicePath(volumePath)

            If Not resolvedPath = "" Then volumePath = resolvedPath
        End If

        mount.VolumePath = volumePath.PadRight(TC_MAX_PATH, Chr(0))

        '' Not sure what this does yet
        'If Not bDevice Then
        '    'UNC
        '    If volumePath.StartsWith("\\") Then
        '        'Bla bla
        '    End If

        '    Dim bps As UInteger, flags As UInteger, d As UInteger
        '    If GetDiskFreeSpace(Path.GetPathRoot(volumePath), d, bps, d, d) Then
        '        mount.BytesPerSector = bps
        '    End If

        '    If (Not mount.MountReadOnly) And GetVolumeInformation(Path.GetPathRoot(volumePath), Nothing, 0, Nothing, d, flags, Nothing, 0) Then
        '        mount.MountReadOnly = Not (flags And FILE_READ_ONLY_VOLUME) = 0
        '    End If
        'End If

        ioCtrlResult = DeviceIoControlMount(ManagedDriver.DriverHandle, IoCtrlCodes.MOUNT_VOLUME, mount, Marshal.SizeOf(mount), mount, Marshal.SizeOf(mount), dwResult, Nothing)

        mount.VolumePassword = Nothing
        mount.ProtectedHidVolPassword = Nothing

        If Not ioCtrlResult Then
            If Marshal.GetLastWin32Error = SystemErrors.SHARING_VIOLATION Then
                'TODO

                If Not mount.ExclusiveAccess Then
                    Return ErrorCodes.FILES_OPEN_LOCK
                Else
                    mount.ExclusiveAccess = False
                    GoTo retry
                End If

                Return ErrorCodes.ACCESS_DENIED
            End If

            Return ErrorCodes.GENERIC
        End If

        If Not mount.ReturnCode = 0 Then Return mount.ReturnCode

        'Mount successful
        BroadcastDeviceChange(DBT_DEVICE.ARRIVAL, driveNo, 0)

        If Not mount.ExclusiveAccess Then Return ErrorCodes.OUTOFMEMORY

        Return mount.ReturnCode
    End Function

    Private Function UnmountVolume(dosDriveNumber As Integer, forceUnmount As Boolean) As ErrorCodes
        Dim result As ErrorCodes
        Dim forced As Boolean = forceUnmount
        Dim dismountMaxRetries As Integer = 30

retry:
        BroadcastDeviceChange(DBT_DEVICE.REMOVEPENDING, dosDriveNumber, 0)

        Do
            result = DriverUnmountVolume(dosDriveNumber, forced)

            If result = ErrorCodes.FILES_OPEN Then 'ERR_FILES_OPEN
                Threading.Thread.Sleep(50)
            Else
                Exit Do
            End If

            dismountMaxRetries -= 1
        Loop While (dismountMaxRetries > 0)

        If Not result = 0 Then
            Return result
        End If

        BroadcastDeviceChange(DBT_DEVICE.REMOVECOMPLETE, dosDriveNumber, 0)

        Return result
    End Function


    Private Function DriverUnmountVolume(dosDriveNumber As Integer, forceUnmount As Boolean) As ErrorCodes
        Dim unmount As UNMOUNT_STRUCT
        Dim dwResult As UInteger
        Dim ioCtrlResult As Boolean

        unmount = New UNMOUNT_STRUCT With {
            .nDosDriveNo = dosDriveNumber,
            .ignoreOpenFiles = forceUnmount
        }

        ioCtrlResult = DeviceIoControlUnmount(ManagedDriver.DriverHandle, IoCtrlCodes.UNMOUNT_VOLUME, unmount, Marshal.SizeOf(unmount), unmount, Marshal.SizeOf(unmount), dwResult, Nothing)

        If Not ioCtrlResult Then Return ErrorCodes.OS_ERROR

        Return unmount.nReturnCode
    End Function


    Private Function GetMountedVolume(ByRef mountList As MOUNT_LIST_STRUCT) As Boolean
        Dim dwResult As UInteger

        mountList = New MOUNT_LIST_STRUCT

        Return DeviceIoControlListMounted(ManagedDriver.DriverHandle, IoCtrlCodes.GET_MOUNTED_VOLUMES, mountList, Marshal.SizeOf(mountList), mountList, Marshal.SizeOf(mountList), dwResult, Nothing)
    End Function

    Private Function IsMountedVolume(ByVal volname As String) As Boolean
        Dim mlist As MOUNT_LIST_STRUCT
        Dim dwResult As UInteger
        Dim i As Integer
        Dim volume As String

        volume = volname

        If Not volume.StartsWith("\Device\") Then volume = "\??\" & volume

        Dim resolvedPath As String = VolumeGuidPathToDevicePath(volname)
        If Not resolvedPath = "" Then volume = resolvedPath

        mlist = New MOUNT_LIST_STRUCT

        DeviceIoControlListMounted(ManagedDriver.DriverHandle, IoCtrlCodes.GET_MOUNTED_VOLUMES, mlist, Marshal.SizeOf(mlist), mlist, Marshal.SizeOf(mlist), dwResult, Nothing)

        For i = 0 To 25
            If mlist.wszVolume(i).wszVolume = volume Then Return True
        Next

        Return False
    End Function

    Private Function VolumeGuidPathToDevicePath(ByVal volumeGuidPath As String) As String
        If Not volumeGuidPath.StartsWith("\\?\") Then volumeGuidPath = volumeGuidPath.Substring(4)

        If volumeGuidPath.Contains("Volume{") Or Not volumeGuidPath.LastIndexOf("}\") = volumeGuidPath.Length - 2 Then Return ""

        Dim volDevPath As String = ""
        If Not QueryDosDevice(volumeGuidPath.Substring(0, volumeGuidPath.Length - 1), volDevPath, TC_MAX_PATH) Then Return ""

        'Dim partitionPath As String = HarddiskVolumePathToPartitionPath(volDevPath)

        'Return If(partitionPath = "", volDevPath, partitionPath)

        Return volDevPath
    End Function

    Private Function VolumePathExists(ByVal volumePath As String) As Boolean
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

    Private Function OpenDevice(lpszPath As String, ByRef driver As OPEN_TEST_STRUCT, detectFilesystem As Boolean) As Boolean
        Dim dwResult As UInteger, bResult As Boolean

        driver = New OPEN_TEST_STRUCT With {
            .wszFileName = lpszPath,
            .bDetectTCBootLoader = False,
            .DetectFilesystem = detectFilesystem
        }

        bResult = DeviceIoControlOpenTest(ManagedDriver.DriverHandle, IoCtrlCodes.OPEN_TEST,
                                          driver, Marshal.SizeOf(driver),
                                          driver, Marshal.SizeOf(driver),
                                          dwResult, Nothing)

        If Not bResult Then
            dwResult = Marshal.GetLastWin32Error

            If dwResult = SystemErrors.SHARING_VIOLATION Or dwResult = SystemErrors.NOT_READY Then
                driver.TCBootLoaderDetected = False
                driver.TCBootLoaderDetected = False

                Return True
            Else
                Return False
            End If
        End If

        Return True
    End Function


    Private Sub BroadcastDeviceChange(message As UInteger, nDosDriveNo As Integer, driveMap As UInteger)
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

        SendMessageTimeout(HWND_BROADCAST, WM_DEVICECHANGE, message, dbv.ToPointer, SMTO_ABORTIFHUNG, timeOut, dwResult)
    End Sub

    'Friend Function GetModeOfOperationByDriveNo(dosDriveNo As Integer) As Integer
    '    Dim dwResult As UInteger

    '    Dim prop As New VOLUME_PROPERTIES_STRUCT With {
    '        .driveNo = dosDriveNo
    '    }

    '    If DeviceIoControlVolProp(ManagedDriver.DriverHandle, IoCtrlCodes.GET_VOLUME_PROPERTIES, prop, Marshal.SizeOf(prop), prop, Marshal.SizeOf(prop), dwResult, Nothing) Then
    '        Return prop.mode
    '    End If

    '    Return 0
    'End Function

End Class