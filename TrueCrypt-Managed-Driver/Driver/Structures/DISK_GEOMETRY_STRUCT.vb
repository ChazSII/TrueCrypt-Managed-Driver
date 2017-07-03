Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Common.Structures

Namespace Driver
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure DISK_GEOMETRY_STRUCT
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
        Dim deviceName() As Char
        Dim diskGeometry As DISK_GEOMETRY
    End Structure
End Namespace