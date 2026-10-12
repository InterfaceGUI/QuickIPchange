Imports System.ComponentModel

Public Class NetworkAdapterInfo
    Implements INotifyPropertyChanged

    Private _name As String
    Private _description As String
    Private _ipAddress As String
    Private _subnetMask As String
    Private _gateway As String
    Private _macAddress As String
    Private _adapterType As AdapterType
    Private _isPhysical As Boolean
    Private _isDHCPEnabled As Boolean

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
            OnPropertyChanged("Name")
        End Set
    End Property

    Public Property Description As String
        Get
            Return _description
        End Get
        Set(value As String)
            _description = value
            OnPropertyChanged("Description")
        End Set
    End Property

    Public Property IPAddress As String
        Get
            Return _ipAddress
        End Get
        Set(value As String)
            _ipAddress = value
            OnPropertyChanged("IPAddress")
        End Set
    End Property

    Public Property SubnetMask As String
        Get
            Return _subnetMask
        End Get
        Set(value As String)
            _subnetMask = value
            OnPropertyChanged("SubnetMask")
        End Set
    End Property

    Public Property Gateway As String
        Get
            Return _gateway
        End Get
        Set(value As String)
            _gateway = value
            OnPropertyChanged("Gateway")
        End Set
    End Property

    Public Property MacAddress As String
        Get
            Return _macAddress
        End Get
        Set(value As String)
            _macAddress = value
            OnPropertyChanged("MacAddress")
        End Set
    End Property

    Public Property AdapterType As AdapterType
        Get
            Return _adapterType
        End Get
        Set(value As AdapterType)
            _adapterType = value
            OnPropertyChanged("AdapterType")
            OnPropertyChanged("AdapterIcon")
        End Set
    End Property

    Public Property IsPhysical As Boolean
        Get
            Return _isPhysical
        End Get
        Set(value As Boolean)
            _isPhysical = value
            OnPropertyChanged("IsPhysical")
        End Set
    End Property

    Public Property IsDHCPEnabled As Boolean
        Get
            Return _isDHCPEnabled
        End Get
        Set(value As Boolean)
            _isDHCPEnabled = value
            OnPropertyChanged("IsDHCPEnabled")
        End Set
    End Property

    ' 用於 UI 顯示的網卡圖標（使用 Segoe MDL2 Assets 字型）
    Public ReadOnly Property AdapterIcon As String
        Get
            Select Case AdapterType
                Case AdapterType.Ethernet
                    Return ChrW(&HE839) ' Ethernet 圖標
                Case AdapterType.WiFi
                    Return ChrW(&HE701) ' WiFi 圖標
                Case Else
                    Return ChrW(&HE968) ' 通用網路圖標
            End Select
        End Get
    End Property

    ' 用於 UI 顯示的 IP 資訊
    Public ReadOnly Property DisplayIPInfo As String
        Get
            If IsDHCPEnabled AndAlso String.IsNullOrEmpty(IPAddress) Then
                Return "DHCP"
            ElseIf Not String.IsNullOrEmpty(IPAddress) Then
                Return IPAddress
            Else
                Return "未連接"
            End If
        End Get
    End Property

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class

Public Enum AdapterType
    Ethernet
    WiFi
    Other
End Enum
