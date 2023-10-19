Imports System.IO
Imports System.Net
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class UpdataMsgBox
    Public Sub New(downloadUrl As String, releaseInfo As String)
        InitializeComponent()
        TextBox1.Text = releaseInfo
        ' 初始化 WebClient 並設置事件處理程序
        Dim client As New WebClient()
        AddHandler client.DownloadProgressChanged, AddressOf client_DownloadProgressChanged
        AddHandler client.DownloadFileCompleted, AddressOf client_DownloadFileCompleted

        ' 開始下載
        Dim currentPath As String = Application.ExecutablePath
        Dim newPath As String = Path.Combine(Path.GetDirectoryName(currentPath), $"{Path.GetFileName(currentPath)}.new")
        If File.Exists(newPath) Then File.Delete(newPath)
        client.DownloadFileAsync(New Uri(downloadUrl), newPath)
    End Sub

    Private Sub client_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
        ' 更新 ProgressBar 的值
        ProgressBar1.Value = e.ProgressPercentage
    End Sub
    Private Sub client_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs)
        If e.Error Is Nothing Then

            Dim currentPath As String = Application.ExecutablePath
            Dim oldPath As String = Path.Combine(Path.GetDirectoryName(currentPath), $"{Path.GetFileName(currentPath)}.old")
            If File.Exists(oldPath) Then File.Delete(oldPath)

            File.Move(currentPath, oldPath)

            Dim newPath As String = Path.Combine(Path.GetDirectoryName(currentPath), $"{Path.GetFileName(currentPath)}.new")

            File.Move(newPath, currentPath)

            MessageBox.Show("更新完成!")

            Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
            startInfo.FileName = Application.ExecutablePath
            startInfo.UseShellExecute = True
            startInfo.Verb = "runas" ' This indicates running as administrator
            Try
                Dim process As Process = New Process()
                process.StartInfo = startInfo
                process.Start()
                'Me.Dispose()
                End
                'Application.Exit()
            Catch ex As Exception
                ' Handle the exception if elevation fails
            End Try
            End
        Else
            MessageBox.Show($"下載出錯: {e.Error.Message}")
        End If
        Me.Close()
    End Sub

    Private Sub UpdataMsgBox_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class