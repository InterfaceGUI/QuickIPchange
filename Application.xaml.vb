Class Application

    ' 主視窗建立前先依設定套用淺色/深色主題
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        ThemeManager.Initialize()
    End Sub

    ' 未處理的例外改以對話框顯示，避免程式無聲崩潰
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        MessageBox.Show($"發生未預期的錯誤：{vbCrLf}{e.Exception.Message}",
                        "QuickIPChange", MessageBoxButton.OK, MessageBoxImage.Error)
        e.Handled = True
    End Sub

End Class
