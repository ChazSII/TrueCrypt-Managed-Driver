Imports CS2_Software.TrueCryptManagedDriver.Driver
Imports CS2_Software.TrueCryptManagedDriver.Security

<Obsolete("Please use TrueCryptManaged. This class is depricated and may be removed in furture versions.")>
Public Class TC_Driver
    Inherits TrueCryptManaged

    Public Sub New(driverLocation64bit As String)
        MyBase.New(driverLocation64bit)
    End Sub

    ''' <summary>
    ''' This method is obsolete and does not support keyfiles. Included for legacy only.
    ''' </summary>
    <Obsolete()>
    Public Overloads Function MountContainer(fileName As String, driveLetter As Char, password As Password, options As MOUNT_OPTIONS) As ErrorCodes
        Return MyBase.MountContainer(
            fileName,
            password.Text,
            driveLetter,
            options.ReadOnly,
            options.Removable,
            options.PreserveTimestamp,
            options.UseBackupHeader,
            options.RecoveryMode)
    End Function
End Class
