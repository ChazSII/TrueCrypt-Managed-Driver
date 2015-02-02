Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Principal
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Common
    Public Class WinUtils

#Region "Windows"
        Public Shared Function IsAdmin() As Boolean
            Dim ID As WindowsIdentity = WindowsIdentity.GetCurrent
            Dim Principal As WindowsPrincipal = New WindowsPrincipal(ID)
            Return Principal.IsInRole(WindowsBuiltInRole.Administrator)
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
        Public Shared Sub TaskKill(ByVal NomeProcesso As String)
            AvviaProcesso("taskkill", WinDir, "/f /im " & NomeProcesso)
        End Sub

        Public Shared Sub TakeOwn(ByVal NomeFile As String)
            AvviaProcesso("takeown", WinDir, "/F " & Path4Dos(NomeFile))
        End Sub

        Public Shared Sub TakeOwnDir(ByVal NomeDir As String)
            AvviaProcesso("takeown", WinDir, "/F " & Path4Dos(NomeDir) & " /R")
        End Sub

        Public Shared Sub Icacls(ByVal NomeFile As String)
            AvviaProcesso("icacls", WinDir, Path4Dos(NomeFile) & " /grant everyone:f")
        End Sub

        Public Shared Sub IcaclsDir(ByVal NomeDir As String)
            AvviaProcesso("icacls", WinDir, Path4Dos(NomeDir) & " /t /grant everyone:f")
        End Sub

        Public Shared Sub Regedt32(ByVal NomeFile As String)
            AvviaProcesso("regedt32", WinDir, "/s " & Path4Dos(NomeFile))
        End Sub

        Public Shared Sub schtasks(ByVal File As String)
            AvviaProcesso("schtasks", WinDir, "/delete /tn " & File & " /f")
        End Sub

        Public Shared Sub cscript(ByVal Params As String)
            AvviaProcesso("cscript", WinDir, Params)
        End Sub

        Public Shared Sub shutdown(ByVal Params As String)
            AvviaProcesso("shutdown", WinDir, Params)
        End Sub

        Public Shared Sub InstallCertificate(ByVal PFXFile As String, ByVal Password As String)
            Dim Store As New X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine)
            Dim Certificate As New X509Certificate2(PFXFile, Password)

            Store.Open(OpenFlags.ReadWrite)
            Store.Add(Certificate)
            Store.Close()
        End Sub

        Public Shared Function Path4Dos(ByVal Percorso As String) As String
            If Percorso.Contains(" ") Then
                Return Chr(34) & Percorso & Chr(34)
            Else
                Return Percorso
            End If
        End Function

        Public Shared Sub AvviaProcesso(ByVal NomeProgramma As String, ByVal WorkDir As String, ByVal Parametri As String)
            Dim Start As New ProcessStartInfo

            Start.Arguments = Parametri
            Start.FileName = NomeProgramma
            Start.UseShellExecute = True
            Start.WorkingDirectory = WorkDir
            Start.Verb = "runas"
            Start.WindowStyle = ProcessWindowStyle.Hidden

            Try
                Process.Start(Start).WaitForExit()
            Catch ex As Exception
                'All'UAC l'utente ha scelto No
            End Try
        End Sub

        Public Shared Function AvviaProcessoIO(ByVal NomeProgramma As String, ByVal WorkDir As String, ByVal Argomenti As String, ByVal Modo As ProcessWindowStyle, ByVal CreaFinestra As Boolean) As Process
            Dim Start As New ProcessStartInfo, Proc As New Process

            Start.Arguments = Argomenti
            Start.FileName = NomeProgramma
            Start.UseShellExecute = False
            Start.WorkingDirectory = WorkDir

            Start.CreateNoWindow = CreaFinestra
            Start.WindowStyle = Modo

            Start.RedirectStandardOutput = True
            Start.RedirectStandardInput = True

            Proc.StartInfo = Start

            Try
                Proc.Start()

                Return Proc
            Catch ex As Exception
                'All'UAC l'utente ha scelto No
            End Try

            Return Nothing
        End Function
#End Region

        Public Shared Sub Sleep(ByVal Milliseconds As Integer)
            Threading.Thread.Sleep(Milliseconds)
        End Sub

    End Class
End Namespace