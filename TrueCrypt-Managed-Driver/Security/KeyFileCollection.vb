Imports System.IO
Imports System.Text

Namespace Security
    Public Class KeyFileCollection
        Implements IList(Of KeyFile) ', IEnumerable(Of KeyFile)

        Dim KeyFiles As New List(Of KeyFile)

        Public Sub Add(item As KeyFile) Implements ICollection(Of KeyFile).Add
            KeyFiles.Add(item)
        End Sub

        Public Sub Add(item As String)
            KeyFiles.Add(New KeyFile(item))
        End Sub

        Public Sub Clear() Implements ICollection(Of KeyFile).Clear
            KeyFiles.Clear()
        End Sub

        Public Function Contains(item As KeyFile) As Boolean Implements ICollection(Of KeyFile).Contains
            Return KeyFiles.Contains(item)
        End Function

        Public Sub CopyTo(array() As KeyFile, arrayIndex As Integer) Implements ICollection(Of KeyFile).CopyTo
            KeyFiles.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of KeyFile).Count
            Get
                Return KeyFiles.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyFile).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As KeyFile) As Boolean Implements ICollection(Of KeyFile).Remove
            Return KeyFiles.Remove(item)
        End Function

        Public Function GetEnumerator() As IEnumerator(Of KeyFile) Implements IEnumerable(Of KeyFile).GetEnumerator
            Return New KeyFileEnumerator(KeyFiles)
        End Function

        Public Function IndexOf(item As KeyFile) As Integer Implements IList(Of KeyFile).IndexOf
            Return KeyFiles.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As KeyFile) Implements IList(Of KeyFile).Insert
            KeyFiles.Insert(index, item)
        End Sub

        Default Public Property Item(index As Integer) As KeyFile Implements IList(Of KeyFile).Item
            Get
                Return KeyFiles(index)
            End Get
            Set(value As KeyFile)
                KeyFiles(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of KeyFile).RemoveAt
            KeyFiles.RemoveAt(index)
        End Sub

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New KeyFileEnumerator(KeyFiles)
        End Function
    End Class
End Namespace