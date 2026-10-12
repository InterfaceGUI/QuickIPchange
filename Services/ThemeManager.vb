Imports System.IO
Imports System.Text.Json
Imports Microsoft.Win32

Public Enum AppTheme
    System
    Light
    Dark
End Enum

''' <summary>
''' 管理淺色/深色主題：置換 Application 資源中的顏色字典，並把選擇存到 settings.json。
''' 所有樣式都以 DynamicResource 引用畫刷，置換字典後 UI 會即時更新。
''' </summary>
Public Module ThemeManager

    Private Class AppSettings
        Public Property Theme As String = "System"
    End Class

    Private ReadOnly JsonOptions As New JsonSerializerOptions With {.WriteIndented = True}
    Private _initialized As Boolean

    Public Property Current As AppTheme = AppTheme.System

    ''' <summary>目前實際生效的是否為深色。</summary>
    Public ReadOnly Property IsDarkActive As Boolean
        Get
            Return If(Current = AppTheme.System, SystemPrefersDark(), Current = AppTheme.Dark)
        End Get
    End Property

    Public Event ThemeChanged()

    Public Sub Initialize()
        If _initialized Then Return
        _initialized = True

        Current = LoadSetting()
        ApplyBrushes()

        ' 使用「跟隨系統」時，Windows 切換外觀要跟著換
        AddHandler SystemEvents.UserPreferenceChanged,
            Sub(s, e)
                If Current = AppTheme.System AndAlso e.Category = UserPreferenceCategory.General Then
                    Application.Current?.Dispatcher.BeginInvoke(Sub() ApplyBrushes())
                End If
            End Sub
    End Sub

    Public Sub SetTheme(theme As AppTheme)
        Current = theme
        ApplyBrushes()
        SaveSetting()
    End Sub

    ''' <summary>在淺色與深色之間切換（以目前實際顯示的為基準）。</summary>
    Public Sub Toggle()
        SetTheme(If(IsDarkActive, AppTheme.Light, AppTheme.Dark))
    End Sub

    Private Sub ApplyBrushes()
        Dim dicts = Application.Current.Resources.MergedDictionaries
        Dim target As New ResourceDictionary With {
            .Source = New Uri($"Styles/Colors.{If(IsDarkActive, "Dark", "Light")}.xaml", UriKind.Relative)
        }

        Dim existingIndex = -1
        For i = 0 To dicts.Count - 1
            Dim src = dicts(i).Source?.OriginalString
            If src IsNot Nothing AndAlso src.Contains("Colors.", StringComparison.OrdinalIgnoreCase) Then
                existingIndex = i
                Exit For
            End If
        Next

        If existingIndex >= 0 Then
            dicts(existingIndex) = target
        Else
            dicts.Insert(0, target)
        End If

        RaiseEvent ThemeChanged()
    End Sub

    Private Function SystemPrefersDark() As Boolean
        Try
            Using key = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")
                Dim value = key?.GetValue("AppsUseLightTheme")
                Return value IsNot Nothing AndAlso CInt(value) = 0
            End Using
        Catch
            Return True
        End Try
    End Function

    Private Function LoadSetting() As AppTheme
        Try
            Dim path = GetSettingsPath()
            If Not File.Exists(path) Then Return AppTheme.System
            Dim settings = JsonSerializer.Deserialize(Of AppSettings)(File.ReadAllText(path))
            Dim parsed As AppTheme
            If settings IsNot Nothing AndAlso [Enum].TryParse(settings.Theme, ignoreCase:=True, result:=parsed) Then
                Return parsed
            End If
        Catch
        End Try
        Return AppTheme.System
    End Function

    Private Sub SaveSetting()
        Try
            File.WriteAllText(GetSettingsPath(), JsonSerializer.Serialize(New AppSettings With {.Theme = Current.ToString()}, JsonOptions))
        Catch
            ' 存檔失敗不影響使用
        End Try
    End Sub

    Private Function GetSettingsPath() As String
        Dim folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickIPChange")
        Directory.CreateDirectory(folder)
        Return Path.Combine(folder, "settings.json")
    End Function

End Module
