Class Application

    ' 未處理的例外改以對話框顯示，避免程式無聲崩潰
    Private Sub Application_DispatcherUnhandledException(sender As Object, e As Threading.DispatcherUnhandledExceptionEventArgs) Handles Me.DispatcherUnhandledException
        MessageBox.Show($"發生未預期的錯誤：{vbCrLf}{e.Exception.Message}",
                        "QuickIPChange", MessageBoxButton.OK, MessageBoxImage.Error)
        e.Handled = True
    End Sub

End Class
