Imports System.IO
Imports System.Text

Namespace Security
    Public Class KeyFile
        Dim pFileName As String
        Dim pIsDirectory As Boolean

        Public Sub New(ByVal FileName As String)
            pFileName = FileName
            pIsDirectory = Directory.Exists(FileName)
        End Sub

        Public ReadOnly Property FileName As String
            Get
                Return pFileName
            End Get
        End Property

        Public ReadOnly Property IsDirectory As Boolean
            Get
                Return pIsDirectory
            End Get
        End Property
    End Class
End Namespace