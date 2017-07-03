Namespace Driver
    Public Enum ErrorCodes As Integer
        'WARNING: ADD ANY NEW CODES AT THE END (DO NOT INSERT THEM BETWEEN EXISTING). DO *NOT* DELETE ANY 
        'EXISTING CODES! Changing these values or their meanings may cause incompatibility with other versions
        '(for example, if a new version of the TrueCrypt installer receives an error code from an installed 
        'driver whose version is lower, it will report and interpret the error incorrectly).

        SUCCESS = 0
        OS_ERROR = 1
        OUTOFMEMORY = 2
        PASSWORD_WRONG = 3
        VOL_FORMAT_BAD = 4
        DRIVE_NOT_FOUND = 5
        FILES_OPEN = 6
        VOL_SIZE_WRONG = 7
        COMPRESSION_NOT_SUPPORTED = 8
        PASSWORD_CHANGE_VOL_TYPE = 9
        PASSWORD_CHANGE_VOL_VERSION = 10
        VOL_SEEKING = 11
        VOL_WRITING = 12
        FILES_OPEN_LOCK = 13
        VOL_READING = 14
        DRIVER_VERSION = 15
        NEW_VERSION_REQUIRED = 16
        CIPHER_INIT_FAILURE = 17
        CIPHER_INIT_WEAK_KEY = 18
        SELF_TESTS_FAILED = 19
        SECTOR_SIZE_INCOMPATIBLE = 20
        VOL_ALREADY_MOUNTED = 21
        NO_FREE_DRIVES = 22
        FILE_OPEN_FAILED = 23
        VOL_MOUNT_FAILED = 24
        DEPRECATED_ERR_INVALID_DEVICE = 25
        ACCESS_DENIED = 26
        MODE_INIT_FAILED = 27
        DONT_REPORT = 28
        ENCRYPTION_NOT_COMPLETED = 29
        PARAMETER_INCORRECT = 30
        SYS_HIDVOL_HEAD_REENC_MODE_WRONG = 31
        NONSYS_INPLACE_ENC_INCOMPLETE = 32
        USER_ABORT = 33

        GENERIC = 255
    End Enum
End Namespace