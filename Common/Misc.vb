Imports System.IO
Imports CS2_Software.TrueCryptManagedDriver.Driver.Constants

Namespace Common
    Friend Module Misc

        Public Function IsFileOnReadOnlyFilesystem(ByVal fileName As String) As Boolean
            Dim d As UInteger, flags As UInteger

            If GetVolumeInformation(Path.GetPathRoot(fileName), Nothing, 0, Nothing, d, flags, Nothing, 0) Then
                Return (flags And FILE_READ_ONLY_VOLUME)
            End If

            Return False
        End Function

        Public Function StringLen(ByVal sender As String) As Integer
            Dim count As Integer = 0

            For Each Car As Char In sender
                If Not Car = Chr(0) Then count += 1
            Next

            Return count
        End Function

    End Module
End Namespace