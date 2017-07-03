Imports System.IO
Imports CS2_Software.TrueCryptManagedDriver.Driver.Constants
Imports CS2_Software.TrueCryptManagedDriver.Common.Native

Namespace Common
    Friend Module Misc

        Public Function IsFileOnReadOnlyFilesystem(fileName As String) As Boolean
            Dim flags As UInteger

            If GetVolumeInformation(Path.GetPathRoot(fileName), Nothing, 0, Nothing, 0, flags, Nothing, 0) Then
                Return (flags And FILE_READ_ONLY_VOLUME)
            End If

            Return False
        End Function

        'Public Function StringLen(sender As String) As Integer
        '    Dim count As Integer = 0

        '    For Each Car As Char In sender
        '        If Not Car = Chr(0) Then count += 1
        '    Next

        '    Return count
        'End Function

    End Module
End Namespace