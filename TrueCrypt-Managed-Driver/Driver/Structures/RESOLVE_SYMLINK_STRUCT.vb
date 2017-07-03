Imports System.Runtime.InteropServices

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure RESOLVE_SYMLINK_STRUCT
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim symLinkName() As Char

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim targetName() As Char
        End Structure
    End Namespace
End Namespace