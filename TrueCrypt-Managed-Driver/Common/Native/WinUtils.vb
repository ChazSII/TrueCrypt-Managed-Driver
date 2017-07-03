Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Principal
Imports System.Globalization

Namespace Common.Native
    Public Class WinUtils

#Region "Windows"
        Public Shared Function IsAdmin() As Boolean
            Using ID As WindowsIdentity = WindowsIdentity.GetCurrent()
                Dim Principal As WindowsPrincipal = New WindowsPrincipal(ID)

                Return Principal.IsInRole(WindowsBuiltInRole.Administrator)
            End Using
        End Function

        Public Shared Function Is64Bit() As Boolean
            Return Environment.Is64BitOperatingSystem
        End Function

        Public Shared Function Language() As CultureInfo
            Return CultureInfo.CurrentUICulture
        End Function

        Public Shared Function EditionID() As String
            Return My.Computer.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID", "")
        End Function

        Public Shared ReadOnly Property WinDir() As String
            Get
                Return Environment.GetFolderPath(Environment.SpecialFolder.Windows)
            End Get
        End Property

        Public Shared ReadOnly Property SystemDir() As String
            Get
                Return Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)
            End Get
        End Property

        Public Shared ReadOnly Property SysWOWDir() As String
            Get
                Return Environment.GetFolderPath(Environment.SpecialFolder.System)
            End Get
        End Property

        Public Shared ReadOnly Property TempDir() As String
            Get
                Return Environment.GetEnvironmentVariable("temp")
            End Get
        End Property

        Public Shared ReadOnly Property UserProfile() As String
            Get
                Return Environment.GetEnvironmentVariable("userprofile")
            End Get
        End Property
#End Region

#Region "Utilities"
        Public Shared Sub TaskKill(processName As String)
            StartProcess("taskkill", WinDir, "/f /im " & processName)
        End Sub

        Public Shared Sub TakeOwn(fileName As String)
            StartProcess("takeown", WinDir, "/F " & Path4Dos(fileName))
        End Sub

        Public Shared Sub TakeOwnDir(directoryName As String)
            StartProcess("takeown", WinDir, "/F " & Path4Dos(directoryName) & " /R")
        End Sub

        Public Shared Sub Icacls(fileName As String)
            StartProcess("icacls", WinDir, Path4Dos(fileName) & " /grant everyone:f")
        End Sub

        Public Shared Sub IcaclsDir(directoryName As String)
            StartProcess("icacls", WinDir, Path4Dos(directoryName) & " /t /grant everyone:f")
        End Sub

        Public Shared Sub Regedt32(fileName As String)
            StartProcess("regedt32", WinDir, "/s " & Path4Dos(fileName))
        End Sub

        Public Shared Sub Schtasks(file As String)
            StartProcess("schtasks", WinDir, "/delete /tn " & file & " /f")
        End Sub

        Public Shared Sub Cscript(ByVal Params As String)
            StartProcess("cscript", WinDir, Params)
        End Sub

        Public Shared Sub Shutdown(ByVal Params As String)
            StartProcess("shutdown", WinDir, Params)
        End Sub

        Public Shared Sub InstallCertificate(ByVal PFXFile As String, ByVal Password As String)
            Dim Store As New X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine)
            Dim Certificate As New X509Certificate2(PFXFile, Password)

            Store.Open(OpenFlags.ReadWrite)
            Store.Add(Certificate)
            Store.Close()
        End Sub

        Public Shared Function Path4Dos(path As String) As String
            If path.Contains(" ") Then
                Return Chr(34) & path & Chr(34)
            Else
                Return path
            End If
        End Function

        Public Shared Sub StartProcess(fileName As String, workDir As String, arguments As String)
            Dim Start As New ProcessStartInfo With {
                .Arguments = arguments,
                .FileName = fileName,
                .UseShellExecute = True,
                .WorkingDirectory = workDir,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            }

            Try
                Process.Start(Start).WaitForExit()
            Catch ex As Exception
                'At UAC the user chose No
            End Try
        End Sub

        Public Shared Function StartProcess(fileName As String, workDir As String, arguments As String, windowsStyle As ProcessWindowStyle, createWindow As Boolean) As Process
            Dim Start As New ProcessStartInfo With {
                .Arguments = arguments,
                .FileName = fileName,
                .UseShellExecute = False,
                .WorkingDirectory = workDir,
                .CreateNoWindow = createWindow,
                .WindowStyle = windowsStyle,
                .RedirectStandardOutput = True,
                .RedirectStandardInput = True
            }

            Dim Proc As New Process With {
                .StartInfo = Start
            }

            Try
                Proc.Start()

                Return Proc
            Catch ex As Exception
                'At UAC the user chose No
            End Try

            Return Nothing
        End Function
#End Region

        Public Shared Sub Sleep(ByVal Milliseconds As Integer)
            Threading.Thread.Sleep(Milliseconds)
        End Sub

    End Class
End Namespace