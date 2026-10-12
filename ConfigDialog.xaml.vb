Imports ToastNotifications
Imports ToastNotifications.Lifetime
Imports ToastNotifications.Position
Imports ToastNotifications.Messages

Public Class ConfigDialog
    Private _notifier As Notifier

    ''' <summary>使用者按下儲存後的結果（已正規化並通過驗證）</summary>
    Public ReadOnly Property Configuration As New IPConfiguration

    ' 新增模式
    Public Sub New()
        InitializeComponent()
        InitializeNotifier()
    End Sub

    ' 編輯模式
    Public Sub New(existing As IPConfiguration)
        InitializeComponent()
        InitializeNotifier()

        TitleText.Text = "編輯配置"
        ConfigNameBox.Text = existing.ConfigName
        IPBox.IPAddress = existing.IPAddress
        MaskBox.IPAddress = existing.SubnetMask
        GatewayBox.IPAddress = existing.Gateway
        Dns1Box.IPAddress = existing.DNS1
        Dns2Box.IPAddress = existing.DNS2
    End Sub

    Private Sub InitializeNotifier()
        _notifier = New Notifier(Sub(cfg)
                                     cfg.PositionProvider = New WindowPositionProvider(
                                         parentWindow:=Me,
                                         corner:=Corner.BottomRight,
                                         offsetX:=10,
                                         offsetY:=10)
                                     cfg.LifetimeSupervisor = New TimeAndCountBasedLifetimeSupervisor(
                                         notificationLifetime:=TimeSpan.FromSeconds(3),
                                         maximumNotificationCount:=MaximumNotificationCount.FromCount(3))
                                     cfg.Dispatcher = Application.Current.Dispatcher
                                 End Sub)
    End Sub

    Private Sub ConfigDialog_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        ConfigNameBox.Focus()
        ConfigNameBox.SelectAll()
    End Sub

    Private Sub Save_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(ConfigNameBox.Text) Then
            _notifier?.ShowWarning("請輸入配置名稱")
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
            _notifier?.ShowWarning(validationError)
            Return
        End If

        Configuration.CopyFrom(candidate)
        DialogResult = True
    End Sub

    Private Sub ConfigDialog_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        _notifier?.Dispose()
        _notifier = Nothing
    End Sub
End Class
