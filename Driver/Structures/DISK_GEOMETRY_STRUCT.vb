Imports System.Runtime.InteropServices
Imports CS2Soft.TrueCryptManagedDriver.Common.Structures

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure DISK_GEOMETRY_STRUCT
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim deviceName() As Char
            Dim diskGeometry As DISK_GEOMETRY
        End Structure
    End Namespace
End Namespace