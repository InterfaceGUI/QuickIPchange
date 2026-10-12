Imports System.Security.Principal
Imports System.Diagnostics

Public Class AdminRelauncher
    ''' <summary>
    ''' 檢查當前程式是否以系統管理員權限執行
    ''' </summary>
    Public Shared Function IsRunAsAdmin() As Boolean
        Try
            Dim identity = WindowsIdentity.GetCurrent()
            Dim principal = New WindowsPrincipal(identity)
            Return principal.IsInRole(WindowsBuiltInRole.Administrator)
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 以系統管理員權限重新啟動程式
    ''' </summary>
    Public Shared Sub RestartAsAdmin()
        Try
            Dim startInfo As New ProcessStartInfo() With {
                .UseShellExecute = True,
                .WorkingDirectory = Environment.CurrentDirectory,
                .FileName = Process.GetCurrentProcess().MainModule.FileName,
                .Verb = "runas"
            }

            ' 傳遞原本的命令列參數
            Dim args = Environment.GetCommandLineArgs()
            If args.Length > 1 Then
                startInfo.Arguments = String.Join(" ", args.Skip(1))
            End If

            Process.Start(startInfo)
            Application.Current.Shutdown()
        Catch ex As Exception
            ' 使用者取消 UAC 提示或發生錯誤
            MessageBox.Show($"無法以系統管理員權限啟動程式：{ex.Message}",
                          "錯誤", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    ''' <summary>
    ''' 檢查並在需要時要求系統管理員權限
    ''' </summary>
    ''' <param name="showPrompt">是否顯示提示訊息</param>
    ''' <returns>True 表示已有管理員權限，False 表示需要重新啟動</returns>
    Public Shared Function EnsureAdmin(Optional showPrompt As Boolean = True) As Boolean
        If IsRunAsAdmin() Then
            Return True
        End If

        If showPrompt Then
            Dim result = MessageBox.Show(
                "此程式需要系統管理員權限才能執行。" & vbCrLf &
                "是否要以系統管理員權限重新啟動？",
                "需要系統管理員權限",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                RestartAsAdmin()
            Else
                Application.Current.Shutdown()
            End If
        Else
            RestartAsAdmin()
        End If

        Return False
    End Function
End Class