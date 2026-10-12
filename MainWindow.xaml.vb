Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json
Imports ToastNotifications
Imports ToastNotifications.Core
Imports ToastNotifications.Lifetime
Imports ToastNotifications.Messages
Imports ToastNotifications.Position

Class MainWindow
    Private Shared ReadOnly RefreshInterval As TimeSpan = TimeSpan.FromSeconds(3)
    Private Shared ReadOnly SettleDelay As TimeSpan = TimeSpan.FromSeconds(1)

    Private ReadOnly _adapters As New ObservableCollection(Of NetworkAdapterInfo)
    Private _configStorage As New List(Of IPConfiguration)
    Private _currentAdapter As NetworkAdapterInfo
    Private _refreshTimer As Threading.DispatcherTimer
    Private _isRefreshing As Boolean
    Private _isBusy As Boolean
    Private _notifier As Notifier
    Private _notifierOptions As MessageOptions

#Region "生命週期"

    Private Async Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Title = $"QuickIPChange v{UpdateService.CurrentVersion.ToString(3)} - 快速 IP 切換工具"
        InitializeNotifier()

        AddHandler ThemeManager.ThemeChanged, AddressOf UpdateThemeIcon
        UpdateThemeIcon()

        AdapterListBox.ItemsSource = _adapters
        LoadConfigsFromFile()
        RenderQuickConfigs()

        Await RefreshAdaptersAsync()

        _refreshTimer = New Threading.DispatcherTimer With {.Interval = RefreshInterval}
        AddHandler _refreshTimer.Tick, AddressOf RefreshTimer_Tick
        _refreshTimer.Start()

        UpdateService.CleanupLeftovers()
        Await CheckForUpdateAsync()
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        _refreshTimer?.Stop()
        _refreshTimer = Nothing
        _notifier?.Dispose()
        _notifier = Nothing
        RemoveHandler ThemeManager.ThemeChanged, AddressOf UpdateThemeIcon
    End Sub

    Private Sub ThemeToggle_Click(sender As Object, e As RoutedEventArgs)
        ThemeManager.Toggle()
    End Sub

    ''' <summary>深色時顯示太陽（切到淺色），淺色時顯示月亮（切到深色），並同步視窗標題列顏色</summary>
    Private Sub UpdateThemeIcon()
        ThemeToggleButton.Content = If(ThemeManager.IsDarkActive, ChrW(&HE706), ChrW(&HE708))
        ThemeToggleButton.ToolTip = If(ThemeManager.IsDarkActive, "切換為淺色主題", "切換為深色主題")
        ApplyTitleBarTheme()
    End Sub

    Private Sub MainWindow_SourceInitialized(sender As Object, e As EventArgs) Handles Me.SourceInitialized
        ApplyTitleBarTheme()
    End Sub

    ' DWMWA_USE_IMMERSIVE_DARK_MODE：讓系統標題列配合深色主題（Windows 10 20H1 以後）
    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20

    <Runtime.InteropServices.DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(hwnd As IntPtr, attr As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    Private Sub ApplyTitleBarTheme()
        Dim hwnd = New Interop.WindowInteropHelper(Me).Handle
        If hwnd = IntPtr.Zero Then Return
        Dim dark = If(ThemeManager.IsDarkActive, 1, 0)
        Try
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, dark, 4)
        Catch
            ' 舊版 Windows 不支援時忽略
        End Try
    End Sub

    Private Sub InitializeNotifier()
        _notifier = New Notifier(Sub(cfg)
                                     cfg.PositionProvider = New WindowPositionProvider(
                                         parentWindow:=Me,
                                         corner:=Corner.TopRight,
                                         offsetX:=10,
                                         offsetY:=10)
                                     cfg.LifetimeSupervisor = New TimeAndCountBasedLifetimeSupervisor(
                                         notificationLifetime:=TimeSpan.FromSeconds(3),
                                         maximumNotificationCount:=MaximumNotificationCount.FromCount(5))
                                     cfg.Dispatcher = Application.Current.Dispatcher
                                 End Sub)
        _notifierOptions = New MessageOptions With {
            .ShowCloseButton = False,
            .FreezeOnMouseEnter = False
        }
    End Sub

#End Region

#Region "網卡列表與刷新"

    Private Async Sub RefreshTimer_Tick(sender As Object, e As EventArgs)
        If _isBusy Then Return
        Await RefreshAdaptersAsync()
    End Sub

    ''' <summary>在背景執行緒查詢網卡，再回到 UI 執行緒合併進列表</summary>
    Private Async Function RefreshAdaptersAsync() As Task
        If _isRefreshing Then Return
        _isRefreshing = True
        Try
            Dim fresh = Await Task.Run(Function() NetworkService.GetPhysicalAdapters())
            MergeAdapters(fresh)
        Catch ex As Exception
            Debug.WriteLine($"刷新網卡資訊失敗: {ex.Message}")
        Finally
            _isRefreshing = False
        End Try
    End Function

    ''' <summary>增量更新列表：移除消失的、更新既有的、加入新的，避免整個列表重建造成閃爍與選取遺失</summary>
    Private Sub MergeAdapters(fresh As List(Of NetworkAdapterInfo))
        Dim previousId = _currentAdapter?.Id

        For i = _adapters.Count - 1 To 0 Step -1
            Dim id = _adapters(i).Id
            If Not fresh.Any(Function(a) a.Id = id) Then _adapters.RemoveAt(i)
        Next

        For Each item In fresh
            Dim id = item.Id
            Dim existing = _adapters.FirstOrDefault(Function(a) a.Id = id)
            If existing Is Nothing Then
                _adapters.Add(item)
            Else
                existing.UpdateFrom(item)
            End If
        Next

        NoAdapterText.Visibility = If(_adapters.Count = 0, Visibility.Visible, Visibility.Collapsed)
        Dim connectedCount = _adapters.Where(Function(a) a.IsConnected).Count()
        AdapterCountText.Text = $"{_adapters.Count} 張網卡，{connectedCount} 張已連線"

        If AdapterListBox.SelectedItem Is Nothing AndAlso _adapters.Count > 0 Then
            Dim previous = _adapters.FirstOrDefault(Function(a) a.Id = previousId)
            AdapterListBox.SelectedItem = If(previous, _adapters(0))
        End If

        UpdateDetailPanel()
    End Sub

    Private Sub AdapterListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        _currentAdapter = TryCast(AdapterListBox.SelectedItem, NetworkAdapterInfo)
        UpdateDetailPanel()
        FillManualInputs()
    End Sub

    ''' <summary>更新右上角的唯讀資訊（每次刷新都會呼叫）</summary>
    Private Sub UpdateDetailPanel()
        Dim adapter = _currentAdapter
        If adapter Is Nothing Then
            DetailPanel.Visibility = Visibility.Collapsed
            NoSelectionText.Visibility = Visibility.Visible
            Return
        End If

        DetailPanel.Visibility = Visibility.Visible
        NoSelectionText.Visibility = Visibility.Collapsed

        DetailName.Text = adapter.Name
        DetailDescription.Text = adapter.Description
        DetailIP.Text = OrDash(adapter.IPAddress)
        DetailMask.Text = OrDash(adapter.SubnetMask)
        DetailGateway.Text = OrDash(adapter.Gateway)
        DetailDns.Text = OrDash(adapter.DnsServers)
        DetailMAC.Text = OrDash(adapter.MacAddress)
        DetailMode.Text = If(adapter.IsDHCPEnabled, "DHCP 自動取得", "手動設定（靜態）")

        StatusText.Text = If(adapter.IsConnected, "已連線", "未連線")
        StatusDot.SetResourceReference(Shapes.Shape.FillProperty,
                                       If(adapter.IsConnected, "SuccessBrush", "SecondaryForegroundBrush"))
    End Sub

    ''' <summary>把目前網卡的設定帶入手動輸入區。只在切換網卡或套用成功後呼叫，避免覆蓋使用者正在輸入的內容</summary>
    Private Sub FillManualInputs()
        Dim adapter = _currentAdapter
        If adapter Is Nothing Then
            ManualIP.Clear()
            ManualMask.Clear()
            ManualGateway.Clear()
            ManualDns1.Clear()
            ManualDns2.Clear()
            Return
        End If

        ManualIP.IPAddress = adapter.IPAddress
        ManualMask.IPAddress = adapter.SubnetMask
        ManualGateway.IPAddress = adapter.Gateway

        Dim dns = adapter.DnsServers.Split(","c, StringSplitOptions.RemoveEmptyEntries Or StringSplitOptions.TrimEntries)
        ManualDns1.IPAddress = If(dns.Length > 0, dns(0), "")
        ManualDns2.IPAddress = If(dns.Length > 1, dns(1), "")
    End Sub

    Private Shared Function OrDash(value As String) As String
        Return If(String.IsNullOrEmpty(value), "—", value)
    End Function

#End Region

#Region "套用設定"

    Private Async Sub ApplyManual_Click(sender As Object, e As RoutedEventArgs)
        If Not EnsureAdapterSelected() Then Return

        Dim config As New IPConfiguration With {
            .IPAddress = ManualIP.IPAddress,
            .SubnetMask = ManualMask.IPAddress,
            .Gateway = ManualGateway.IPAddress,
            .DNS1 = ManualDns1.IPAddress,
            .DNS2 = ManualDns2.IPAddress
        }
        config.Normalize()

        Dim validationError = IPValidator.Validate(config)
        If validationError IsNot Nothing Then
            _notifier?.ShowWarning(validationError, _notifierOptions)
            Return
        End If

        Await ApplyConfigAsync(config, "已成功套用 IP 設定")
    End Sub

    Private Async Sub SetDHCP_Click(sender As Object, e As RoutedEventArgs)
        If Not EnsureAdapterSelected() Then Return
        Dim adapter = _currentAdapter

        Await RunNetworkOperationAsync(
            Function() NetworkService.SetDHCPAsync(adapter.Name, adapter.IsDHCPEnabled),
            "已切換到 DHCP 模式")
    End Sub

    Private Async Function ApplyConfigAsync(config As IPConfiguration, successMessage As String) As Task
        Dim adapter = _currentAdapter
        Await RunNetworkOperationAsync(
            Function() NetworkService.ApplyIPConfigurationAsync(adapter, config),
            successMessage)
    End Function

    ''' <summary>統一處理：鎖定 UI、執行 netsh、等待設定生效、重新整理、回填輸入區</summary>
    Private Async Function RunNetworkOperationAsync(operation As Func(Of Task), successMessage As String) As Task
        If _isBusy Then Return
        Dim adapter = _currentAdapter
        SetBusy(True)
        Try
            Await operation()
            _notifier?.ShowSuccess(successMessage, _notifierOptions)

            Await Task.Delay(SettleDelay)
            Await RefreshAdaptersAsync()
            If _currentAdapter Is adapter Then FillManualInputs()
        Catch ex As Exception
            _notifier?.ShowError(ex.Message, _notifierOptions)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Function EnsureAdapterSelected() As Boolean
        If _currentAdapter IsNot Nothing Then Return True
        _notifier?.ShowWarning("請先選擇一個網路介面卡", _notifierOptions)
        Return False
    End Function

    Private Sub SetBusy(busy As Boolean)
        _isBusy = busy
        QuickPanel.IsEnabled = Not busy
        ManualPanel.IsEnabled = Not busy
        AdapterListBox.IsEnabled = Not busy
        Cursor = If(busy, Cursors.Wait, Nothing)
    End Sub

