Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Common.Enums

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
        Public Structure UNMOUNT_STRUCT
            Dim nDosDriveNo As Integer 'Drive letter to unmount
            Dim ignoreOpenFiles As Boolean
            Dim HiddenVolumeProtectionTriggered As Boolean
            Dim nReturnCode As TC_ERROR 'Return code back from driver
        End Structure
    End Namespace
End Namespace