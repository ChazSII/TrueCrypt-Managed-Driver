Imports System.Runtime.InteropServices

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure VOLUME_PROPERTIES_STRUCT
            Dim driveNo As Integer
            Dim uniqueId As Integer

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=260)>
            Dim wszVolume() As Char
            Dim diskLenght As ULong
            Dim ea As Integer
            Dim mode As Integer
            Dim pkcs5 As Integer
            Dim pkcs5Iterations As Integer
            Dim hiddenVolume As Boolean
            Dim [readOnly] As Boolean
            Dim removable As Boolean
            Dim partitionInInactiveSysEncScope As Boolean
            Dim volumeHeaderFlags As UInteger
            Dim totalBytesRead As ULong
            Dim totalBytesWritten As ULong
            Dim hiddenVolProtection As Integer 'Hidden volume protection status (e.g. HIDVOL_PROT_STATUS_NONE, HIDVOL_PROT_STATUS_ACTIVE, etc.)
            Dim volFormatVersion As Integer
        End Structure
    End Namespace
End Namespace