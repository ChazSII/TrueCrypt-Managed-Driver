Imports System.Runtime.InteropServices

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure PASSWORD_STUCT
            Dim Length As UInteger '4 byte

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=65)>
            Dim Text() As Char 'MAX_PASSWORD 65 byte

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3)>
            Dim Pad() As Char '3 byte
        End Structure
    End Namespace
End Namespace