# CLAUDE.md

QuickIPChange V3 是一個 Windows WPF 桌面程式（VB.NET、.NET 8），用來快速切換本機網卡的 IPv4 設定。
V1/V2（WinForms）位於同一 GitHub repo 的 `master` / `v2` 分支；V3 在 `v3` 分支。

## 建置

```
dotnet build -c Debug
```

程式需要系統管理員權限（`app.manifest` 設為 `requireAdministrator`），從 Visual Studio 偵錯時 VS 會提示以管理員身分重啟。

## 架構

- `Services/NetworkService.vb`：讀取網卡（`System.Net.NetworkInformation`）與透過 `netsh interface ipv4` 套用設定，全部為 `Async`。
- `Services/IPValidator.vb`：IPv4 / 遮罩 / 整組設定的驗證，UI 層套用前一律先呼叫 `IPValidator.Validate`。
- `Models/`：`NetworkAdapterInfo`（列表資料、以介面 GUID `Id` 比對）、`IPConfiguration`（可儲存的靜態設定，含 DNS）。
- `MainWindow`：每 3 秒在背景刷新網卡並增量合併進列表；手動輸入區只在切換網卡或套用成功後回填，不會被定時刷新覆蓋。
- `IPAddressControl`：四段式 IP 輸入；四段皆空時 `IPAddress` 回傳空字串。
- 顏色一律以 `DynamicResource` 引用 `Styles/Colors.Dark.xaml` / `Styles/Colors.Light.xaml` 的畫刷（控制項樣式在 `Styles/Theme.xaml`），不要在 XAML 硬編碼色碼。`Services/ThemeManager.vb` 在執行期置換顏色字典切換主題，選擇存於 `%AppData%\QuickIPChange\settings.json`（預設跟隨系統）。

## 慣例

- 閘道、DNS 留空代表「不設定」；閘道留空時 netsh 使用 `gateway=none`。
- 快速配置以 JSON 存於 `%AppData%\QuickIPChange\configs.json`。
- 不要建立 GitHub Release 或推 tag：V1/V2 的自動更新機制會讀取 `releases/latest`。
