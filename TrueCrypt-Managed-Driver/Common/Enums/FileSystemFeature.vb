Namespace Common
    <Flags>
    Public Enum FileSystemFeature As UInteger
        ''' <summary>
        ''' The file system supports case-sensitive file names.
        ''' </summary>
        CaseSensitiveSearch = 1
        ''' <summary>
        ''' The file system preserves the case of file names when it places a name on disk.
        ''' </summary>
        CasePreservedNames = 2
        ''' <summary>
        ''' The file system supports Unicode in file names as they appear on disk.
        ''' </summary>
        UnicodeOnDisk = 4
        ''' <summary>
        ''' The file system preserves and enforces access control lists (ACL).
        ''' </summary>
        PersistentACLS = 8
        ''' <summary>
        ''' The file system supports file-based compression.
        ''' </summary>
        FileCompression = &H10
        ''' <summary>
        ''' The file system supports disk quotas.
        ''' </summary>
        VolumeQuotas = &H20
        ''' <summary>
        ''' The file system supports sparse files.
        ''' </summary>
        SupportsSparseFiles = &H40
        ''' <summary>
        ''' The file system supports re-parse points.
        ''' </summary>
        SupportsReparsePoints = &H80
        ''' <summary>
        ''' The specified volume is a compressed volume, for example, a DoubleSpace volume.
        ''' </summary>
        VolumeIsCompressed = &H8000
        ''' <summary>
        ''' The file system supports object identifiers.
        ''' </summary>
        SupportsObjectIDs = &H10000
        ''' <summary>
        ''' The file system supports the Encrypted File System (EFS).
        ''' </summary>
        SupportsEncryption = &H20000
        ''' <summary>
        ''' The file system supports named streams.
        ''' </summary>
        NamedStreams = &H40000
        ''' <summary>
        ''' The specified volume is read-only.
        ''' </summary>
        ReadOnlyVolume = &H80000
        ''' <summary>
        ''' The volume supports a single sequential write.
        ''' </summary>
        SequentialWriteOnce = &H100000
        ''' <summary>
        ''' The volume supports transactions.
        ''' </summary>
        SupportsTransactions = &H200000
    End Enum
End Namespace