Imports System.Runtime.InteropServices

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure OPEN_TEST_STRUCT
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim wszFileName() As Char
            Dim bDetectTCBootLoader As Boolean
            Dim TCBootLoaderDetected As Boolean
            Dim DetectFilesystem As Boolean
            Dim FilesystemDetected As Boolean
        End Structure
    End Namespace
End Namespace