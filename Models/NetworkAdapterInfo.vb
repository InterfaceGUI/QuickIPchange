Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class NetworkAdapterInfo
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ''' <summary>作業系統的介面 GUID，重新整理時用來比對同一張網卡</summary>
    Public Property Id As String
    ''' <summary>連線名稱（netsh 使用的 name=）</summary>
    Public Property Name As String
    Public Property Description As String
    Public Property MacAddress As String
    Public Property AdapterType As AdapterType

    Private _ipAddress As String = ""
    Private _subnetMask As String = ""
    Private _gateway As String = ""
    Private _dnsServers As String = ""
    Private _isDHCPEnabled As Boolean
    Private _isConnected As Boolean

    Public Property IPAddress As String
        Get
            Return _ipAddress
        End Get
        Set(value As String)
            If SetField(_ipAddress, value) Then OnPropertyChanged(NameOf(DisplayIPInfo))
        End Set
    End Property

    Public Property SubnetMask As String
        Get
            Return _subnetMask
        End Get
        Set(value As String)
            SetField(_subnetMask, value)
        End Set
    End Property

    Public Property Gateway As String
        Get
            Return _gateway
        End Get
        Set(value As String)
            SetField(_gateway, value)
        End Set
    End Property

    Public Property DnsServers As String
        Get
            Return _dnsServers
        End Get
        Set(value As String)
            SetField(_dnsServers, value)
        End Set
    End Property

    Public Property IsDHCPEnabled As Boolean
        Get
            Return _isDHCPEnabled
        End Get
        Set(value As Boolean)
            If SetField(_isDHCPEnabled, value) Then OnPropertyChanged(NameOf(DisplayIPInfo))
        End Set
    End Property

    Public Property IsConnected As Boolean
        Get
            Return _isConnected
        End Get
        Set(value As Boolean)
            If SetField(_isConnected, value) Then OnPropertyChanged(NameOf(DisplayIPInfo))
        End Set
    End Property

    ' 網卡圖標（Segoe MDL2 Assets 字型）
    Public ReadOnly Property AdapterIcon As String
        Get
            Select Case AdapterType
                Case AdapterType.Ethernet
                    Return ChrW(&HE839)
                Case AdapterType.WiFi
                    Return ChrW(&HE701)
                Case Else
                    Return ChrW(&HE968)
            End Select
        End Get
    End Property

    ' 列表上顯示的 IP 摘要
    Public ReadOnly Property DisplayIPInfo As String
        Get
            Dim state = If(IsConnected, "", "未連線 · ")
            If String.IsNullOrEmpty(IPAddress) Then
                Return state & If(IsDHCPEnabled, "DHCP（尚未取得位址）", "無 IP")
            End If
            Return state & If(IsDHCPEnabled, $"{IPAddress}（DHCP）", IPAddress)
        End Get
    End Property

    ''' <summary>以新查到的資料更新可變欄位，只在值改變時觸發通知</summary>
    Public Sub UpdateFrom(other As NetworkAdapterInfo)
        IPAddress = other.IPAddress
        SubnetMask = other.SubnetMask
        Gateway = other.Gateway
        DnsServers = other.DnsServers
        IsDHCPEnabled = other.IsDHCPEnabled
        IsConnected = other.IsConnected
    End Sub

    Private Function SetField(Of T)(ByRef field As T, value As T, <CallerMemberName> Optional propertyName As String = Nothing) As Boolean
        If EqualityComparer(Of T).Default.Equals(field, value) Then Return False
        field = value
        OnPropertyChanged(propertyName)
        Return True
    End Function

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class

Public Enum AdapterType
    Ethernet
    WiFi
    Other
End Enum
