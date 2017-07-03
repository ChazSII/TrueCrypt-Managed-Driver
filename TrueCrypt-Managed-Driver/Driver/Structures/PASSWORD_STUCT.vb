Imports System.Runtime.InteropServices

Namespace Driver
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    Public Structure PASSWORD_STUCT
        '''unsigned int
        Dim Length As UInteger

        '''unsigned char[65]
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=65)>
        Dim Text As String

        '''char[3]
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=3)>
        Dim Pad As String
    End Structure
End Namespace