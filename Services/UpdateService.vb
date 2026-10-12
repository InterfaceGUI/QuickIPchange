Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Reflection
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks

Public Class UpdateInfo
    Public Property Version As Version
    Public Property TagName As String
    Public Property DownloadUrl As String
    Public Property ReleaseNotes As String
End Class

''' <summary>
''' 自動更新。與 V1/V2 共用同一個 GitHub repo，但規則不同：
'''  - V1/V2 讀 releases/latest，該端點會略過 pre-release；
'''  - V3 一律以 pre-release 發布，並在這裡自行列出 releases、只挑主版號相同（3.x）的版本，
'''    因此兩邊互不干擾。
''' 下載後比照 V2：把目前 exe 改名為 .old、新檔改為原名，再以系統管理員身分重啟。
''' </summary>
Public Module UpdateService

    Private Const ReleasesApi As String = "https://api.github.com/repos/InterfaceGUI/QuickIPchange/releases?per_page=30"

    Private ReadOnly Http As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(15)}

    Sub New()
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("QuickIPChange/3")
        Http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
    End Sub

    Public ReadOnly Property CurrentVersion As Version
        Get
            Return Normalize(Assembly.GetExecutingAssembly().GetName().Version)
        End Get
    End Property

    ''' <summary>回傳比目前版本新的同主版號 release；沒有或失敗時回傳 Nothing。</summary>
    Public Async Function CheckAsync() As Task(Of UpdateInfo)
        Try
            Dim json = Await Http.GetStringAsync(ReleasesApi)
            Dim best As UpdateInfo = Nothing

            Using doc = JsonDocument.Parse(json)
                For Each release In doc.RootElement.EnumerateArray()
                    If GetBool(release, "draft") Then Continue For

                    Dim tag = GetString(release, "tag_name")
                    Dim parsed As Version = Nothing
                    If Not Version.TryParse(tag?.TrimStart("v"c, "V"c), parsed) Then Continue For
                    parsed = Normalize(parsed)
                    If parsed.Major <> CurrentVersion.Major Then Continue For

                    Dim exeUrl = FindExeAsset(release)
                    If exeUrl Is Nothing Then Continue For

                    If best Is Nothing OrElse parsed > best.Version Then
                        best = New UpdateInfo With {
                            .Version = parsed,
                            .TagName = tag,
                            .DownloadUrl = exeUrl,
                            .ReleaseNotes = GetString(release, "body")
                        }
                    End If
                Next
            End Using

            If best IsNot Nothing AndAlso best.Version > CurrentVersion Then Return best
            Return Nothing
        Catch
            Return Nothing
        End Try
    End Function

    ''' <summary>下載新版到 exe 旁的 .new 檔，progress 回報 0~100。</summary>
    Public Async Function DownloadAsync(info As UpdateInfo, progress As IProgress(Of Integer), token As CancellationToken) As Task(Of String)
        Dim newPath = GetSiblingPath(".new")
        If File.Exists(newPath) Then File.Delete(newPath)

        Using response = Await Http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token)
            response.EnsureSuccessStatusCode()
            Dim total = response.Content.Headers.ContentLength
            Using source = Await response.Content.ReadAsStreamAsync(token)
                Using target = File.Create(newPath)
                    Dim buffer(81919) As Byte
                    Dim readTotal As Long = 0
                    Do
                        Dim read = Await source.ReadAsync(buffer, token)
                        If read = 0 Then Exit Do
                        Await target.WriteAsync(buffer.AsMemory(0, read), token)
                        readTotal += read
                        If total.HasValue AndAlso total.Value > 0 Then
                            progress?.Report(CInt(readTotal * 100 \ total.Value))
                        End If
                    Loop
                End Using
            End Using
        End Using

        progress?.Report(100)
        Return newPath
    End Function

    ''' <summary>以下載好的檔案取代目前 exe，並以系統管理員身分重新啟動。</summary>
    Public Sub ApplyAndRestart(newPath As String)
        Dim currentPath = Environment.ProcessPath
        Dim oldPath = GetSiblingPath(".old")

        If File.Exists(oldPath) Then File.Delete(oldPath)
        File.Move(currentPath, oldPath)
        File.Move(newPath, currentPath)

        Process.Start(New ProcessStartInfo With {
            .FileName = currentPath,
            .UseShellExecute = True,
            .Verb = "runas"
        })
        Application.Current.Shutdown()
    End Sub

    ''' <summary>啟動時清掉上次更新留下的 .old / .new。</summary>
    Public Sub CleanupLeftovers()
        For Each suffix In {".old", ".new"}
            Try
                Dim p = GetSiblingPath(suffix)
                If File.Exists(p) Then File.Delete(p)
            Catch
                ' 仍被鎖定時下次再清
            End Try
        Next
    End Sub

    Private Function GetSiblingPath(suffix As String) As String
        Return Environment.ProcessPath & suffix
    End Function

    Private Function FindExeAsset(release As JsonElement) As String
        Dim assets As JsonElement
        If Not release.TryGetProperty("assets", assets) Then Return Nothing
        For Each asset In assets.EnumerateArray()
            Dim name = GetString(asset, "name")
            If name IsNot Nothing AndAlso name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) Then
                Return GetString(asset, "browser_download_url")
            End If
        Next
        Return Nothing
    End Function

    Private Function GetString(el As JsonElement, name As String) As String
        Dim v As JsonElement
        If el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then Return v.GetString()
        Return Nothing
    End Function

    Private Function GetBool(el As JsonElement, name As String) As Boolean
        Dim v As JsonElement
        Return el.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.True
    End Function

    ''' <summary>把缺少的段落補 0，避免 3.0.0 與 3.0.0.0 比較時被判定不同。</summary>
    Private Function Normalize(v As Version) As Version
        Return New Version(v.Major, Math.Max(v.Minor, 0), Math.Max(v.Build, 0), Math.Max(v.Revision, 0))
    End Function

End Module
