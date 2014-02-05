Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim objIniFile As New IniFile(My.Computer.FileSystem.CurrentDirectory & "/settings.ini")
        Dim ircserver As String = objIniFile.GetString("Moebius", "server", "irc.wyldryde.org")
        Dim nick As String = objIniFile.GetString("Moebius", "nick", "Moebius")
        Me.Text = nick & " on " & ircserver
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Module1.Main()
    End Sub
End Class
