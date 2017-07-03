Imports System.Runtime.InteropServices
Imports CS2_Software.TrueCryptManagedDriver.Driver

<TestClass()> Public Class PasswordTests

    <TestMethod()> Public Sub TestSingleKeyFile()
        Dim testPassword As String = "aslkdfngoine4508htns0q3j-2E%$^^%EE7ur1ekj2t-gg5qe4bv"

        Using vbPassword As New CS2_Software.TrueCryptManagedDriver.Security.Password(testPassword, "test.keyfile")

            Dim cPassword As New PASSWORD_STUCT With {
                .Text = testPassword,
                .Length = testPassword.Length
            }

            Dim cMixKeyFileSuccess As Boolean = MixKeyFile(cPassword, "test.keyfile")
            Dim vbApplyKeyFileSuccess As Boolean = vbPassword.TryApplyKeyFiles()

            Assert.IsTrue(cMixKeyFileSuccess)
            Assert.IsTrue(vbApplyKeyFileSuccess)

            Assert.AreEqual(cPassword.Text, vbPassword.Text)
        End Using
    End Sub

    '''Return Type: boolean
    '''password: Password*
    '''fileName: char*
    <DllImport("Lib\TrueCryptNativeTestLib.dll", EntryPoint:="MixKeyFile", CallingConvention:=CallingConvention.Cdecl)>
    Public Shared Function MixKeyFile(ByRef password As PASSWORD_STUCT,
                                      <[In](), MarshalAs(UnmanagedType.LPStr)> ByVal fileName As String) As <MarshalAs(UnmanagedType.I1)> Boolean
    End Function

End Class