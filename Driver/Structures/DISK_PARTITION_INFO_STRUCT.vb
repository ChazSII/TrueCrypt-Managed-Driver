Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Common.Structures

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure DISK_PARTITION_INFO_STRUCT
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim deviceName() As Char
            Dim partInfo() As PARTITION_INFORMATION
            Dim IsGPT As Boolean
            Dim IsDynamic As Boolean
        End Structure
    End Namespace
End Namespace