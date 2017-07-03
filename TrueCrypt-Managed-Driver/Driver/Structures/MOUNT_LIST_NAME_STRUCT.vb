Imports System.Runtime.InteropServices

Namespace Driver
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure MOUNT_LIST_NAME_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim wszVolume() As Char
    End Structure
End Namespace