Imports System.Runtime.InteropServices

Namespace Driver
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure MOUNT_STRUCT
        ''' <summary>
        ''' Return code back from driver
        ''' </summary>
        Public ReturnCode As ErrorCodes

        Public IsFileSystemDirty As Boolean
        Public VolumeMountedReadOnlyAfterAccessDenied As Boolean
        Public VolumeMountedReadOnlyAfterDeviceWriteProtected As Boolean

        ''' <summary>
        ''' TC_MAX_PATH (260) Volume to be mounted
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Public VolumePath() As Char

        ''' <summary>
        ''' User password
        ''' </summary>
        Public VolumePassword As PASSWORD_STUCT

        ''' <summary>
        ''' Cache passwords in driver
        ''' </summary>
        Public Cache As Boolean

        ''' <summary>
        ''' Drive number to mount
        ''' </summary>
        Public DosDriveNumber As Integer

        Public BytesPerSector As UInteger

        ''' <summary>
        ''' Mount volume in read-only mode
        ''' </summary>
        Public MountReadOnly As Boolean

        ''' <summary>
        ''' Mount volume as removable media
        ''' </summary>
        Public MountRemovable As Boolean

        ''' <summary>
        ''' Open host file/device in exclusive access mode
        ''' </summary>
        Public ExclusiveAccess As Boolean

        ''' <summary>
        ''' Annunce volume to mount manager
        ''' </summary>
        Public MountManager As Boolean

        ''' <summary>
        ''' Preserve file container timestamp
        ''' </summary>
        Public PreserveTimestamp As Boolean

        ''' <summary>
        ''' If TRUE, we are to attempt to mount a partition located on an encrypted system drive without pre-boot authentication
        ''' </summary>
        Public PartitionInInactiveSysEncScope As Boolean

        ''' <summary>
        ''' If TRUE, this contains the drive number of the system drive on which the partition is located
        ''' </summary>
        Public PartitionInInactiveSysEncScopeDriveNo As Integer

        Public SystemFavorite As Boolean

        'Hidden volume protection

        ''' <summary>
        ''' TRUE if the user wants the hidden volume within this volume to be protected against being overwritten (damaged)
        ''' </summary>
        Public ProtectHiddenVolume As Boolean

        ''' <summary>
        ''' Password to the hidden volume to be protected against overwriting
        ''' </summary>
        Public ProtectedHidVolPassword As PASSWORD_STUCT

        Public UseBackupHeader As Boolean
        Public RecoveryMode As Boolean
    End Structure
End Namespace