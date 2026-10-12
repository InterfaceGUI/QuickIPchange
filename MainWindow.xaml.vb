Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.IO
Imports System.Text.Json
Imports ToastNotifications
Imports ToastNotifications.Lifetime
Imports ToastNotifications.Position
Imports ToastNotifications.Messages
Imports ToastNotifications.Core

Class MainWindow
    Private _adapters As ObservableCollection(Of NetworkAdapterInfo)
    Private _currentAdapter As NetworkAdapterInfo
    Private _configStorage As New List(Of IPConfiguration)
    Private _refreshTimer As Threading.DispatcherTimer
    Private _isRefreshing As Boolean = False
    Private _notifier As Notifier
    Private _notifier_options As MessageOptions

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        ' 初始化 Toast 通知
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
        _notifier_options = New MessageOptions()

        With _notifier_options

            .ShowCloseButton = False
            .FreezeOnMouseEnter = False
        End With

        LoadAdapters()
        LoadConfigFromSettings()
        StartAutoRefresh()
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
        ' 停止自動刷新計時器
        If _refreshTimer IsNot Nothing Then
            _refreshTimer.Stop()
            _refreshTimer = Nothing
        End If

        ' 清理 Toast 通知
        If _notifier IsNot Nothing Then
            _notifier.Dispose()
            _notifier = Nothing
        End If
    End Sub

    ' 啟動自動刷新
    Private Sub StartAutoRefresh()
        _refreshTimer = New Threading.DispatcherTimer With {
            .Interval = TimeSpan.FromSeconds(3)
        }
        AddHandler _refreshTimer.Tick, AddressOf RefreshTimer_Tick
        _refreshTimer.Start()
    End Sub

    ' 定時刷新事件
    Private Sub RefreshTimer_Tick(sender As Object, e As EventArgs)
        If _isRefreshing Then Return

        ' 如果用戶正在輸入手動 IP 設定，不刷新
        If IsManualInputFocused() Then Return

        _isRefreshing = True

        Try
            RefreshAdapters()
        Finally
            _isRefreshing = False
        End Try
    End Sub

    ' 檢查手動輸入區域是否有焦點
    Private Function IsManualInputFocused() As Boolean
        Return ManualIP.IsKeyboardFocusWithin OrElse
               ManualMask.IsKeyboardFocusWithin OrElse
               ManualGateway.IsKeyboardFocusWithin
    End Function

    ' 刷新網卡列表和資訊
    Private Sub RefreshAdapters()
        Try
            Dim currentAdapters = NetworkService.GetPhysicalAdapters()
            Dim selectedAdapterName = _currentAdapter?.Name

            ' 檢查網卡數量是否變化
            Dim adapterCountChanged = currentAdapters.Count <> _adapters.Count

            ' 檢查網卡名稱列表是否變化
            Dim adapterNamesChanged = False
            If Not adapterCountChanged Then
                Dim currentNames = currentAdapters.Select(Function(a) a.Name).OrderBy(Function(n) n).ToList()
                Dim existingNames = _adapters.Select(Function(a) a.Name).OrderBy(Function(n) n).ToList()
                adapterNamesChanged = Not currentNames.SequenceEqual(existingNames)
            End If

            ' 如果網卡列表變化，重新載入
            If adapterCountChanged OrElse adapterNamesChanged Then
                _adapters.Clear()
                For Each adapter In currentAdapters
                    _adapters.Add(adapter)
                Next

                ' 嘗試重新選擇之前的網卡
                If Not String.IsNullOrEmpty(selectedAdapterName) Then
                    Dim adapter = _adapters.FirstOrDefault(Function(a) a.Name = selectedAdapterName)
                    If adapter IsNot Nothing Then
                        AdapterListBox.SelectedItem = adapter
                    ElseIf _adapters.Count > 0 Then
                        AdapterListBox.SelectedIndex = 0
                    End If
                ElseIf _adapters.Count > 0 AndAlso AdapterListBox.SelectedItem Is Nothing Then
                    AdapterListBox.SelectedIndex = 0
                End If
            Else
                ' 網卡列表未變化，檢查並更新現有網卡的 IP 資訊
                Dim hasChanges = False

                For i = 0 To _adapters.Count - 1
                    Dim existingAdapter = _adapters(i)
                    Dim newAdapter = currentAdapters.FirstOrDefault(Function(a) a.Name = existingAdapter.Name)

                    If newAdapter IsNot Nothing Then
                        ' 檢查並更新 IP 相關資訊
                        If existingAdapter.IPAddress <> newAdapter.IPAddress Then
                            existingAdapter.IPAddress = newAdapter.IPAddress
                            hasChanges = True
                        End If
                        If existingAdapter.SubnetMask <> newAdapter.SubnetMask Then
                            existingAdapter.SubnetMask = newAdapter.SubnetMask
                            hasChanges = True
                        End If
                        If existingAdapter.Gateway <> newAdapter.Gateway Then
                            existingAdapter.Gateway = newAdapter.Gateway
                            hasChanges = True
                        End If
                        If existingAdapter.IsDHCPEnabled <> newAdapter.IsDHCPEnabled Then
                            existingAdapter.IsDHCPEnabled = newAdapter.IsDHCPEnabled
                            hasChanges = True
                        End If
                    End If
                Next

                ' 只有當資料有變化時才更新詳細資訊面板
                If hasChanges AndAlso _currentAdapter IsNot Nothing Then
                    UpdateDetailPanel()
                End If
            End If

        Catch ex As Exception
            ' 刷新失敗時不顯示錯誤，避免干擾用戶
            Debug.WriteLine($"刷新網卡資訊失敗: {ex.Message}")
        End Try
    End Sub

    ' 載入網卡列表
    Private Sub LoadAdapters()
        Try
            _adapters = New ObservableCollection(Of NetworkAdapterInfo)(NetworkService.GetPhysicalAdapters())
            AdapterListBox.ItemsSource = _adapters

            If _adapters.Count > 0 Then
                AdapterListBox.SelectedIndex = 0
            End If
        Catch ex As Exception
            _notifier?.ShowError("載入網卡列表失敗: " & ex.Message, _notifier_options)
        End Try
    End Sub

    ' 網卡選擇變更事件
    Private Sub AdapterListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If AdapterListBox.SelectedItem IsNot Nothing Then
            _currentAdapter = CType(AdapterListBox.SelectedItem, NetworkAdapterInfo)
            UpdateDetailPanel()
            LoadQuickConfigs()
        Else
            _currentAdapter = Nothing
            DetailPanel.Visibility = Visibility.Collapsed
            NoSelectionText.Visibility = Visibility.Visible
        End If
    End Sub

    ' 更新詳細資訊面板
    Private Sub UpdateDetailPanel()
        If _currentAdapter IsNot Nothing Then
            DetailPanel.Visibility = Visibility.Visible
            NoSelectionText.Visibility = Visibility.Collapsed

            ' 顯示網卡名稱，如果是 DHCP 模式則加上標記
            If _currentAdapter.IsDHCPEnabled Then
                DetailName.Text = _currentAdapter.Name & " (DHCP)"
            Else
                DetailName.Text = _currentAdapter.Name
            End If

            DetailIP.Text = If(String.IsNullOrEmpty(_currentAdapter.IPAddress), "無", _currentAdapter.IPAddress)
            DetailMask.Text = If(String.IsNullOrEmpty(_currentAdapter.SubnetMask), "無", _currentAdapter.SubnetMask)
            DetailGateway.Text = If(String.IsNullOrEmpty(_currentAdapter.Gateway), "無", _currentAdapter.Gateway)
            DetailMAC.Text = If(String.IsNullOrEmpty(_currentAdapter.MacAddress), "無", _currentAdapter.MacAddress)

            ' 填入手動設定區域
            ManualIP.IPAddress = If(String.IsNullOrEmpty(_currentAdapter.IPAddress), "", _currentAdapter.IPAddress)
            ManualMask.IPAddress = If(String.IsNullOrEmpty(_currentAdapter.SubnetMask), "", _currentAdapter.SubnetMask)
            ManualGateway.IPAddress = If(String.IsNullOrEmpty(_currentAdapter.Gateway), "", _currentAdapter.Gateway)
        End If
    End Sub

    ' 載入快速切換配置
    Private Sub LoadQuickConfigs()
        QuickConfigPanel.Children.Clear()

        ' 載入全局配置（所有網卡共用）
        For Each config In _configStorage
            AddQuickConfigButton(config)
        Next
    End Sub

    ' 新增快速配置按鈕
    Private Sub AddQuickConfigButton(config As IPConfiguration)
        Dim btn As New Button With {
            .Content = config.ConfigName,
            .Margin = New Thickness(0, 0, 10, 10),
            .Padding = New Thickness(16, 8, 16, 8),
            .Tag = config,
            .Style = TryCast(FindResource("SecondaryButton"), Style)
        }

        ' 左鍵點擊：套用配置
        AddHandler btn.Click, Sub(s, e)
                                  ApplyQuickConfig(config)
                              End Sub

        ' 右鍵選單
        Dim contextMenu As New ContextMenu
        Dim editItem As New MenuItem With {.Header = "編輯"}
        Dim deleteItem As New MenuItem With {.Header = "刪除"}

        AddHandler editItem.Click, Sub(s, e)
                                       EditConfig(config)
                                   End Sub

        AddHandler deleteItem.Click, Sub(s, e)
                                         DeleteConfig(config)
                                     End Sub

        contextMenu.Items.Add(editItem)
        contextMenu.Items.Add(deleteItem)
        btn.ContextMenu = contextMenu

        QuickConfigPanel.Children.Add(btn)
    End Sub

    ' 套用快速配置
    Private Sub ApplyQuickConfig(config As IPConfiguration)
        If _currentAdapter Is Nothing Then
            Return
        End If

        Try
            If NetworkService.ApplyIPConfiguration(_currentAdapter.Name, config) Then
                _notifier?.ShowSuccess($"已成功套用配置 '{config.ConfigName}'", _notifier_options)
                RefreshCurrentAdapter()
            End If
        Catch ex As Exception

            _notifier?.ShowError(ex.Message, _notifier_options)
        End Try
    End Sub

    ' 新增配置按鈕點擊
    Private Sub AddConfig_Click(sender As Object, e As RoutedEventArgs)
        Dim dialog As New ConfigDialog()
        If dialog.ShowDialog() = True Then
            Dim newConfig = dialog.Configuration

            ' 儲存全局配置
            _configStorage.Add(newConfig)
            SaveConfigToSettings()

            ' 新增按鈕
            AddQuickConfigButton(newConfig)
        End If
    End Sub

    ' 編輯配置
    Private Sub EditConfig(config As IPConfiguration)
        Dim dialog As New ConfigDialog(config)
        If dialog.ShowDialog() = True Then
            ' 更新配置
            config.ConfigName = dialog.Configuration.ConfigName
            config.IPAddress = dialog.Configuration.IPAddress
            config.SubnetMask = dialog.Configuration.SubnetMask
            config.Gateway = dialog.Configuration.Gateway

            SaveConfigToSettings()
            LoadQuickConfigs()
        End If
    End Sub

    ' 刪除配置
    Private Sub DeleteConfig(config As IPConfiguration)
        Dim result = MessageBox.Show($"確定要刪除配置 '{config.ConfigName}' 嗎？", "確認刪除", MessageBoxButton.YesNo, MessageBoxImage.Question)

        If result = MessageBoxResult.Yes Then
            _configStorage.Remove(config)
            SaveConfigToSettings()
            LoadQuickConfigs()
        End If
    End Sub

    ' 手動套用 IP 設定
    Private Sub ApplyManual_Click(sender As Object, e As RoutedEventArgs)
        If _currentAdapter Is Nothing Then
            _notifier?.ShowWarning("請先選擇一個網路介面卡", _notifier_options)
            Return
        End If

        ' 驗證輸入（閘道可以留空）
        If String.IsNullOrWhiteSpace(ManualIP.IPAddress) OrElse
           String.IsNullOrWhiteSpace(ManualMask.IPAddress) Then
            _notifier?.ShowWarning("請填寫 IP 位址和子網路遮罩", _notifier_options)
            Return
        End If

        Try
            ' 處理閘道：如果為空或全為 0 則設為 0.0.0.0
            Dim gateway As String = ManualGateway.IPAddress.Trim()
            If String.IsNullOrWhiteSpace(gateway) OrElse gateway = "0.0.0.0" Then
                gateway = "0.0.0.0"
            End If

            Dim config As New IPConfiguration With {
                .IPAddress = ManualIP.IPAddress.Trim(),
                .SubnetMask = ManualMask.IPAddress.Trim(),
                .Gateway = gateway
            }

            If NetworkService.ApplyIPConfiguration(_currentAdapter.Name, config) Then
                _notifier?.ShowSuccess("已成功套用 IP 設定", _notifier_options)
                RefreshCurrentAdapter()
            End If
        Catch ex As Exception
            _notifier?.ShowError(ex.Message, _notifier_options)
        End Try
    End Sub

    ' 切換到 DHCP
    Private Sub SetDHCP_Click(sender As Object, e As RoutedEventArgs)
        If _currentAdapter Is Nothing Then
            _notifier?.ShowWarning("請先選擇一個網路介面卡", _notifier_options)
            Return
        End If

        Try
            If NetworkService.SetDHCP(_currentAdapter.Name) Then
                _notifier?.ShowSuccess("已成功切換到 DHCP 模式", _notifier_options)
                RefreshCurrentAdapter()
            End If
        Catch ex As Exception
            _notifier?.ShowError(ex.Message, _notifier_options)
        End Try
    End Sub

    ' 刷新當前網卡資訊（IP 設定變更後調用）
    Private Sub RefreshCurrentAdapter()
        ' 暫停自動刷新，避免重複刷新
        If _refreshTimer IsNot Nothing Then
            _refreshTimer.Stop()
        End If

        ' 延遲 1 秒後刷新，等待網路設定生效
        Dim timer As New Threading.DispatcherTimer With {
            .Interval = TimeSpan.FromSeconds(1)
        }

        AddHandler timer.Tick, Sub(s, e)
                                   timer.Stop()

                                   ' 執行刷新
                                   RefreshAdapters()

                                   ' 重啟自動刷新
                                   If _refreshTimer IsNot Nothing Then
                                       _refreshTimer.Start()
                                   End If
                               End Sub

        timer.Start()
    End Sub

    ' 從 JSON 檔案載入配置
    Private Sub LoadConfigFromSettings()
        _configStorage.Clear()

        Try
            Dim configPath = GetConfigFilePath()
            If File.Exists(configPath) Then
                Dim json = File.ReadAllText(configPath)
                Dim loadedList = JsonSerializer.Deserialize(Of List(Of IPConfiguration))(json)
                If loadedList IsNot Nothing Then
                    _configStorage = loadedList
                Else
                    _configStorage = New List(Of IPConfiguration)
                End If
            End If
        Catch ex As Exception
            ' 設定檔可能不存在或格式錯誤，忽略
            _configStorage = New List(Of IPConfiguration)
        End Try
    End Sub

    ' 儲存配置到 JSON 檔案
    Private Sub SaveConfigToSettings()
        Try
            Dim configPath = GetConfigFilePath()
            Dim json = JsonSerializer.Serialize(_configStorage, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(configPath, json)
        Catch ex As Exception
            _notifier?.ShowError("儲存配置失敗: " & ex.Message, _notifier_options)
        End Try
    End Sub

    ' 取得配置檔路徑
    Private Function GetConfigFilePath() As String
        Dim appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        Dim appFolder = Path.Combine(appDataPath, "QuickIPChange")
        If Not Directory.Exists(appFolder) Then
            Directory.CreateDirectory(appFolder)
        End If
        Return Path.Combine(appFolder, "configs.json")
    End Function

End Class
