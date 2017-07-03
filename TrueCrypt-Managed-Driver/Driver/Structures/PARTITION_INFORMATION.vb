Imports System.Runtime.InteropServices

Namespace Common
    <StructLayout(LayoutKind.Sequential)>
    Public Structure PARTITION_INFORMATION
        Public StartingOffset As Long
        Public PartitionLength As Long
        Public HiddenSectors As Integer
        Public PartitionNumber As Integer
        Public PartitionType As Byte

        <MarshalAs(UnmanagedType.I1)>
        Public BootIndicator As Boolean

        <MarshalAs(UnmanagedType.I1)>
        Public RecognizedPartition As Boolean

        <MarshalAs(UnmanagedType.I1)>
        Public RewritePartition As Boolean
    End Structure
End Namespace