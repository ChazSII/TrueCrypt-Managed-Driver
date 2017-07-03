Imports System.Text
Imports CS2_Software.TrueCryptManagedDriver
Imports CS2_Software.TrueCryptManagedDriver.Driver
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class MountTests
    Private testPassword As String = "aslkdfngoine4508htns0q3j-2E%$^^%EE7ur1ekj2t-gg5qe4bv"

    <TestMethod()> Public Sub TestMountWithKeyFile()
        Using tcDrv As New TrueCryptManaged("Lib\truecrypt-x64.sys") ', "Lib\truecrypt.sys")
            Try
                Dim returnStatus As ErrorCodes =
                    tcDrv.MountContainer(
                                     IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "testWithKeyFile.container"),
                                     testPassword,
                                     "Y",
                                     readOnly:=True,
                                     removable:=True,
                                     keyFileNames:={"test.keyfile"})

                Assert.IsTrue(returnStatus = ErrorCodes.SUCCESS, "Status code: " & [Enum].GetName(GetType(ErrorCodes), returnStatus))

                Assert.IsTrue(IO.Directory.Exists("Y:\"), "Mounted file does not exist.")
            Catch ex As Exception
                Throw ex
            Finally
                tcDrv.Dismount("Y", True)
            End Try
        End Using
    End Sub

    <TestMethod()> Public Sub TestMountWithOutKeyFile()
        Using tcDrv As New TrueCryptManaged("Lib\truecrypt-x64.sys") ', "Lib\truecrypt.sys")
            Try
                Dim returnStatus As ErrorCodes =
                    tcDrv.MountContainer(
                                     IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "testWithOutKeyFile.container"),
                                     testPassword,
                                     "Y",
                                     readOnly:=True,
                                     removable:=True)

                Assert.IsTrue(returnStatus = ErrorCodes.SUCCESS, "Status code: " & [Enum].GetName(GetType(ErrorCodes), returnStatus))

                Assert.IsTrue(IO.Directory.Exists("Y:\"), "Mounted file does not exist.")
            Catch ex As Exception
                Throw ex
            Finally
                tcDrv.Dismount("Y", True)
            End Try
        End Using
    End Sub

    <TestMethod()> Public Sub TestLegacyMount()
        Using tcDrv As New TC_Driver("Lib\truecrypt-x64.sys")
            Try
                Dim mountPassword As New Security.Password(testPassword)

                Dim mountOptions As New MOUNT_OPTIONS With {
                    .ReadOnly = True,
                    .Removable = True
                }

                Dim returnStatus As ErrorCodes =
                    tcDrv.MountContainer(
                                      IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "testWithOutKeyFile.container"),
                                     "Y",
                                     mountPassword,
                                     mountOptions)

                Assert.IsTrue(returnStatus = ErrorCodes.SUCCESS, "Status code: " & [Enum].GetName(GetType(ErrorCodes), returnStatus))

                Assert.IsTrue(IO.Directory.Exists("Y:\"), "Mounted file does not exist.")
            Catch ex As Exception
                Throw ex
            Finally
                tcDrv.Dismount("Y", True)
            End Try
        End Using
    End Sub

End Class