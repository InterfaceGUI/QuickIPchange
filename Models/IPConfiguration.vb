''' <summary>
''' 一組可儲存的靜態 IP 設定（以 JSON 存於 %AppData%\QuickIPChange\configs.json）。
''' 閘道與 DNS 可留空；留空表示不設定。
''' </summary>
Public Class IPConfiguration
    Public Property ConfigName As String = ""
    Public Property IPAddress As String = ""
    Public Property SubnetMask As String = ""
    Public Property Gateway As String = ""
    Public Property DNS1 As String = ""
    Public Property DNS2 As String = ""

    ''' <summary>去除空白並把 0.0.0.0 正規化為空字串</summary>
    Public Sub Normalize()
        ConfigName = If(ConfigName, "").Trim()
        IPAddress = Clean(IPAddress)
        SubnetMask = Clean(SubnetMask)
        Gateway = Clean(Gateway)
        DNS1 = Clean(DNS1)
        DNS2 = Clean(DNS2)
    End Sub

    Public Function Clone() As IPConfiguration
        Dim copy As New IPConfiguration
        copy.CopyFrom(Me)
        Return copy
    End Function

    Public Sub CopyFrom(other As IPConfiguration)
        ConfigName = other.ConfigName
        IPAddress = other.IPAddress
        SubnetMask = other.SubnetMask
        Gateway = other.Gateway
        DNS1 = other.DNS1
        DNS2 = other.DNS2
    End Sub

    Private Shared Function Clean(value As String) As String
        Dim v = If(value, "").Trim()
        Return If(v = "0.0.0.0", "", v)
    End Function
End Class
