Imports CS2_Software.TrueCryptManagedDriver.Driver

<TestClass()> Public Class DriverInstanceTests

    Const CURRENT_VER As Integer = &H71A

    <TestMethod()> Public Sub TestLoadDriver()
        Using tcDriver As New ManagedDriverInstance()
            Assert.IsTrue(tcDriver.IsLoaded)
        End Using
    End Sub

    <TestMethod()> Public Sub TestDriverVersion()
        Using tcDriver As New ManagedDriverInstance()
            Assert.AreEqual(tcDriver.DriverVersion, CURRENT_VER)
        End Using
    End Sub

    <TestMethod()> Public Sub TestLoadDriverWithPath()
        Using tcDriver As New ManagedDriverInstance("lib\truecrypt-x64.sys")
            Assert.IsTrue(tcDriver.IsLoaded)
        End Using
    End Sub

    <TestMethod()> Public Sub TestDriverVersionWithPath()
        Using tcDriver As New ManagedDriverInstance("lib\truecrypt-x64.sys")
            Assert.AreEqual(tcDriver.DriverVersion, CURRENT_VER)
        End Using
    End Sub

End Class