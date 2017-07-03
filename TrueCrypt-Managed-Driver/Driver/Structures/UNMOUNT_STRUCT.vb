Imports System.Runtime.InteropServices

Namespace Driver
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure UNMOUNT_STRUCT
        Dim nDosDriveNo As Integer 'Drive letter to unmount
        Dim ignoreOpenFiles As Boolean
        Dim HiddenVolumeProtectionTriggered As Boolean
        Dim nReturnCode As ErrorCodes 'Return code back from driver
    End Structure
End Namespace