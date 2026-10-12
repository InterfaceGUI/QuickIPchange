Imports System.Net

''' <summary>
''' IPv4 位址、子網路遮罩與整組設定的驗證邏輯。
''' </summary>
Public Module IPValidator

    ''' <summary>空白或 0.0.0.0 都視為「未填」</summary>
    Public Function IsEmptyOrZero(value As String) As Boolean
        Return String.IsNullOrWhiteSpace(value) OrElse value.Trim() = "0.0.0.0"
    End Function

    ''' <summary>嚴格解析四段點分十進位 IPv4（不接受 "1.2" 這類簡寫）</summary>
    Public Function TryParseIPv4(value As String, ByRef result As IPAddress) As Boolean
        result = Nothing
        If String.IsNullOrWhiteSpace(value) Then Return False

        Dim parts = value.Trim().Split("."c)
        If parts.Length <> 4 Then Return False

        Dim bytes(3) As Byte
        For i = 0 To 3
            Dim n As Integer
            If Not Integer.TryParse(parts(i), n) OrElse n < 0 OrElse n > 255 Then Return False
            bytes(i) = CByte(n)
        Next

        result = New IPAddress(bytes)
        Return True
    End Function

    ''' <summary>可作為主機、閘道或 DNS 的單點位址（排除 0.x、多點傳播與廣播）</summary>
    Public Function IsValidHostAddress(value As String) As Boolean
        Dim ip As IPAddress = Nothing
        If Not TryParseIPv4(value, ip) Then Return False
        Dim b = ip.GetAddressBytes()
        Return b(0) <> 0 AndAlso b(0) < 224
    End Function

    ''' <summary>子網路遮罩必須是連續的 1 後接連續的 0（例如 255.255.254.0）</summary>
    Public Function IsValidSubnetMask(value As String) As Boolean
        Dim ip As IPAddress = Nothing
        If Not TryParseIPv4(value, ip) Then Return False

        Dim b = ip.GetAddressBytes()
        Dim bits As UInteger = (CUInt(b(0)) << 24) Or (CUInt(b(1)) << 16) Or (CUInt(b(2)) << 8) Or CUInt(b(3))
        If bits = 0UI Then Return False

        ' 取反後應為 2^n - 1，即 inverted And (inverted + 1) = 0
        Dim inverted As UInteger = Not bits
        Return (inverted And (inverted + 1UI)) = 0UI
    End Function

    ''' <summary>
    ''' 驗證整組靜態 IP 設定。回傳 Nothing 表示通過，否則回傳可直接顯示的錯誤訊息。
    ''' </summary>
    Public Function Validate(config As IPConfiguration) As String
        If IsEmptyOrZero(config.IPAddress) Then Return "請輸入 IP 位址"
        If Not IsValidHostAddress(config.IPAddress) Then Return "IP 位址格式不正確"

        If IsEmptyOrZero(config.SubnetMask) Then Return "請輸入子網路遮罩"
        If Not IsValidSubnetMask(config.SubnetMask) Then Return "子網路遮罩不正確（例如 255.255.255.0）"

        If Not IsEmptyOrZero(config.Gateway) Then
            If Not IsValidHostAddress(config.Gateway) Then Return "預設閘道格式不正確"
            If config.Gateway.Trim() = config.IPAddress.Trim() Then Return "預設閘道不可與 IP 位址相同"
        End If

        If Not IsEmptyOrZero(config.DNS1) AndAlso Not IsValidHostAddress(config.DNS1) Then Return "慣用 DNS 格式不正確"
        If Not IsEmptyOrZero(config.DNS2) AndAlso Not IsValidHostAddress(config.DNS2) Then Return "其他 DNS 格式不正確"

        Return Nothing
    End Function

End Module
