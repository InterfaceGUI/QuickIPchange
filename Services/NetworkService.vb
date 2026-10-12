Imports System.Diagnostics
Imports System.Globalization
Imports Microsoft.Win32
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading.Tasks

''' <summary>
''' 讀取網卡資訊（System.Net.NetworkInformation）與透過 netsh 套用 IPv4 設定。
''' 所有 netsh 操作皆為非同步，不會阻塞 UI 執行緒。
''' </summary>
Public Class NetworkService

    Shared Sub New()
        ' netsh 輸出使用 OEM 字碼頁（繁中為 950），需註冊 CodePages 才能正確解碼錯誤訊息
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
    End Sub

    Private Shared ReadOnly VirtualKeywords As String() =
        {"virtual", "vmware", "hyper-v", "virtualbox", "vbox", "tap-windows", "docker", "wsl", "loopback", "bluetooth", "tunnel", "vpn"}

    ''' <summary>取得實體網卡（有線、無線），包含尚未連線的，以便事先設定 IP</summary>
    Public Shared Function GetPhysicalAdapters() As List(Of NetworkAdapterInfo)
        Dim adapters As New List(Of NetworkAdapterInfo)

        For Each nic In NetworkInterface.GetAllNetworkInterfaces()
            Try
                Dim adapterType = MapAdapterType(nic.NetworkInterfaceType)
                If Not adapterType.HasValue Then Continue For
                If IsVirtualAdapter(nic.Description) Then Continue For

                Dim props = nic.GetIPProperties()

                ' 可能同時存在靜態位址與 169.254.x 自動私有位址（APIPA），優先取非 APIPA 的
                Dim unicast = props.UnicastAddresses.
                    Where(Function(u) u.Address.AddressFamily = AddressFamily.InterNetwork).
                    OrderBy(Function(u) If(IsLinkLocal(u.Address), 1, 0)).
                    FirstOrDefault()
                Dim gateway = props.GatewayAddresses.
                    Select(Function(g) g.Address).
                    FirstOrDefault(Function(a) a.AddressFamily = AddressFamily.InterNetwork)
                Dim dns = props.DnsAddresses.
                    Where(Function(a) a.AddressFamily = AddressFamily.InterNetwork).
                    Select(Function(a) a.ToString())

                Dim isDhcp = False
                Try
                    isDhcp = props.GetIPv4Properties()?.IsDhcpEnabled
                Catch
                    ' 介面未綁定 IPv4 時會擲出例外，視為非 DHCP
                End Try

                adapters.Add(New NetworkAdapterInfo With {
                    .Id = nic.Id,
                    .Name = nic.Name,
                    .Description = nic.Description,
                    .MacAddress = FormatMac(nic.GetPhysicalAddress()),
                    .AdapterType = adapterType.Value,
                    .IsConnected = (nic.OperationalStatus = OperationalStatus.Up),
                    .IPAddress = If(unicast?.Address.ToString(), ""),
                    .SubnetMask = If(unicast?.IPv4Mask?.ToString(), ""),
                    .Gateway = If(gateway?.ToString(), ""),
                    .DnsServers = String.Join(", ", dns),
                    .IsDHCPEnabled = isDhcp
                })
            Catch
                ' 忽略單一網卡的讀取錯誤
            End Try
        Next

        Return adapters.OrderByDescending(Function(a) a.IsConnected).ThenBy(Function(a) a.AdapterType).ThenBy(Function(a) a.Name).ToList()
    End Function

    Private Shared Function MapAdapterType(type As NetworkInterfaceType) As AdapterType?
        Select Case type
            Case NetworkInterfaceType.Ethernet, NetworkInterfaceType.Ethernet3Megabit,
                 NetworkInterfaceType.FastEthernetT, NetworkInterfaceType.FastEthernetFx,
                 NetworkInterfaceType.GigabitEthernet
                Return AdapterType.Ethernet
            Case NetworkInterfaceType.Wireless80211
                Return AdapterType.WiFi
            Case NetworkInterfaceType.Loopback, NetworkInterfaceType.Tunnel, NetworkInterfaceType.Ppp,
                 NetworkInterfaceType.Unknown
                Return Nothing
            Case Else
                Return AdapterType.Other
        End Select
    End Function

    Private Shared Function IsLinkLocal(address As Net.IPAddress) As Boolean
        Dim b = address.GetAddressBytes()
        Return b.Length = 4 AndAlso b(0) = 169 AndAlso b(1) = 254
    End Function

    Private Shared Function IsVirtualAdapter(description As String) As Boolean
        If String.IsNullOrEmpty(description) Then Return False
        Dim lower = description.ToLowerInvariant()
        Return VirtualKeywords.Any(Function(k) lower.Contains(k))
    End Function

    Private Shared Function FormatMac(address As PhysicalAddress) As String
        Dim bytes = address?.GetAddressBytes()
        If bytes Is Nothing OrElse bytes.Length = 0 Then Return ""
        Return String.Join(":", bytes.Select(Function(b) b.ToString("X2")))
    End Function

    ''' <summary>套用靜態 IP。閘道留空則移除預設閘道；DNS 皆留空則不變更 DNS。</summary>
    Public Shared Async Function ApplyIPConfigurationAsync(adapter As NetworkAdapterInfo, config As IPConfiguration) As Task
        Dim adapterName = adapter.Name
        Dim gatewayArg = If(IPValidator.IsEmptyOrZero(config.Gateway), "none", config.Gateway.Trim())

        Await RunNetshAsync(
            $"interface ipv4 set address name=""{adapterName}"" source=static address={config.IPAddress.Trim()} mask={config.SubnetMask.Trim()} gateway={gatewayArg}",
            "設定 IP 位址")

        Dim dnsList = {config.DNS1, config.DNS2}.
            Where(Function(d) Not IPValidator.IsEmptyOrZero(d)).
            Select(Function(d) d.Trim()).
            ToList()

        If dnsList.Count > 0 Then
            Await RunNetshAsync(
                $"interface ipv4 set dnsservers name=""{adapterName}"" source=static address={dnsList(0)} register=primary validate=no",
                "設定 DNS")
            For i = 1 To dnsList.Count - 1
                Await RunNetshAsync(
                    $"interface ipv4 add dnsservers name=""{adapterName}"" address={dnsList(i)} index={i + 1} validate=no",
                    "新增 DNS")
            Next
        End If

        ' Windows 限制：網卡未連線時 netsh / WMI 都不會真的關閉 DHCP（登錄檔 EnableDHCP 仍為 1），
        ' 插線後仍會走 DHCP。這裡比照「網路連線」控制台的做法直接寫登錄檔。
        If Not adapter.IsConnected AndAlso IsDhcpEnabledNow(adapter.Id) Then
            SetRegistryDhcpFlag(adapter.Id, enabled:=False)
        End If
    End Function

    Private Shared Function IsDhcpEnabledNow(interfaceId As String) As Boolean
        Try
            Dim nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(Function(n) n.Id = interfaceId)
            Return If(nic?.GetIPProperties().GetIPv4Properties()?.IsDhcpEnabled, False)
        Catch
            Return False
        End Try
    End Function

    Private Shared Sub SetRegistryDhcpFlag(interfaceId As String, enabled As Boolean)
        Dim keyPath = $"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{interfaceId}"
        Using key = Registry.LocalMachine.OpenSubKey(keyPath, writable:=True)
            If key Is Nothing Then Throw New InvalidOperationException("找不到介面的 TCP/IP 登錄檔設定")
            key.SetValue("EnableDHCP", If(enabled, 1, 0), RegistryValueKind.DWord)
        End Using
    End Sub

    ''' <summary>切換為 DHCP（位址與 DNS）</summary>
    Public Shared Async Function SetDHCPAsync(adapterName As String, isAlreadyDhcp As Boolean) As Task
        ' 已是 DHCP 時 netsh 會以非零代碼回報「DHCP 已啟用」，故跳過
        If Not isAlreadyDhcp Then
            Await RunNetshAsync($"interface ipv4 set address name=""{adapterName}"" source=dhcp", "切換為 DHCP")
        End If

        Try
            Await RunNetshAsync($"interface ipv4 set dnsservers name=""{adapterName}"" source=dhcp", "切換 DNS 為 DHCP")
        Catch
            ' DNS 已是 DHCP 時部分系統會回報錯誤，不影響結果
        End Try
    End Function

    Private Shared Async Function RunNetshAsync(arguments As String, stepName As String) As Task
        Dim encoding = GetOemEncoding()
        Dim psi As New ProcessStartInfo("netsh", arguments) With {
            .CreateNoWindow = True,
            .UseShellExecute = False,
            .RedirectStandardOutput = True,
            .RedirectStandardError = True,
            .StandardOutputEncoding = encoding,
            .StandardErrorEncoding = encoding
        }

        Using proc = Process.Start(psi)
            Dim outputTask = proc.StandardOutput.ReadToEndAsync()
            Dim errorTask = proc.StandardError.ReadToEndAsync()
            Await proc.WaitForExitAsync()

            If proc.ExitCode <> 0 Then
                Dim output = (Await outputTask).Trim()
                Dim errorOutput = (Await errorTask).Trim()
                Dim message = If(String.IsNullOrEmpty(errorOutput), output, errorOutput)
                If String.IsNullOrEmpty(message) Then message = $"netsh 結束代碼 {proc.ExitCode}"
                Throw New InvalidOperationException($"{stepName}失敗：{message}")
            End If
        End Using
    End Function

    Private Shared Function GetOemEncoding() As Encoding
        Try
            Return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
        Catch
            Return Encoding.Default
        End Try
    End Function

End Class
