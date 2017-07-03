Imports System.IO
Imports System.Text
Imports CS2_Software.TrueCryptManagedDriver.Common
Imports CS2_Software.TrueCryptManagedDriver.Common.Native
Imports CS2_Software.TrueCryptManagedDriver.Driver

Namespace Security
    Public Class Password
        Implements IDisposable

        Public ReadOnly Property Text As String
        Public ReadOnly Property KeyFiles As New List(Of KeyFile)

        Public Sub New(password As String)
            _Text = password
        End Sub

        Public Sub New(password As String, ParamArray keyFiles As KeyFile())
            _Text = password

            If keyFiles IsNot Nothing Then
                keyFiles.ToList().ForEach(Sub(item) _KeyFiles.Add(item))
            End If
        End Sub

        Public Sub New(password As String, ParamArray keyFileNames As String())
            _Text = password

            If keyFileNames IsNot Nothing Then
                keyFileNames.ToList().ForEach(Sub(item) _KeyFiles.Add(New KeyFile(item)))
            End If
        End Sub


        Public Function TryApplyKeyFiles() As Boolean
            Dim keyPool(KEYFILE_POOL_SIZE - 1) As Byte
            Dim passwordArray(MAX_PASSWORD - 1) As Byte

            For Each keyFile As KeyFile In KeyFiles
                'If it's a token
                'TODO
                If keyFile.IsDirectory Then
                    For Each sFile As String In Directory.EnumerateFiles(keyFile.FileName, "*.*", SearchOption.TopDirectoryOnly)
                        Dim keyFileInfo As New FileInfo(sFile)

                        If Not keyFileInfo.Attributes And FileAttributes.Hidden Then
                            If Not ProcessKeyFile(keyPool, keyFile.FileName) Then Return False
                        End If
                    Next
                Else
                    'If it's a file
                    If Not ProcessKeyFile(keyPool, keyFile.FileName) Then Return False
                End If
            Next

            'Mix the keyfile pool contents into the password
            For i As Integer = 0 To keyPool.Length - 1
                If i < Text.Length Then
                    passwordArray(i) = (Asc(Text(i)) + keyPool(i)) Mod 256
                Else
                    passwordArray(i) = keyPool(i)
                End If
            Next

            _Text = Encoding.Default.GetString(passwordArray)

            keyPool = Nothing
            passwordArray = Nothing

            Return True
        End Function

        Private Function ProcessKeyFile(ByRef keyPool() As Byte, ByVal fileName As String) As Boolean
            Dim srcFilePointer As IntPtr = -1
            Dim keyFileCreationTime As Long,
                keyFileLastWriteTime As Long,
                keyFileLastAccessTime As Long,
                keyFileTimeStampValid As Boolean = False

            Dim totalRead As Integer

            Try

                srcFilePointer = CreateFile(fileName, FileAccess.ReadWrite, FileShare.ReadWrite, Nothing, FileMode.Open, 0, Nothing)

                If Not srcFilePointer = INVALID_HANDLE_VALUE Then
                    If GetFileTime(srcFilePointer, keyFileCreationTime, keyFileLastAccessTime, keyFileLastWriteTime) Then
                        keyFileTimeStampValid = True
                    End If
                End If

                Using keyFileStream As New FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using keyFileReader As New BinaryReader(keyFileStream)
                        Dim buffer(64 * 1024) As Byte

                        Dim crc As UInteger = 4294967295
                        Dim writePosistion As Integer = 0

                        Dim bytesRead As Integer = keyFileReader.Read(buffer, 0, buffer.Length)

                        While bytesRead > 0
                            For i As Integer = 0 To bytesRead - 1
                                crc = UPDC32(buffer(i), crc)

                                keyPool(writePosistion) = (keyPool(writePosistion) + ((crc >> 24) Mod 256)) Mod 256
                                keyPool(writePosistion + 1) = (keyPool(writePosistion + 1) + ((crc >> 16) Mod 256)) Mod 256
                                keyPool(writePosistion + 2) = (keyPool(writePosistion + 2) + ((crc >> 8) Mod 256)) Mod 256
                                keyPool(writePosistion + 3) = (keyPool(writePosistion + 3) + ((crc) Mod 256)) Mod 256
                                writePosistion += 4

                                If writePosistion >= KEYFILE_POOL_SIZE Then
                                    writePosistion = 0
                                End If

                                If totalRead >= KEYFILE_MAX_READ_LEN Then GoTo close
                                totalRead += 1
                            Next

                            bytesRead = keyFileReader.Read(buffer, 0, buffer.Length)
                        End While

close:
                        If keyFileTimeStampValid And Not IsFileOnReadOnlyFilesystem(fileName) Then
                            SetFileTime(srcFilePointer, keyFileCreationTime, keyFileLastAccessTime, keyFileLastWriteTime)
                        End If

                    End Using
                End Using
            Catch ex As Exception
                totalRead = 0
            Finally
                If srcFilePointer <> INVALID_HANDLE_VALUE Then
                    CloseHandle(srcFilePointer)
                End If
            End Try

            If totalRead = 0 Then Return False

            Return True
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
                _Text = Nothing
                _KeyFiles = Nothing
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace