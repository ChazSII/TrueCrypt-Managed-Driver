Imports System.Runtime.InteropServices
Imports TrueCryptDriver.Common.Enums
Imports TrueCryptDriver.Common.Structures

Public Module TC_Driver_Declarations

    Public hDriver As IntPtr

#Region "Driver Constants"

    'Path
    Public Const MAX_PASSWORD As Integer = 64
    Public Const TC_MAX_PATH As Integer = 260

    Public Const KEYFILE_POOL_SIZE As Integer = 64
    Public Const KEYFILE_MAX_READ_LEN As Integer = 1024 ^ 2

    'TC Prefix
    Public Const TC_UNIQUE_ID_PREFIX As String = "TrueCryptVolume"
    Public Const TC_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"

    Public Const NT_MOUNT_PREFIX As String = "\Device\TrueCryptVolume"
    Public Const NT_ROOT_PREFIX As String = "\Device\TrueCrypt"
    Public Const DOS_MOUNT_PREFIX As String = "\\DosDevices\"
    Public Const DOS_ROOT_PREFIX As String = "\DosDevices\TrueCrypt"
    Public Const WIN32_ROOT_PREFIX As String = "\\.\TrueCrypt"

    'Mutex
    Public Const TC_MUTEX_NAME_SYSENC As String = "Global\TrueCrypt System Encryption Wizard"
    Public Const TC_MUTEX_NAME_NONSYS_INPLACE_ENC As String = "Global\TrueCrypt In-Place Encryption Wizard"
    Public Const TC_MUTEX_NAME_APP_SETUP As String = "Global\TrueCrypt Setup"
    Public Const TC_MUTEX_NAME_DRIVER_SETUP As String = "Global\TrueCrypt Driver Setup"

    Public Const TC_DRIVER_CONFIG_REG_VALUE_NAME As String = "TrueCryptConfig"
    Public Const TC_ENCRYPTION_FREE_CPU_COUNT_REG_VALUE_NAME As String = "TrueCryptEncryptionFreeCpuCount"

    'WARNING: Modifying the following values can introduce incompatibility with previous versions.
    Public Const TC_DRIVER_CONFIG_CACHE_BOOT_PASSWORD As UInteger = &H1
    Public Const TC_DRIVER_CONFIG_CACHE_BOOT_PASSWORD_FOR_SYS_FAVORITES As UInteger = &H2
    Public Const TC_DRIVER_CONFIG_DISABLE_NONADMIN_SYS_FAVORITES_ACCESS As UInteger = &H4
    Public Const TC_DRIVER_CONFIG_DISABLE_HARDWARE_ENCRYPTION As UInteger = &H8

    'CreateFile API
    Public Const OPEN_EXISTING As Integer = 3

    'Handle
    Public Const INVALID_HANDLE_VALUE As Integer = -1

    'Services
    Public Const SC_MANAGER_ALL_ACCESS As Integer = &HF003F 'Service Control Manager object specific access types
    Public Const SERVICE_ALL_ACCESS As Integer = &HF01FF    'Service object specific access type
    Public Const SERVICE_KERNEL_DRIVER As Integer = &H1     'Service Type
    Public Const SERVICE_ERROR_NORMAL As Integer = &H1      'Error control type

    Public Const FILE_READ_ONLY_VOLUME As Integer = &H80000

    'Device Broadcast
    Public Const WM_DEVICECHANGE As Integer = &H219

    'Message for WM_DEVICECHANGE
    Public Const DBT_DEVICEARRIVAL As Integer = &H8000
    Public Const DBT_DEVICEQUERYREMOVE As Integer = &H8001
    Public Const DBT_DEVICEQURYREMOVEFAILED As Integer = &H8002
    Public Const DBT_DEVICEREMOVEPENDING As Integer = &H8003
    Public Const DBT_DEVICEREMOVECOMPLETE As Integer = &H8004
    Public Const DBT_DEVICETYPESPECIFIC As Integer = &H8005

    Public Const DBT_DEVTYP_VOLUME As Integer = &H2

    'File System Notification flags
    Public Const SHCNE_DRIVERREMOVED As Integer = &H80
    Public Const SHCNE_DRIVERADD As Integer = &H100

    'Flags
    Public Const SHCNF_PATH As Integer = &H1

    Public Const SMTO_ABORTIFHUNG As Integer = &H2

    'Destination
    Public Const HWND_BROADCAST As Integer = 65535

#End Region

#Region "Driver Enums"

    Public Enum BootEncryptionSetupMode As UInteger
        SetupNone = 0
        SetupEncryption
        SetupDecryption
    End Enum

    Public Enum TC_IOCTL As UInteger
        TC_IOCTL_GET_DRIVER_VERSION = 2236416 + 4 * 1 'TC_IOCTL(1)
        TC_IOCTL_GET_BOOT_LOADER_VERSION = 2236416 + 4 * 2 'TC_IOCTL(2)
        TC_IOCTL_MOUNT_VOLUME = 2236416 + 4 * 3 'TC_IOCTL(3)
        TC_IOCTL_UNMOUNT_VOLUME = 2236416 + 4 * 4 'TC_IOCTL(4)
        TC_IOCTL_UNMOUNT_ALL_VOLUMES = 2236416 + 4 * 5 'TC_IOCTL(5)
        TC_IOCTL_GET_MOUNTED_VOLUMES = 2236416 + 4 * 6 'TC_IOCTL(6)
        TC_IOCTL_GET_VOLUME_PROPERTIES = 2236416 + 4 * 7 'TC_IOCTL(7)
        TC_IOCTL_GET_DEVICE_REFCOUNT = 2236416 + 4 * 8 'TC_IOCTL(8)
        TC_IOCTL_IS_DRIVER_UNLOAD_DISABLED = 2236416 + 4 * 9 'TC_IOCTL(9)
        TC_IOCTL_IS_ANY_VOLUME_MOUNTED = 2236416 + 4 * 10 'TC_IOCTL(10)
        TC_IOCTL_GET_PASSWORD_CACHE_STATUS = 2236416 + 4 * 11 'TC_IOCTL(11)
        TC_IOCTL_WIPE_PASSWORD_CACHE = 2236416 + 4 * 12 'TC_IOCTL(12)
        TC_IOCTL_OPEN_TEST = 2236416 + 4 * 13 'TC_IOCTL(13)
        TC_IOCTL_GET_DRIVE_PARTITION_INFO = 2236416 + 4 * 14 'TC_IOCTL(14)
        TC_IOCTL_GET_DRIVE_GEOMETRY = 2236416 + 4 * 15 'TC_IOCTL(15)
        TC_IOCTL_PROBE_REAL_DRIVE_SIZE = 2236416 + 4 * 16 'TC_IOCTL(16)
        TC_IOCTL_GET_RESOLVED_SYMLINK = 2236416 + 4 * 17 'TC_IOCTL(17)
        TC_IOCTL_GET_BOOT_ENCRYPTION_STATUS = 2236416 + 4 * 18 'TC_IOCTL(18)
        TC_IOCTL_BOOT_ENCRYPTION_SETUP = 2236416 + 4 * 19 'TC_IOCTL(19)
        TC_IOCTL_ABORT_BOOT_ENCRYPTION_SETUP = 2236416 + 4 * 20 'TC_IOCTL(20)
        TC_IOCTL_GET_BOOT_ENCRYPTION_SETUP_RESULT = 2236416 + 4 * 21 'TC_IOCTL(21)
        TC_IOCTL_GET_BOOT_DRIVE_VOLUME_PROPERTIES = 2236416 + 4 * 22 'TC_IOCTL(22)
        TC_IOCTL_REOPEN_BOOT_VOLUME_HEADER = 2236416 + 4 * 23 'TC_IOCTL(23)
        TC_IOCTL_GET_BOOT_ENCRYPTION_ALGORITHM_NAME = 2236416 + 4 * 24 'TC_IOCTL(24)
        TC_IOCTL_GET_PORTABLE_MODE_STATUS = 2236416 + 4 * 25 'TC_IOCTL(25)
        TC_IOCTL_SET_PORTABLE_MODE_STATUS = 2236416 + 4 * 26 'TC_IOCTL(26)
        TC_IOCTL_IS_HIDDEN_SYSTEM_RUNNING = 2236416 + 4 * 27 'TC_IOCTL(27)
        TC_IOCTL_GET_SYSTEM_DRIVE_CONFIG = 2236416 + 4 * 28 'TC_IOCTL(28)
        TC_IOCTL_DISK_IS_WRITABLE = 2236416 + 4 * 29 'TC_IOCTL(29)
        TC_IOCTL_START_DECOY_SYSTEM_WIPE = 2236416 + 4 * 30 'TC_IOCTL(30)
        TC_IOCTL_ABORT_DECOY_SYSTEM_WIPE = 2236416 + 4 * 31 'TC_IOCTL(31)
        TC_IOCTL_GET_DECOY_SYSTEM_WIPE_STATUS = 2236416 + 4 * 32 'TC_IOCTL(32)
        TC_IOCTL_GET_DECOY_SYSTEM_WIPE_RESULT = 2236416 + 4 * 33 'TC_IOCTL(33)
        TC_IOCTL_WRITE_BOOT_DRIVE_SECTOR = 2236416 + 4 * 34 'TC_IOCTL(34)
        TC_IOCTL_GET_WARNING_FLAGS = 2236416 + 4 * 35 'TC_IOCTL(35)
        TC_IOCTL_SET_SYSTEM_FAVORITE_VOLUME_DIRTY = 2236416 + 4 * 36 'TC_IOCTL(36)
        TC_IOCTL_REREAD_DRIVER_CONFIG = 2236416 + 4 * 37 'TC_IOCTL(37)
        TC_IOCTL_GET_SYSTEM_DRIVE_DUMP_CONFIG = 2236416 + 4 * 38 'TC_IOCTL(38)

        'Legacy IOCTLs used before version 5.0
        TC_IOCTL_LEGACY_GET_DRIVER_VERSION = 466968
        TC_IOCTL_LEGACY_GET_MOUNTED_VOLUMES = 466948
    End Enum

