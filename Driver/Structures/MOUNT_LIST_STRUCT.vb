Imports System.Runtime.InteropServices

Namespace Driver
    Namespace Structures
        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
        Public Structure MOUNT_LIST_STRUCT
            Dim ulMountedDrives As UInteger 'Bitfield of all mounted drive letters

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
            Dim wszVolume() As MOUNT_LIST_NAME_STRUCT 'Volume names of mounted volumes

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
            Dim diskLength() As ULong

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
            Dim ea() As Integer

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=26)>
            Dim volumeType() As Integer 'Volume type (e.g. PROP_VOL_TYPE_OUTER, PROP_VOL_TYPE_OUTER_VOL_WRITE_PREVENTED, etc.)
        End Structure
    End Namespace
End Namespace