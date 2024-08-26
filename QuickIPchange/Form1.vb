Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Management
Imports System.Reflection
Imports System.Security.Principal
Imports Newtonsoft.Json.Linq

Public Class Form1

    Private Function GetNetworkAdapters() As List(Of NetworkInterface)
        Dim adapters As New List(Of NetworkInterface)
        For Each nic As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
            If nic.OperationalStatus = OperationalStatus.Up AndAlso
           (nic.NetworkInterfaceType = NetworkInterfaceType.Ethernet OrElse
            nic.NetworkInterfaceType = NetworkInterfaceType.Wireless80211) Then
                adapters.Add(nic)
            End If
        Next
        Return adapters
    End Function
    Dim rbtn_list As List(Of Button) = New List(Of Button)

    Private Sub DisplayNetworkAdapters()
        Dim adapters = GetNetworkAdapters()
        Dim rbtn_t As Button = btn_template
        For Each b In rbtn_list
            b.Dispose()
        Next

        For Each adapter In adapters

            Dim rbtn As New Button With {
                .Text = $"{adapter.Name}{vbCrLf}{adapter.Description}{vbCrLf}{adapter.GetPhysicalAddress().ToString()}{vbCrLf}",
                .AutoSize = rbtn_t.AutoSize,
                .Font = rbtn_t.Font,
                .Name = $"rbtn_{adapter.Name.Trim}",
                .Size = rbtn_t.Size,
                .Parent = rbtn_t.Parent,
                .BackColor = rbtn_t.BackColor,
                .FlatStyle = rbtn_t.FlatStyle,
                .ForeColor = rbtn_t.ForeColor,
                .Padding = rbtn_t.Padding,
                .Visible = True,
                .Tag = adapter.Name,
                .ImageAlign = rbtn_t.ImageAlign,
                .TextImageRelation = rbtn_t.TextImageRelation,
                .Image = IIf(adapter.Name = "Wi-Fi", My.Resources.wifi, My.Resources.wired_network_connection)
            }

            AddHandler rbtn.Click, AddressOf rbtn_Clicked

            For Each ipInfo In adapter.GetIPProperties().UnicastAddresses
                If ipInfo.Address.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                    rbtn.Text += $"{ipInfo.Address}{vbCrLf}"
                End If
            Next
            rbtn_list.Add(rbtn)
        Next
    End Sub

    Dim n_edit As String
    Private Sub rbtn_Clicked(sender As Button, e As EventArgs)
        n_edit = sender.Tag
        Dim n_adapter As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces().Where(Function(n) n.Name = n_edit).FirstOrDefault()
        Dim ipv4 As UnicastIPAddressInformation = n_adapter.GetIPProperties().UnicastAddresses.Where(Function(n) n.Address.AddressFamily = Sockets.AddressFamily.InterNetwork).FirstOrDefault()
        Dim gateway = n_adapter.GetIPProperties().GatewayAddresses
        Label1.Text = n_adapter.Name
        Label2.Text = n_adapter.Description
        Label3.Text = n_adapter.GetPhysicalAddress().ToString()
        Label4.Text = ipv4.Address.ToString()
        IpTextBox1.Text = Label4.Text
        IpTextBox2.Text = ipv4.IPv4Mask.ToString()
        If gateway.Count <> 0 Then
            IpTextBox3.Text = gateway.Item(0).Address.ToString()
        Else
            IpTextBox3.Text = ""
        End If
        Timer2.Enabled = True
    End Sub



    Dim loaded As Boolean
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        loaded = False

        If Not IsAdministrator() And Not Debugger.IsAttached Then
            RestartAsAdministrator()
        End If

        Dim currentVersion As Version = Assembly.GetExecutingAssembly().GetName().Version
        ToolStripStatusLabel5.Text = currentVersion.ToString()

        Dim currentProcess As Process = Process.GetCurrentProcess()
        Dim processes() As Process = Process.GetProcessesByName(currentProcess.ProcessName)

        For Each p As Process In processes
            ' 檢查是否進程是當前的進程，如果不是則關閉它
            If p.Id <> currentProcess.Id Then
                Try
                    ' 嘗試正常終止進程
                    p.CloseMainWindow()
                    ' 如果上面的方法失敗，強制終止進程
                    p.Kill()
                Catch ex As Exception
                    ' 處理任何錯誤
                    Console.WriteLine(ex.Message)
                End Try
            End If
        Next
        Timer_update_chk.Enabled = True
        Timer1.Enabled = True
        loaded = True
        DisplayNetworkAdapters()
        ToolStripTextBox2.Text = My.Settings.DefultIP
        Label1.Text = ""
        Label2.Text = ""
        Label3.Text = ""
        Label4.Text = ""
    End Sub


    Private Async Function getGithubReleases() As Task(Of Dictionary(Of String, String))
        Dim result As New Dictionary(Of String, String)
        Dim apiUrl As String = "https://api.github.com/repos/InterfaceGUI/QuickIPchange/releases/latest"
        Dim responseJson As String

        Using wc As New WebClient()
            ' 設置 User-Agent，GitHub API 需要這個 header
            wc.Headers.Add("User-Agent", "request")
            Try
                responseJson = Await wc.DownloadStringTaskAsync(apiUrl)
            Catch ex As Exception
                Return Nothing
            End Try
            'responseJson = wc.DownloadString(apiUrl)
        End Using

        Dim jsonObject As JObject = JObject.Parse(responseJson)

        ' 提取 tag_name
        Dim tagName As String = jsonObject("tag_name").ToString()

        ' 提取 .exe 下載連結
        Dim exeDownloadUrl As String = ""
        For Each asset As JObject In jsonObject("assets")
            If asset("name").ToString().EndsWith(".exe") Then
                exeDownloadUrl = asset("browser_download_url").ToString()
                Exit For
            End If
        Next

        ' 將提取的資訊加入字典
        result.Add("tag_name", tagName)
        result.Add("exe_download_url", exeDownloadUrl)
        result.Add("release_notes", jsonObject("body").ToString())
        ToolStripStatusLabel7.Visible = True
        Return result


    End Function

    Private Async Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer_update_chk.Tick
        Timer_update_chk.Enabled = False

        Dim ReleasesInfo As Dictionary(Of String, String) = Await getGithubReleases()
        If ReleasesInfo Is Nothing Then
            ToolStripStatusLabel7.Visible = True
            Exit Sub
        End If

        Dim latestVersion As Version = New Version(ReleasesInfo.Item("tag_name"))
        Dim currentVersion As Version = Assembly.GetExecutingAssembly().GetName().Version


        Dim comparisonResult As Integer = currentVersion.CompareTo(latestVersion)
        If comparisonResult < 0 Then
            ToolStripStatusLabel6.Text = $"| 有新版本可用：{latestVersion}"
            If Debugger.IsAttached Then Return
            Dim result As DialogResult = MessageBox.Show($"發現新版本：{latestVersion}。是否現在更新?", "有新版本", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                Dim updateForm As New UpdataMsgBox(ReleasesInfo.Item("exe_download_url"), ReleasesInfo.Item("release_notes"))
                updateForm.ShowDialog()
            End If

        ElseIf comparisonResult = 0 Then
            ToolStripStatusLabel6.Text = $"已是最新版本"
            Console.WriteLine($"目前版本 {currentVersion} 已是最新版本")
        Else
            Console.WriteLine($"目前版本 {currentVersion} 高於 GitHub 上的版本 {latestVersion}")
        End If

    End Sub

    Private Function IsAdministrator() As Boolean
        Dim identity As WindowsIdentity = WindowsIdentity.GetCurrent()
        Dim principal As WindowsPrincipal = New WindowsPrincipal(identity)
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)

    End Function

    Private Sub RestartAsAdministrator()
        Dim startInfo As ProcessStartInfo = New ProcessStartInfo()
        startInfo.FileName = Application.ExecutablePath
        startInfo.UseShellExecute = True
        startInfo.Verb = "runas" ' This indicates running as administrator
        Try
            Dim process As Process = New Process()
            process.StartInfo = startInfo
            process.Start()
            'Me.Dispose()
            End
            'Application.Exit()
        Catch ex As Exception
            ' Handle the exception if elevation fails
        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        DisplayNetworkAdapters()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SetIPv4Address(Label1.Text, IpTextBox1.Text, IpTextBox2.Text, IpTextBox3.Text)
    End Sub

    Private Sub EnableDHCP(adapterName As String)
        Try
            Dim processStartInfo As New ProcessStartInfo()
            processStartInfo.FileName = "netsh"
            processStartInfo.Arguments = $"interface ip set address ""{adapterName}"" dhcp"
            processStartInfo.RedirectStandardOutput = True
            processStartInfo.UseShellExecute = False
            processStartInfo.CreateNoWindow = True

            Dim process As New Process()
            process.StartInfo = processStartInfo
            process.Start()
            Dim output As String = process.StandardOutput.ReadToEnd()
            Debug.WriteLine(output)

            process.WaitForExit()
            If process.ExitCode = 0 Then
                MessageBox.Show($"IPv4 設定已更新 {output}")
            Else
                MessageBox.Show($"An error occurred: {output}")
            End If

            ' MessageBox.Show("IPv4 設定已更新")
        Catch ex As Exception
            MessageBox.Show("設定 IPv4 時發生錯誤: " & ex.Message)
        End Try
    End Sub
    Private Sub SetIPv4Address(adapterName As String, ip As String, subnet As String, gateway As String)
        Try
            Dim processStartInfo As New ProcessStartInfo()
            processStartInfo.FileName = "netsh"
            processStartInfo.Arguments = $"interface ip set address ""{adapterName}"" static {ip} {subnet} {gateway}"
            processStartInfo.RedirectStandardOutput = True
            processStartInfo.UseShellExecute = False
            processStartInfo.CreateNoWindow = True

            Dim process As New Process()
            process.StartInfo = processStartInfo
            process.Start()
            Dim output As String = process.StandardOutput.ReadToEnd()
            Debug.WriteLine(output)

            process.WaitForExit()
            If process.ExitCode = 0 Then
                MessageBox.Show($"IPv4 設定已更新 {output}")
            Else
                MessageBox.Show($"An error occurred: {output}")
            End If

            ' MessageBox.Show("IPv4 設定已更新")
        Catch ex As Exception
            MessageBox.Show("設定 IPv4 時發生錯誤: " & ex.Message)
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles btn_dhcp.Click
        EnableDHCP(Label1.Text)
    End Sub

    Private Sub Timer2_Tick_1(sender As Object, e As EventArgs) Handles Timer2.Tick
        Dim n_adapter As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces().Where(Function(n) n.Name = n_edit).FirstOrDefault()
        Dim ipv4 As UnicastIPAddressInformation = n_adapter.GetIPProperties().UnicastAddresses.Where(Function(n) n.Address.AddressFamily = Sockets.AddressFamily.InterNetwork).FirstOrDefault()
        Label1.Text = n_adapter.Name
        Label2.Text = n_adapter.Description
        Label3.Text = n_adapter.GetPhysicalAddress().ToString()
        If ipv4 IsNot Nothing Then
            Label4.Text = ipv4.Address.ToString()
        Else
            Label4.Text = ""
        End If
    End Sub

    Private Sub Button_Click(sender As Button, e As EventArgs) Handles Button2.Click, Button3.Click, Button4.Click, Button5.Click
        SetIPv4Address(Label1.Text, $"192.168.{sender.Tag}.{ My.Settings.DefultIP}", IpTextBox2.Text, IpTextBox3.Text)
    End Sub

    Private Sub ToolStripTextBox2_Click(sender As Object, e As EventArgs) Handles ToolStripTextBox2.TextChanged
        If ToolStripTextBox2.Text <> "" Then
            If Val(ToolStripTextBox2.Text) >= 0 And Val(ToolStripTextBox2.Text) <= 255 Then
                My.Settings.DefultIP = ToolStripTextBox2.Text
                My.Settings.Save()
                Button2.Text = $"ABB (192.168.125.{ My.Settings.DefultIP})"
                Button3.Text = $"WSI CNC(192.168.1.{ My.Settings.DefultIP})"
                Button4.Text = $"WSI AutoLine (192.168.100.{ My.Settings.DefultIP})"
                Button5.Text = $"192.168.0.{ My.Settings.DefultIP}"
            End If
        End If
    End Sub
End Class
