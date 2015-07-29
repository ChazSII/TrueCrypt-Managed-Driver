Imports System.IO
Imports System.Text
Imports CS2_Software.TrueCryptManagedDriver.Common
Imports CS2_Software.TrueCryptManagedDriver.Driver.Constants

Namespace Security
    Public Class Password
        Dim pPassword As String
        Dim pKeyFile As New KeyFileCollection

        Public Property Password As String
            Set(value As String)
                pPassword = value
            End Set
            Get
                Return pPassword
            End Get
        End Property

        Public Property KeyFile As KeyFileCollection
            Set(value As KeyFileCollection)
                pKeyFile = value
            End Set
            Get
                Return pKeyFile
            End Get
        End Property

        Friend Function ApplyKeyFile(ByRef Password As String) As Boolean
            Dim keyPool(KEYFILE_POOL_SIZE - 1) As Byte, Ret(MAX_PASSWORD) As Byte
            Dim fInfo As FileInfo

            'For i As Integer = 0 To KEYFILE_POOL_SIZE - 1
            '    keyPool(i) = 204
            'Next

            For Each KeyFile As KeyFile In pKeyFile
                'If it's a token
                'TODO

                If KeyFile.IsDirectory Then 'If it's a directory
                    For Each sFile As String In Directory.EnumerateFiles(KeyFile.FileName, "*.*", SearchOption.TopDirectoryOnly)
                        fInfo = New FileInfo(sFile)

                        If Not fInfo.Attributes And FileAttributes.Hidden Then
                            If Not ProcessKeyFile(keyPool, KeyFile.FileName) Then Return False
                        End If
                    Next
                Else 'If it's a file
                    If Not ProcessKeyFile(keyPool, KeyFile.FileName) Then Return False
                End If
            Next

            'Mix the keyfile pool contents into the password
            For i As Integer = 0 To keyPool.Length - 1
                If i < pPassword.Length Then
                    Ret(i) = (Asc(pPassword(i)) + keyPool(i)) Mod 256
                Else
                    Ret(i) = keyPool(i)
                End If
            Next

            Password = Encoding.Default.GetString(Ret)

            keyPool = Nothing
            Ret = Nothing

            Return True
        End Function

        Private Function ProcessKeyFile(ByRef keyPool() As Byte, ByVal FileName As String) As Boolean
            Dim keyFileInfo As New IO.FileInfo(FileName)
            Dim totalRead As Integer = 0
            Dim crc As UInteger = 4294967295 'Decimal for 0xFFFFFFFF

            Using keyFileStream As New FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
                Using binKeyFileStream As New BinaryReader(keyFileStream)
                    Dim buffer(64 * 1024) As Byte

                    Dim writePos As Integer = 0
                    Dim bytesRead As Integer = binKeyFileStream.Read(buffer, 0, buffer.Length)

                    While bytesRead > 0
                        For i As Integer = 0 To bytesRead - 1
                            crc = UPDC32(buffer(i), crc)

                            For writePosCount As Integer = 0 To 3
                                Dim bitShiftAmount As Integer = 24 - (8 * writePosCount)

                                keyPool(writePos) = ProccessShiftBits(keyPool(writePos), crc, bitShiftAmount)

                                writePos += 1
                            Next

                            If writePos >= KEYFILE_POOL_SIZE Then writePos = 0

                            If totalRead >= KEYFILE_MAX_READ_LEN Then GoTo close

                            totalRead += 1
                        Next

                        bytesRead = binKeyFileStream.Read(buffer, 0, buffer.Length)
                    End While
                End Using
            End Using

close:
            IO.File.SetCreationTime(FileName, keyFileInfo.CreationTime)
            IO.File.SetLastAccessTime(FileName, keyFileInfo.LastAccessTime)
            IO.File.SetLastWriteTime(FileName, keyFileInfo.LastWriteTime)

            If totalRead = 0 Then Return False

            Return True
        End Function

        Private Function ProccessShiftBits(original As Byte, crc As UInteger, ammount As Integer) As Byte
            Dim shiftResult As UInteger = crc >> ammount
            Dim shiftByte As Byte = CByte(shiftResult Mod 256)

            Dim sumValue As UInteger = original + shiftResult
            Dim sumByte As Byte = CByte(sumValue Mod 256)

            Return sumByte
        End Function

    End Class
End Namespace