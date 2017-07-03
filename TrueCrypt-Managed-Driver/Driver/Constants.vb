Namespace Driver
    Friend Module Constants
        'Path
        Public Const MAX_PASSWORD As Integer = 64
        Public Const TC_MAX_PATH As Integer = 260

        Public Const KEYFILE_POOL_SIZE As Integer = 64
        Public Const KEYFILE_MAX_READ_LEN As Integer = 1024 ^ 2

        Public Const TC_DRIVER_CONFIG_REG_VALUE_NAME As String = "TrueCryptConfig"
        Public Const TC_ENCRYPTION_FREE_CPU_COUNT_REG_VALUE_NAME As String = "TrueCryptEncryptionFreeCpuCount"
        Public Const TC_SERVICE_REG_KEY As String = "SYSTEM\CurrentControlSet\Services\TrueCrypt"

        'WARNING: Modifying the following values can introduce incompatibility with previous versions.
        Public Const TC_DRIVER_CONFIG_CACHE_BOOT_PASSWORD As UInteger = &H1
        Public Const TC_DRIVER_CONFIG_CACHE_BOOT_PASSWORD_FOR_SYS_FAVORITES As UInteger = &H2
        Public Const TC_DRIVER_CONFIG_DISABLE_NONADMIN_SYS_FAVORITES_ACCESS As UInteger = &H4
        Public Const TC_DRIVER_CONFIG_DISABLE_HARDWARE_ENCRYPTION As UInteger = &H8

        'CreateFile API
        Public Const OPEN_EXISTING As Integer = 3

        'Handle
        Public Const FILE_READ_ONLY_VOLUME As Integer = &H80000

        'Device Broadcast
        Public Const WM_DEVICECHANGE As Integer = &H219
        Public Const DBT_DEVTYP_VOLUME As Integer = &H2

        'File System Notification flags
        Public Const SHCNE_DRIVERREMOVED As Integer = &H80
        Public Const SHCNE_DRIVERADD As Integer = &H100

        'Flags
        Public Const SHCNF_PATH As Integer = &H1
        Public Const SMTO_ABORTIFHUNG As Integer = &H2

        'Destination
        Public Const HWND_BROADCAST As Integer = 65535


        Public Const INVALID_HANDLE_VALUE As Integer = -1
    End Module
End Namespace