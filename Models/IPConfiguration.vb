Public Class IPConfiguration
    Public Property ConfigName As String
    Public Property IPAddress As String
    Public Property SubnetMask As String
    Public Property Gateway As String
    Public Property DNS1 As String
    Public Property DNS2 As String

    Public Sub New()
        ConfigName = ""
        IPAddress = ""
        SubnetMask = ""
        Gateway = ""
        DNS1 = ""
        DNS2 = ""
    End Sub

    Public Sub New(name As String, ip As String, mask As String, gw As String, Optional dns1 As String = "", Optional dns2 As String = "")
        ConfigName = name
        IPAddress = ip
        SubnetMask = mask
        Gateway = gw
        Me.DNS1 = dns1
        Me.DNS2 = dns2
    End Sub

    ' 序列化為字串以儲存在 My.Settings
    Public Function ToSettingString() As String
        Return $"{ConfigName}|{IPAddress}|{SubnetMask}|{Gateway}|{DNS1}|{DNS2}"
    End Function

    ' 從字串反序列化
    Public Shared Function FromSettingString(settingString As String) As IPConfiguration
        Dim parts = settingString.Split("|"c)
        If parts.Length >= 4 Then
            Return New IPConfiguration With {
                .ConfigName = parts(0),
                .IPAddress = parts(1),
                .SubnetMask = parts(2),
                .Gateway = parts(3),
                .DNS1 = If(parts.Length > 4, parts(4), ""),
                .DNS2 = If(parts.Length > 5, parts(5), "")
            }
        End If
        Return Nothing
    End Function
End Class
