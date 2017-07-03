Imports System.IO

Namespace Security
    Public Class KeyFile

        Public Sub New(ByVal FileName As String)
            _FileName = FileName
            _IsDirectory = Directory.Exists(FileName)
        End Sub

        Public ReadOnly Property FileName As String

        Public ReadOnly Property IsDirectory As Boolean

    End Class
End Namespace