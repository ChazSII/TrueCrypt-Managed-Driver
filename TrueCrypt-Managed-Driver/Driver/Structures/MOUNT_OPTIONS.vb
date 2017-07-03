Imports CS2_Software.TrueCryptManagedDriver.Security

Namespace Driver
    Public Structure MOUNT_OPTIONS
        Dim [ReadOnly] As Boolean
        Dim Removable As Boolean
        Dim ProtectHiddenVolume As Boolean
        Dim PreserveTimestamp As Boolean
        Dim PartitionInInactiveSysEncScope As Boolean
        Dim ProtectedHidVolPassword As Password
        Dim UseBackupHeader As Boolean
        Dim RecoveryMode As Boolean
    End Structure
End Namespace