#End Region

#Region "快速切換配置"

    Private Sub RenderQuickConfigs()
        QuickConfigPanel.Children.Clear()
        For Each config In _configStorage
            QuickConfigPanel.Children.Add(CreateQuickConfigButton(config))
        Next
        NoConfigText.Visibility = If(_configStorage.Count = 0, Visibility.Visible, Visibility.Collapsed)
    End Sub

    Private Function CreateQuickConfigButton(config As IPConfiguration) As Button
        Dim content As New StackPanel
        content.Children.Add(New TextBlock With {
            .Text = config.ConfigName,
            .FontWeight = FontWeights.SemiBold,
            .HorizontalAlignment = HorizontalAlignment.Center
        })
        Dim ipText As New TextBlock With {
            .Text = config.IPAddress,
            .FontSize = 11,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .Margin = New Thickness(0, 2, 0, 0)
        }
        ' 以資源參照而非固定畫刷，切換主題時顏色才會跟著變
        ipText.SetResourceReference(TextBlock.ForegroundProperty, "SecondaryForegroundBrush")
        content.Children.Add(ipText)

        Dim tooltip = $"IP：{config.IPAddress}{vbCrLf}遮罩：{config.SubnetMask}"
        If Not String.IsNullOrEmpty(config.Gateway) Then tooltip &= $"{vbCrLf}閘道：{config.Gateway}"
        If Not String.IsNullOrEmpty(config.DNS1) Then tooltip &= $"{vbCrLf}DNS：{config.DNS1}" & If(String.IsNullOrEmpty(config.DNS2), "", $", {config.DNS2}")

        Dim btn As New Button With {
            .Content = content,
            .ToolTip = tooltip,
            .Margin = New Thickness(0, 0, 8, 8),
            .Padding = New Thickness(14, 6, 14, 6),
            .MinWidth = 120,
            .Style = TryCast(FindResource("SecondaryButton"), Style)
        }
        AddHandler btn.Click, Async Sub() Await ApplyQuickConfigAsync(config)

        Dim editItem As New MenuItem With {.Header = "編輯"}
        AddHandler editItem.Click, Async Sub() Await EditConfigAsync(config)

        Dim deleteItem As New MenuItem With {.Header = "刪除"}
        AddHandler deleteItem.Click, Sub() DeleteConfig(config)

        Dim menu As New ContextMenu
        menu.Items.Add(editItem)
        menu.Items.Add(deleteItem)
        btn.ContextMenu = menu

        Return btn
    End Function

    Private Async Function ApplyQuickConfigAsync(config As IPConfiguration) As Task
        If Not EnsureAdapterSelected() Then Return

        ' 舊版設定檔可能含有不合法的值，套用前再驗證一次
        Dim validationError = IPValidator.Validate(config)
        If validationError IsNot Nothing Then
            _notifier?.ShowWarning($"配置 '{config.ConfigName}' 無效：{validationError}", _notifierOptions)
            Return
        End If

        Await ApplyConfigAsync(config, $"已成功套用配置 '{config.ConfigName}'")
    End Function

    Private Async Sub AddConfig_Click(sender As Object, e As RoutedEventArgs)
        Dim result = Await ConfigEditorPanel.ShowAsync(Nothing)
        If result Is Nothing Then Return

        _configStorage.Add(result)
        SaveConfigsToFile()
        RenderQuickConfigs()
    End Sub

    Private Async Function EditConfigAsync(config As IPConfiguration) As Task
        Dim result = Await ConfigEditorPanel.ShowAsync(config)
        If result Is Nothing Then Return

        config.CopyFrom(result)
        SaveConfigsToFile()
        RenderQuickConfigs()
    End Function

    Private Sub DeleteConfig(config As IPConfiguration)
        Dim result = MessageBox.Show(Me, $"確定要刪除配置 '{config.ConfigName}' 嗎？", "確認刪除",
                                     MessageBoxButton.YesNo, MessageBoxImage.Question)
        If result = MessageBoxResult.Yes Then
            _configStorage.Remove(config)
            SaveConfigsToFile()
            RenderQuickConfigs()
        End If
    End Sub

