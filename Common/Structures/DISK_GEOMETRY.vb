Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Common.Enums

Namespace Common
    Namespace Structures
        <StructLayout(LayoutKind.Sequential)> _
        Public Structure DISK_GEOMETRY
            Public Cylinders As Long
            Public MediaType As MEDIA_TYPE
            Public TracksPerCylinder As Integer
            Public SectorsPerTrack As Integer
            Public BytesPerSector As Integer

            Public ReadOnly Property disksize() As Long
                Get
                    Return Cylinders * CLng(TracksPerCylinder) * CLng(SectorsPerTrack) * CLng(BytesPerSector)
                End Get
            End Property
        End Structure
    End Namespace
End Namespace
