Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Security.Principal
Imports System.Text.RegularExpressions

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



    Private Sub AddMessageToListBox1(message As String)
        ListBox1.Items.Add(message)
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Not IsAdministrator() Then
            RestartAsAdministrator()
        End If
        Button2.Text = $"設定為 192.168.1.{ My.Settings.DefultIP}"
        Button4.Text = $"設定為 192.168.0.{ My.Settings.DefultIP}"
        Button5.Text = $"設定為 192.168.100.{ My.Settings.DefultIP}"

        getinterfaces()
        Timer1.Enabled = True
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
            Application.Exit()
        Catch ex As Exception
            ' Handle the exception if elevation fails
        End Try
    End Sub

    Private Sub ToolStripTextBox1_TextChanged(sender As Object, e As EventArgs) Handles ToolStripTextBox1.TextChanged
        If ToolStripTextBox1.Text <> "" Then
            If Val(ToolStripTextBox1.Text) >= 0 And Val(ToolStripTextBox1.Text) <= 255 Then
                My.Settings.DefultIP = ToolStripTextBox1.Text

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
