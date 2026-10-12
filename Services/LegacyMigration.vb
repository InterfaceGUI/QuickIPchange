Imports System.IO
Imports System.Text.RegularExpressions

''' <summary>
''' 從 V2 自動更新上來時的一次性搬移：V2 的快速按鈕是固定四組、只有最後一碼（My.Settings.DefultIP）可調，
''' 這裡讀出該值並建立對應的四組配置，讓舊使用者更新後不會發現按鈕全部不見。
''' 只在 configs.json 不存在（首次啟動）時呼叫。
''' </summary>
Public Module LegacyMigration

    Private Const DefaultHostOctet As Integer = 253   ' V2 Settings.settings 的預設值

    ''' <summary>回傳依 V2 設定產生的配置；找不到 V2 設定時仍回傳預設四組。</summary>
    Public Function SeedFromV2() As List(Of IPConfiguration)
        Dim host = ReadV2HostOctet()
        Return New List(Of IPConfiguration) From {
            Preset("ABB", $"192.168.125.{host}"),
            Preset("WSI CNC", $"192.168.1.{host}"),
            Preset("WSI AutoLine", $"192.168.100.{host}"),
            Preset("192.168.0.x", $"192.168.0.{host}")
        }
    End Function

    ''' <summary>V2 是否曾在這台電腦上執行過（有留下 user.config）。</summary>
    Public Function HasV2Settings() As Boolean
        Return FindV2UserConfigs().Any()
    End Function

    Private Function Preset(name As String, ip As String) As IPConfiguration
        Return New IPConfiguration With {.ConfigName = name, .IPAddress = ip, .SubnetMask = "255.255.255.0"}
    End Function

    ''' <summary>
    ''' V2 的 My.Settings 存在 %LocalAppData%\QuickIPchange\&lt;exe&gt;_Url_或_Path_&lt;hash&gt;\&lt;版本&gt;\user.config，
    ''' 取最近修改的那一份裡的 DefultIP。
    ''' </summary>
    Private Function ReadV2HostOctet() As Integer
        Try
            For Each file In FindV2UserConfigs()
                Dim m = Regex.Match(IO.File.ReadAllText(file),
                                    "<setting name=""DefultIP""[^>]*>\s*<value>(\d{1,3})</value>",
                                    RegexOptions.IgnoreCase)
                Dim value As Integer
                If m.Success AndAlso Integer.TryParse(m.Groups(1).Value, value) AndAlso value >= 0 AndAlso value <= 255 Then
                    Return value
                End If
            Next
        Catch
        End Try
        Return DefaultHostOctet
    End Function

    Private Function FindV2UserConfigs() As IEnumerable(Of String)
        Dim root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuickIPchange")
        If Not Directory.Exists(root) Then Return Enumerable.Empty(Of String)()
        Try
            Return Directory.EnumerateFiles(root, "user.config", SearchOption.AllDirectories).
                OrderByDescending(Function(f) File.GetLastWriteTimeUtc(f)).
                ToList()
        Catch
            Return Enumerable.Empty(Of String)()
        End Try
    End Function

End Module
