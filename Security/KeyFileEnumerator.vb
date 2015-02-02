Imports System.IO
Imports System.Text

Namespace Security
    Public Class KeyFileEnumerator
        Implements IEnumerator(Of KeyFile)

        Dim KeyFiles As List(Of KeyFile)
        Dim position As Integer = -1

        Public Sub New(ByVal KeyFiles As List(Of KeyFile))
            Me.KeyFiles = KeyFiles
        End Sub

        Public ReadOnly Property Current As KeyFile Implements IEnumerator(Of KeyFile).Current
            Get
                Try
                    Return KeyFiles(position)
                Catch ex As IndexOutOfRangeException
                    Throw New InvalidOperationException()
                End Try
            End Get
        End Property

        Public ReadOnly Property Current1 As Object Implements IEnumerator.Current
            Get
                Try
                    Return KeyFiles(position)
                Catch ex As IndexOutOfRangeException
                    Throw New InvalidOperationException()
                End Try
            End Get
        End Property

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            position += 1
            Return position < KeyFiles.Count
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            position = -1
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' Per rilevare chiamate ridondanti

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: eliminare stato gestito (oggetti gestiti).
                End If

                ' TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del seguente Finalize().
                ' TODO: impostare campi di grandi dimensioni su null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: eseguire l'override di Finalize() solo se Dispose(ByVal disposing As Boolean) dispone del codice per liberare risorse non gestite.
        'Protected Overrides Sub Finalize()
        '    ' Non modificare questo codice. Inserire il codice di pulizia in Dispose(ByVal disposing As Boolean).
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Questo codice è aggiunto da Visual Basic per implementare in modo corretto il modello Disposable.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Non modificare questo codice. Inserire il codice di pulizia in Dispose(disposing As Boolean).
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
