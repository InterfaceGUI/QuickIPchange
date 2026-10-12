Imports System.Threading.Tasks
Imports System.Windows.Input

''' <summary>
''' 主視窗內的配置編輯面板（覆蓋在內容上方，取代獨立對話框）。
''' 以 ShowAsync 開啟並等待結果；取消時回傳 Nothing。
''' </summary>
Public Class ConfigEditor

    Private _pending As TaskCompletionSource(Of IPConfiguration)

    Public Sub New()
        InitializeComponent()
        AddHandler PreviewKeyDown, AddressOf Editor_PreviewKeyDown
    End Sub

    ''' <summary>開啟面板。existing 為 Nothing 代表新增。</summary>
    Public Function ShowAsync(existing As IPConfiguration) As Task(Of IPConfiguration)
        ' 若前一次尚未關閉，先以取消收尾
        _pending?.TrySetResult(Nothing)
        _pending = New TaskCompletionSource(Of IPConfiguration)

        Dim isEdit = existing IsNot Nothing
        TitleText.Text = If(isEdit, "編輯配置", "新增配置")
        ConfigNameBox.Text = If(isEdit, existing.ConfigName, "")
        IPBox.IPAddress = If(isEdit, existing.IPAddress, "")
        MaskBox.IPAddress = If(isEdit, existing.SubnetMask, "255.255.255.0")
        GatewayBox.IPAddress = If(isEdit, existing.Gateway, "")
        Dns1Box.IPAddress = If(isEdit, existing.DNS1, "")
        Dns2Box.IPAddress = If(isEdit, existing.DNS2, "")
        HideError()

        Visibility = Visibility.Visible
        Dispatcher.BeginInvoke(Sub()
                                  ConfigNameBox.Focus()
                                  ConfigNameBox.SelectAll()
                              End Sub)

        Return _pending.Task
    End Function

    Private Sub Save_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(ConfigNameBox.Text) Then
            ShowError("請輸入配置名稱")
            ConfigNameBox.Focus()
            Return
        End If

        Dim candidate As New IPConfiguration With {
            .ConfigName = ConfigNameBox.Text,
            .IPAddress = IPBox.IPAddress,
            .SubnetMask = MaskBox.IPAddress,
            .Gateway = GatewayBox.IPAddress,
            .DNS1 = Dns1Box.IPAddress,
            .DNS2 = Dns2Box.IPAddress
        }
        candidate.Normalize()

        Dim validationError = IPValidator.Validate(candidate)
        If validationError IsNot Nothing Then
            ShowError(validationError)
            Return
        End If

        Complete(candidate)
    End Sub

    Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
        Complete(Nothing)
    End Sub

    Private Sub Overlay_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        ' 點到遮罩（非卡片）視為取消
        Complete(Nothing)
    End Sub

    Private Sub Card_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        ' 阻止事件冒泡到遮罩
        e.Handled = True
    End Sub

    Private Sub Editor_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Escape Then
            Complete(Nothing)
            e.Handled = True
        ElseIf e.Key = Key.Enter AndAlso Not TypeOf e.OriginalSource Is Button Then
            Save_Click(sender, e)
            e.Handled = True
        End If
    End Sub

    Private Sub Complete(result As IPConfiguration)
        Visibility = Visibility.Collapsed
        Dim pending = _pending
        _pending = Nothing
        pending?.TrySetResult(result)
    End Sub

    Private Sub ShowError(message As String)
        ErrorText.Text = message
        ErrorText.Visibility = Visibility.Visible
    End Sub

    Private Sub HideError()
        ErrorText.Visibility = Visibility.Collapsed
    End Sub

End Class
