Imports System.Runtime.InteropServices
Imports TrueCryptDriver.Common.Enums

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure MOUNT_STRUCT
            Dim nReturnCode As TC_ERROR 'Return code back from driver
            Dim FilesystemDirty As Boolean
            Dim VolumeMountedReadOnlyAfterAccessDenied As Boolean
            Dim VolumeMountedReadOnlyAfterDeviceWriteProtected As Boolean

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim wszVolume() As Char 'TC_MAX_PATH (260) Volume to be mounted
            Dim VolumePassword As PASSWORD_STUCT 'User password
            Dim bCache As Boolean 'Cache passwords in driver
            Dim nDosDriveNo As Integer 'Drive number to mount
            Dim BytesPerSector As UInteger
            Dim bMountReadOnly As Boolean 'Mount volume in read-only mode
            Dim bMountRemovable As Boolean 'Mount volume as removable media
            Dim bExclusiveAccess As Boolean 'Open host file/device in exclusive access mode
            Dim bMountManager As Boolean 'Annunce volume to mount manager
            Dim bPreserveTimestamp As Boolean 'Preserve file container timestamp
            Dim bPartitionInInactiveSysEncScope As Boolean 'If TRUE, we are to attempt to mount a partition located on an encrypted system drive without pre-boot authentication.
            Dim nPartitionInInactiveSysEncScopeDriveNo As Integer 'If bPartitionInInactiveSysEncScope is TRUE, this contains the drive number of the system drive on which the partition is located.
            Dim SystemFavorite As Boolean

            'Hidden volume protection
            Dim bProtectHiddenVolume As Boolean 'TRUE if the user wants the hidden volume within this volume to be protected against being overwritten (damaged)
            Dim ProtectedHidVolPassword As PASSWORD_STUCT 'Password to the hidden volume to be protected against overwriting
            Dim UseBackupHeader As Boolean
            Dim RecoveryMode As Boolean
        End Structure
    End Namespace
End Namespace