#End Region

#Region "自動更新"

    Private _pendingUpdate As UpdateInfo
    Private _updateCts As Threading.CancellationTokenSource

    Private Async Function CheckForUpdateAsync() As Task
        Dim info = Await UpdateService.CheckAsync()
        If info Is Nothing Then Return

        _pendingUpdate = info
        UpdateBannerText.Text = $"有新版本 {info.Version.ToString(3)} 可用（目前 {UpdateService.CurrentVersion.ToString(3)}）"
        UpdateBanner.ToolTip = If(String.IsNullOrWhiteSpace(info.ReleaseNotes), Nothing, info.ReleaseNotes)
        UpdateProgress.Visibility = Visibility.Collapsed
        UpdateNowButton.Visibility = Visibility.Visible
        UpdateDismissButton.Visibility = Visibility.Visible
        UpdateBanner.Visibility = Visibility.Visible
    End Function

    Private Sub UpdateDismiss_Click(sender As Object, e As RoutedEventArgs)
        UpdateBanner.Visibility = Visibility.Collapsed
    End Sub

    Private Async Sub UpdateNow_Click(sender As Object, e As RoutedEventArgs)
        Dim info = _pendingUpdate
        If info Is Nothing Then Return

        If Debugger.IsAttached Then
            _notifier?.ShowInformation("偵錯中不進行自動更新", _notifierOptions)
            Return
        End If

        UpdateNowButton.Visibility = Visibility.Collapsed
        UpdateDismissButton.Visibility = Visibility.Collapsed
        UpdateProgress.Value = 0
        UpdateProgress.Visibility = Visibility.Visible
        UpdateBannerText.Text = $"正在下載 {info.Version.ToString(3)}…"

        _updateCts = New Threading.CancellationTokenSource()
        Try
            Dim progress = New Progress(Of Integer)(Sub(p) UpdateProgress.Value = p)
            Dim newPath = Await UpdateService.DownloadAsync(info, progress, _updateCts.Token)

            UpdateBannerText.Text = "下載完成，正在重新啟動…"
            _refreshTimer?.Stop()
            UpdateService.ApplyAndRestart(newPath)
        Catch ex As Exception
            UpdateBannerText.Text = "更新失敗：" & ex.Message
            UpdateProgress.Visibility = Visibility.Collapsed
            UpdateNowButton.Visibility = Visibility.Visible
            UpdateDismissButton.Visibility = Visibility.Visible
        End Try
    End Sub

