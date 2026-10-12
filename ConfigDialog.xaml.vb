Imports ToastNotifications
Imports ToastNotifications.Lifetime
Imports ToastNotifications.Position
Imports ToastNotifications.Messages

Public Class ConfigDialog
    Private _configuration As IPConfiguration
    Private _isEdit As Boolean
    Private _notifier As Notifier

    Public Property Configuration As IPConfiguration
        Get
            Return _configuration
        End Get
        Set(value As IPConfiguration)
            _configuration = value
        End Set
    End Property

    ' 新增模式
    Public Sub New()
        InitializeComponent()

        _isEdit = False
        _configuration = New IPConfiguration()
        InitializeNotifier()
    End Sub

    ' 編輯模式
    Public Sub New(config As IPConfiguration)
        InitializeComponent()

        _isEdit = True
        _configuration = New IPConfiguration With {
            .ConfigName = config.ConfigName,
            .IPAddress = config.IPAddress,
            .SubnetMask = config.SubnetMask,
            .Gateway = config.Gateway
        }

        ' 載入現有值
        ConfigNameBox.Text = config.ConfigName
        IPBox.IPAddress = config.IPAddress
        MaskBox.IPAddress = config.SubnetMask
        GatewayBox.IPAddress = config.Gateway
        InitializeNotifier()
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

    Private Sub Save_Click(sender As Object, e As RoutedEventArgs)
        ' 驗證輸入
        If String.IsNullOrWhiteSpace(ConfigNameBox.Text) Then
            _notifier?.ShowWarning("請輸入配置名稱")
            ConfigNameBox.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(IPBox.IPAddress) Then
            _notifier?.ShowWarning("請輸入 IP 位址")
            IPBox.Focus()
            Return
        End If

        If String.IsNullOrWhiteSpace(MaskBox.IPAddress) Then
            _notifier?.ShowWarning("請輸入子網路遮罩")
            MaskBox.Focus()
            Return
        End If

        ' 處理閘道：如果為空或全為 0 則設為 0.0.0.0
        Dim gateway As String = GatewayBox.IPAddress.Trim()
        If String.IsNullOrWhiteSpace(gateway) OrElse gateway = "0.0.0.0" Then
            gateway = "0.0.0.0"
        End If

        ' 儲存值
        _configuration.ConfigName = ConfigNameBox.Text.Trim()
        _configuration.IPAddress = IPBox.IPAddress.Trim()
        _configuration.SubnetMask = MaskBox.IPAddress.Trim()
        _configuration.Gateway = gateway

        DialogResult = True
        Close()
    End Sub

    Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = False
        Close()
    End Sub

    Private Sub ConfigDialog_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        ' 清理 Toast 通知
        If _notifier IsNot Nothing Then
            _notifier.Dispose()
            _notifier = Nothing
        End If
    End Sub
End Class