#End Region

#Region "Driver Structures"

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure PASSWORD_STUCT
        Dim Lenght As UInteger '4 byte

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=65)>
        Dim Text() As Char 'MAX_PASSWORD 65 byte

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
        Dim Pad() As Char '3 byte
    End Structure

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

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure UNMOUNT_STRUCT
        Dim nDosDriveNo As Integer 'Drive letter to unmount
        Dim ignoreOpenFiles As Boolean
        Dim HiddenVolumeProtectionTriggered As Boolean
        Dim nReturnCode As TC_ERROR 'Return code back from driver
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure MOUNT_LIST_STRUCT
        Dim ulMountedDrives As UInteger 'Bitfield of all mounted drive letters

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
        Dim wszVolume() As MOUNT_LIST_NAME_STRUCT 'Volume names of mounted volumes

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
        Dim diskLength() As ULong

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
        Dim ea() As Integer

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
        Dim volumeType() As Integer 'Volume type (e.g. PROP_VOL_TYPE_OUTER, PROP_VOL_TYPE_OUTER_VOL_WRITE_PREVENTED, etc.)
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure MOUNT_LIST_NAME_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim wszVolume() As Char
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure VOLUME_PROPERTIES_STRUCT
        Dim driveNo As Integer
        Dim uniqueId As Integer

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim wszVolume() As Char
        Dim diskLenght As ULong
        Dim ea As Integer
        Dim mode As Integer
        Dim pkcs5 As Integer
        Dim pkcs5Iterations As Integer
        Dim hiddenVolume As Boolean
        Dim [readOnly] As Boolean
        Dim removable As Boolean
        Dim partitionInInactiveSysEncScope As Boolean
        Dim volumeHeaderFlags As UInteger
        Dim totalBytesRead As ULong
        Dim totalBytesWritten As ULong
        Dim hiddenVolProtection As Integer 'Hidden volume protection status (e.g. HIDVOL_PROT_STATUS_NONE, HIDVOL_PROT_STATUS_ACTIVE, etc.)
        Dim volFormatVersion As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure RESOLVE_SYMLINK_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim symLinkName() As Char

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim targetName() As Char
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure DISK_PARTITION_INFO_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim deviceName() As Char
        Dim partInfo() As PARTITION_INFORMATION
        Dim IsGPT As Boolean
        Dim IsDynamic As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure DISK_GEOMETRY_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim deviceName() As Char
        Dim diskGeometry As DISK_GEOMETRY
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure OPEN_TEST_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim wszFileName() As Char
        Dim bDetectTCBootLoader As Boolean
        Dim TCBootLoaderDetected As Boolean
        Dim DetectFilesystem As Boolean
        Dim FilesystemDetected As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure ProbeRealDriveSizeRequest
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim DeviceName() As Char
        Dim RealDriveSize As Long
        Dim TimeOut As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure BootEncryptionStatus
        'New fields must be added at the end of the structure to maintain compatibility with previous versions
        Dim DeviceFilterActive As Boolean

        Dim BootLoaderVersion As UShort

        Dim DriveMounted As Boolean
        Dim VolumeHeaderPresent As Boolean
        Dim DriveEncryptes As Boolean

        Dim BootDriveLength As Long

        Dim ConfiguredEncryptedAreaStart As Long
        Dim ConfiguredEncryptedAreaEnd As Long
        Dim EncryptedAreaStart As Long
        Dim EncryptedAreaEnd As Long

        Dim VolumeHeaderSaltCrc32 As UInteger

        Dim SetupInProgress As Boolean
        Dim SetupMode As BootEncryptionSetupMode
        Dim TransformWaitingForIdle As Boolean

        Dim HibernationPreventionCount As UInteger

        Dim HiddenSystem As Boolean
        Dim HiddenSystemPartitionStart As Long

        'Number of times the filter driver answered that an unencrypted volume
        'is read-only (or mounted an outer/normal TrueCrypt volume as read only)
        Dim HiddenSysLeakProtectionCount As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure ReopenBootVolumeHeaderRequest
        Dim VolumePassword As PASSWORD_STUCT
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure GetBootEncryptionAlgorithmNameRequest
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=255)>
        Dim BootEncryptionAlgorithmName() As Char
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure GetSystemDriveConfigurationRequest
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim DevicePath() As Char
        Dim Configuration As Byte
        Dim DriveIsDynamic As Boolean
        Dim BootLoaderVersion As UShort
        Dim UserConfiguration As Byte

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=25)>
        Dim CustomUserMessage() As Char
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure WriteBootDriveSectorRequest
        Dim Offset As Long

        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=512)>
        Dim Data() As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure GetWarningFlagsRequest
        Dim PagingFileCreationPrevented As Boolean
        Dim SystemFavoriteVolumeDirty As Boolean
    End Structure

    '<StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    'Public Structure BootEncryptionSetupRequest
    '    Dim SetupMode As BootEncryptionSetupMode
    '    Dim WipeAlgorithm As WipeAlgorithmId
    '    Dim ZeroUnreadableSectors As Boolean
    '    Dim DiscardUnreadableEncryptedSectors As Boolean
    'End Structure

    '<StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    'Public Structure WipeDecoySystemRequest
    '    Dim WipeAlgorithm As WipeAlgorithmId

    '    <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)>
    '    Dim WipeKey() As Byte
    'End Structure

    '<StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    'Public Structure DecoySystemWipeStatus
    '    Dim WipeInProgress As Boolean
    '    Dim WipeAlgorithm As WipeAlgorithmId
    '    Dim WipeAreaEnd As Long
    'End Structure

    '<StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    'Public Structure GetSystemDriveDumpConfigRequest
    '    Dim BootDriveFilterExtension As _DriveFilterExtension
    '    Dim HwEncryptionEnabled As Boolean
    'End Structure

#End Region

End Module


