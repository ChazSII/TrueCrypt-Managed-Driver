Imports System.Runtime.InteropServices

Namespace Common
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Public Structure DEV_BROADCAST_VOLUME
        Dim dbcv_size As UInteger
        Dim dbcv_devicetype As UInteger
        Dim dbcv_reserved As UInteger
        Dim dbcv_unitmask As UInteger
        Dim dbcv_flags As UShort
    End Structure
End Namespace