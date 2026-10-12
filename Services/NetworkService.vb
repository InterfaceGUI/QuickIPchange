Imports System.Management
Imports System.Net.NetworkInformation
Imports QuickIPChange

Public Class NetworkService

    ' 獲取所有實體網卡
    Public Shared Function GetPhysicalAdapters() As List(Of NetworkAdapterInfo)
        Dim adapters As New List(Of NetworkAdapterInfo)

        Try
            ' 使用 WMI 查詢網卡
            Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled = TRUE")
            Dim adapterConfigs As New Dictionary(Of UInteger, ManagementObject)

            ' 獲取網卡配置資訊
            Dim configSearcher As New ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE")
            For Each configObj As ManagementObject In configSearcher.Get()
                Dim index As UInteger = CUInt(configObj("Index"))
                adapterConfigs(index) = configObj
            Next

            For Each obj As ManagementObject In searcher.Get()
                Try
                    Dim name As String = obj("NetConnectionID")?.ToString()
                    Dim description As String = obj("Description")?.ToString()
                    Dim adapterTypeStr As String = obj("AdapterType")?.ToString().ToLower()
                    Dim manufacturer As String = obj("Manufacturer")?.ToString().ToLower()

                    ' 過濾虛擬網卡
                    If IsVirtualAdapter(description, manufacturer) Then
                        Continue For
                    End If

                    ' 判斷網卡類型
                    Dim adapterType As AdapterType = AdapterType.Other
                    If description IsNot Nothing Then
                        If description.ToLower().Contains("wireless") OrElse description.ToLower().Contains("wi-fi") OrElse description.ToLower().Contains("wifi") Then
                            adapterType = AdapterType.WiFi
                        ElseIf description.ToLower().Contains("ethernet") OrElse adapterTypeStr?.Contains("ethernet") Then
                            adapterType = AdapterType.Ethernet
                        End If
                    End If

                    ' 獲取配置資訊
                    Dim ipAddress As String = ""
                    Dim subnetMask As String = ""
                    Dim gateway As String = ""
                    Dim macAddress As String = ""
                    Dim isDHCP As Boolean = False
                    Dim index As UInteger = CUInt(obj("Index"))

                    If adapterConfigs.ContainsKey(index) Then
                        Dim config = adapterConfigs(index)
                        macAddress = config("MACAddress")?.ToString()
                        isDHCP = CBool(config("DHCPEnabled"))

                        ' 只獲取 IPv4 地址（過濾 IPv6）
                        Dim ipAddresses = TryCast(config("IPAddress"), String())
                        If ipAddresses IsNot Nothing AndAlso ipAddresses.Length > 0 Then
                            ' 尋找第一個 IPv4 地址（不包含冒號）
                            For Each ip In ipAddresses
                                If Not String.IsNullOrEmpty(ip) AndAlso Not ip.Contains(":"c) Then
                                    ipAddress = ip
                                    Exit For
                                End If
                            Next
                        End If

                        ' 只獲取對應 IPv4 的子網路遮罩
                        Dim subnetMasks = TryCast(config("IPSubnet"), String())
                        If subnetMasks IsNot Nothing AndAlso subnetMasks.Length > 0 Then
                            ' 如果有多個地址，找到對應 IPv4 的索引
                            If ipAddresses IsNot Nothing Then
                                Dim ipv4Index = 0
                                For i = 0 To ipAddresses.Length - 1
                                    If ipAddresses(i) = ipAddress Then
                                        ipv4Index = i
                                        Exit For
                                    End If
                                Next
                                If ipv4Index < subnetMasks.Length Then
                                    subnetMask = subnetMasks(ipv4Index)
                                End If
                            Else
                                subnetMask = subnetMasks(0)
                            End If
                        End If

                        ' 只獲取 IPv4 閘道
                        Dim gateways = TryCast(config("DefaultIPGateway"), String())
                        If gateways IsNot Nothing AndAlso gateways.Length > 0 Then
                            ' 尋找第一個 IPv4 閘道
                            For Each gw In gateways
                                If Not String.IsNullOrEmpty(gw) AndAlso Not gw.Contains(":"c) Then
                                    gateway = gw
                                    Exit For
                                End If
                            Next
                        End If
                    End If

                    adapters.Add(New NetworkAdapterInfo With {
                        .Name = If(String.IsNullOrEmpty(name), description, name),
                        .Description = description,
                        .IPAddress = ipAddress,
                        .SubnetMask = subnetMask,
                        .Gateway = gateway,
                        .MacAddress = macAddress,
                        .AdapterType = adapterType,
                        .IsPhysical = True,
                        .IsDHCPEnabled = isDHCP
                    })
                Catch ex As Exception
                    ' 忽略單個網卡的錯誤
                End Try
            Next
        Catch ex As Exception
            Throw New Exception("獲取網卡資訊失敗: " & ex.Message, ex)
        End Try

        Return adapters
    End Function

    ' 判斷是否為虛擬網卡
    Private Shared Function IsVirtualAdapter(description As String, manufacturer As String) As Boolean
        If String.IsNullOrEmpty(description) Then
            Return False
        End If

        Dim lowerDesc = description.ToLower()
        Dim lowerMfg = If(manufacturer, "").ToLower()

        ' 虛擬網卡關鍵字
        Dim virtualKeywords = {"virtual", "vmware", "hyper-v", "virtualbox", "vbox", "tap-windows", "docker", "wsl"}

        For Each keyword In virtualKeywords
            If lowerDesc.Contains(keyword) OrElse lowerMfg.Contains(keyword) Then
                Return True
            End If
        Next

        Return False
    End Function

    ' 套用 IP 配置
    Public Shared Function ApplyIPConfiguration(adapterName As String, config As IPConfiguration) As Boolean
        Try
            ' 設定 IP 位址
            Dim setIPCommand = $"interface ip set address name=""{adapterName}"" static {config.IPAddress} {config.SubnetMask} {config.Gateway}"
            Dim result = ExecuteNetshCommand(setIPCommand)

            If Not result.Success Then
                Throw New Exception(result.ErrorMessage)
            End If

            Return True
        Catch ex As Exception
            Throw New Exception("套用 IP 配置失敗: " & ex.Message, ex)
        End Try
    End Function

    ' 切換到 DHCP
    Public Shared Function SetDHCP(adapterName As String) As Boolean
        Try
            ' 設定 IPv4 DHCP（僅設定 IPv4，不設定 IPv6）
            Dim setDHCPCommand = $"interface ipv4 set address name=""{adapterName}"" dhcp"
            Dim result = ExecuteNetshCommand(setDHCPCommand)

            If Not result.Success Then
                Throw New Exception(result.ErrorMessage)
            End If

            Return True
        Catch ex As Exception
            Throw New Exception("切換到 DHCP 失敗: " & ex.Message, ex)
        End Try
    End Function

    ' 執行 netsh 命令
    Private Shared Function ExecuteNetshCommand(command As String) As (Success As Boolean, ErrorMessage As String)
        Try
            Dim processInfo As New ProcessStartInfo("netsh", command) With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .StandardOutputEncoding = System.Text.Encoding.UTF8,
                .StandardErrorEncoding = System.Text.Encoding.UTF8,
                .Verb = "runas"
            }

            Dim process As Process = Process.Start(processInfo)
            Using process
                Dim output = process.StandardOutput.ReadToEnd()
                Dim errorOutput = process.StandardError.ReadToEnd()
                process.WaitForExit()

                If process.ExitCode = 0 Then
                    Return (True, "")
                Else
                    Return (False, If(String.IsNullOrEmpty(errorOutput), output, errorOutput))
                End If
            End Using
        Catch ex As Exception
            Return (False, ex.Message)
        End Try
    End Function

End Class
