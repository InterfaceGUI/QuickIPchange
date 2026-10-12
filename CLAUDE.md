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
- 自動更新（`Services/UpdateService.vb`）：列出 GitHub releases，只挑主版號為 3 且含 `.exe` 資產的版本，下載後以 `.old`/`.new` 換檔並以管理員重啟。
- 發布規則：推送 `3.x.y` tag 會觸發 `.github/workflows/release-v3.yml` 建置單一 exe 並建立正式 Release。V1/V2 的自動更新讀取 `releases/latest`，因此會自動升級到 V3（自 3.0.2 起開放）；若要暫時不讓舊版升級，把工作流程的 `prerelease` 改為 true。未經使用者同意不要推 tag。
- `Services/LegacyMigration.vb`：首次啟動（無 configs.json）且電腦上有 V2 的 user.config 時，依其 DefultIP 建立 V2 原本的四組快速配置。
