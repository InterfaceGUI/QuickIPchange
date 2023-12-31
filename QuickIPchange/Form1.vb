﻿Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Reflection
Imports System.Security.Principal
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json.Linq

Public Class Form1

    Private Sub getinterfaces()
        Dim nics As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces
        If nics.Length < 0 Or nics Is Nothing Then
            MsgBox("No NICS")
            Exit Sub
        End If
        ComboBox1.Items.Clear()

        For Each netadapter As NetworkInterface In nics
            Dim intproperties As IPInterfaceProperties = netadapter.GetIPProperties()
            ComboBox1.Items.Add(netadapter.Name)
        Next
        GetActiveNetworkInterfaceName()
    End Sub

    Private Sub ComboBox1_DropDown(sender As Object, e As EventArgs) Handles ComboBox1.DropDown
        getinterfaces()

    End Sub


    Private Sub ChangeIPAddress(interfaceName As String, agrs As String
                                )
        Dim processStartInfo As New ProcessStartInfo()
        processStartInfo.FileName = "netsh"
        processStartInfo.Arguments = $"interface ip set address ""{interfaceName}"" {agrs}"
        processStartInfo.RedirectStandardOutput = True
        processStartInfo.UseShellExecute = False
        processStartInfo.CreateNoWindow = True

        Dim process As New Process()
        process.StartInfo = processStartInfo
        process.Start()
        AddHandler process.OutputDataReceived, Sub()
                                                   AddToOutputListBox(process.StandardOutput.ReadToEnd())
                                               End Sub
        'Dim output As String = process.StandardOutput
        process.WaitForExit()

        If process.ExitCode = 0 Then
            AddToOutputListBox("IP address and subnet mask changed successfully.")
        Else
            AddToOutputListBox($"An error occurred")
        End If
        GetInterfaceInfo(interfaceName)
    End Sub

    Private Sub renewDHCP(interfaceName As String, agrs As String)
        Dim processStartInfo As New ProcessStartInfo()
        processStartInfo.FileName = "netsh"
        processStartInfo.Arguments = $"{agrs}"
        processStartInfo.RedirectStandardOutput = True
        processStartInfo.UseShellExecute = False
        processStartInfo.CreateNoWindow = True

        Dim process As New Process()
        process.StartInfo = processStartInfo
        process.Start()
        AddHandler process.OutputDataReceived, Sub()
                                                   AddToOutputListBox(process.StandardOutput.ReadToEnd())
                                               End Sub
        'Dim output As String = process.StandardOutput
        process.WaitForExit()

        If process.ExitCode = 0 Then
            AddToOutputListBox("")
        Else
            AddToOutputListBox($"An error occurred")
        End If

        GetInterfaceInfo(interfaceName)
    End Sub

    Private Sub AddToOutputListBox(message As String)
        ListBox1.Items.Add(message)
    End Sub

    Private Sub GetInterfaceInfo(interfaceName As String)
        Dim processStartInfo As New ProcessStartInfo()
        processStartInfo.FileName = "ipconfig"
        processStartInfo.Arguments = ""
        processStartInfo.RedirectStandardOutput = True
        processStartInfo.UseShellExecute = False
        'processStartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("GBK") ' Set the appropriate encoding for Chinese characters
        processStartInfo.CreateNoWindow = True

        Dim process As New Process()
        process.StartInfo = processStartInfo
        process.Start()

        Dim output As String = process.StandardOutput.ReadToEnd()
        process.WaitForExit()

        Dim interfaceInfoPattern As String = $"({interfaceName}:).+?IPv4 位址[ .]+: (.+?)\r?\n.+?子網路遮罩[ .]+: (.+?)\r?\n.+?預設閘道[ .]+: (.+?)\r?\n"
        Dim match As Match = Regex.Match(output, interfaceInfoPattern, RegexOptions.Singleline)

        If match.Success Then
            Dim ipAddress As String = match.Groups(2).Value
            Dim subnetMask As String = match.Groups(3).Value
            Dim defaultGateway As String = match.Groups(4).Value

            AddMessageToListBox($"介面卡: {interfaceName}")
            AddMessageToListBox($"IPv4 位址: {ipAddress}")
            AddMessageToListBox($"子網路遮罩: {subnetMask}")
            AddMessageToListBox($"預設閘道: {defaultGateway}")
        Else
            AddMessageToListBox($"未找到接口 '{interfaceName}' 信息。")
        End If
    End Sub

    Private Sub AddMessageToListBox(message As String)
        ListBox1.Items.Add(message)
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ChangeIPAddress(ComboBox1.SelectedItem, $"static 192.168.1.{My.Settings.DefultIP} 255.255.255.0 192.168.1.1")
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        ChangeIPAddress(ComboBox1.SelectedItem, $"static 192.168.100.{My.Settings.DefultIP} 255.255.255.0 192.168.100.1")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        ChangeIPAddress(ComboBox1.SelectedItem, "static 192.168.125.199 255.255.255.0 192.168.125.1")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ChangeIPAddress(ComboBox1.SelectedItem, "source = dhcp")
        renewDHCP(ComboBox1.SelectedItem, "/release")
        renewDHCP(ComboBox1.SelectedItem, "/renew")
        'GetInterfaceInfo(ComboBox1.SelectedItem)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ChangeIPAddress(ComboBox1.SelectedItem, $"static 192.168.0.{My.Settings.DefultIP} 255.255.255.0 192.168.0.1")
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If Not BackgroundWorker1.IsBusy() Then
            BackgroundWorker1.RunWorkerAsync(ComboBox1.SelectedItem)
        End If
    End Sub

    Private Sub GetActiveNetworkInterfaceName()
        Dim activeNetworkInterface As NetworkInterface = GetActiveNetworkInterface()

        If activeNetworkInterface IsNot Nothing Then
            Dim interfaceName As String = activeNetworkInterface.Name
            ComboBox1.SelectedItem = $"{interfaceName}"
            AddMessageToListBox1($"正在使用的網卡名稱: {interfaceName}")
        Else
            AddMessageToListBox1("未找到正在使用的網卡。")
        End If
    End Sub

    Private Function GetActiveNetworkInterface() As NetworkInterface
        Dim networkInterfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        For Each networkInterface As NetworkInterface In networkInterfaces
            If networkInterface.OperationalStatus = OperationalStatus.Up Then
                'AndAlso networkInterface.NetworkInterfaceType <> NetworkInterfaceType.Loopback Then
                Return networkInterface
            End If
        Next

        Return Nothing
    End Function

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

    Private Sub ReqUpdata()

    End Sub

    Private Async Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        Timer2.Enabled = False

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

    Private Sub AddMessageToListBox1(message As String)
        ListBox1.Items.Add(message)
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

        Button2.Text = $"設定為 192.168.1.{ My.Settings.DefultIP}"
        Button4.Text = $"設定為 192.168.0.{ My.Settings.DefultIP}"
        Button5.Text = $"設定為 192.168.100.{ My.Settings.DefultIP}"
        ToolStripTextBox1.Text = My.Settings.DefultIP

        getinterfaces()
        Timer1.Enabled = True
        Timer2.Enabled = True
        loaded = True
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Not BackgroundWorker1.IsBusy() Then
            BackgroundWorker1.RunWorkerAsync(ComboBox1.SelectedItem)
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

    Private Sub ToolStripTextBox1_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox1.TextChanged
        If loaded = False Then Return
        If ToolStripTextBox1.Text <> "" Then
            If Val(ToolStripTextBox1.Text) >= 0 And Val(ToolStripTextBox1.Text) <= 255 Then
                My.Settings.DefultIP = ToolStripTextBox1.Text
                My.Settings.Save()
                Button2.Text = $"設定為 192.168.1.{ My.Settings.DefultIP}"
                Button4.Text = $"設定為 192.168.0.{ My.Settings.DefultIP}"
                Button5.Text = $"設定為 192.168.100.{ My.Settings.DefultIP}"
            End If
        End If
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim interfaceName = e.Argument
        Dim processStartInfo As New ProcessStartInfo()
        processStartInfo.FileName = "ipconfig"
        processStartInfo.Arguments = ""
        processStartInfo.RedirectStandardOutput = True
        processStartInfo.UseShellExecute = False
        'processStartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding("GBK") ' Set the appropriate encoding for Chinese characters
        processStartInfo.CreateNoWindow = True

        Dim process As New Process()
        process.StartInfo = processStartInfo
        process.Start()

        Dim output As String = process.StandardOutput.ReadToEnd()
        process.WaitForExit()

        Dim interfaceInfoPattern As String = $"({interfaceName}:).+?IPv4 位址[ .]+: (.+?)\r?\n.+?子網路遮罩[ .]+: (.+?)\r?\n.+?預設閘道[ .]+: (.+?)\r?\n"
        Dim match As Match = Regex.Match(output, interfaceInfoPattern, RegexOptions.Singleline)

        If match.Success Then
            Dim ipAddress As String = match.Groups(2).Value
            Dim subnetMask As String = match.Groups(3).Value
            Dim defaultGateway As String = match.Groups(4).Value
            Me.Invoke(Sub()
                          ListBox1.Items.Clear()
                          AddMessageToListBox($"介面卡: {interfaceName}")
                          AddMessageToListBox($"IPv4 位址: {ipAddress}")
                          AddMessageToListBox($"子網路遮罩: {subnetMask}")
                          AddMessageToListBox($"預設閘道: {defaultGateway}")
                      End Sub)

        Else
            Me.Invoke(Sub()
                          ListBox1.Items.Clear()
                          AddMessageToListBox($"未找到接口 '{interfaceName}' 信息。")
                      End Sub)

        End If


    End Sub


End Class