#End Region

#Region "設定檔存取"

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {.WriteIndented = True}

    Private Sub LoadConfigsFromFile()
        _configStorage = New List(Of IPConfiguration)
        Try
            Dim path = GetConfigFilePath()
            If Not File.Exists(path) Then
                ' 首次啟動：若這台電腦跑過 V2，依其設定建立四組預設配置
                If LegacyMigration.HasV2Settings() Then
                    _configStorage = LegacyMigration.SeedFromV2()
                    SaveConfigsToFile()
                End If
                Return
            End If

            Dim loaded = JsonSerializer.Deserialize(Of List(Of IPConfiguration))(File.ReadAllText(path))
            If loaded Is Nothing Then Return

            For Each config In loaded
                config.Normalize()
            Next
            _configStorage = loaded
        Catch ex As Exception
            _notifier?.ShowWarning("設定檔讀取失敗，已略過: " & ex.Message, _notifierOptions)
        End Try
    End Sub

    Private Sub SaveConfigsToFile()
        Try
            File.WriteAllText(GetConfigFilePath(), JsonSerializer.Serialize(_configStorage, JsonOptions))
        Catch ex As Exception
            _notifier?.ShowError("儲存配置失敗: " & ex.Message, _notifierOptions)
        End Try
    End Sub

    Private Shared Function GetConfigFilePath() As String
        Dim folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickIPChange")
        Directory.CreateDirectory(folder)
        Return Path.Combine(folder, "configs.json")
    End Function

#End Region

End Class
