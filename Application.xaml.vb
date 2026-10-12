Class Application

#If RELEASE Then
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        ' 檢查並要求系統管理員權限

        If Not AdminRelauncher.EnsureAdmin(showPrompt:=False) Then
            ' 程式將重新啟動,這裡會自動結束
            Return
        End If

        ' 繼續正常啟動流程
    End Sub
#End If

End Class