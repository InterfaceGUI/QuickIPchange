Imports System.Windows.Input

''' <summary>
''' 四段式 IPv4 輸入控制項。全部留空時 IPAddress 回傳空字串；部分留空的段落視為 0。
''' </summary>
Public Class IPAddressControl

    Private ReadOnly _parts As TextBox()

    Public Sub New()
        InitializeComponent()
        _parts = {Part1, Part2, Part3, Part4}

        For Each part In _parts
            AddHandler part.PreviewTextInput, AddressOf Octet_PreviewTextInput
            AddHandler part.PreviewKeyDown, AddressOf Octet_PreviewKeyDown
            AddHandler part.TextChanged, AddressOf Octet_TextChanged
            AddHandler part.GotKeyboardFocus, Sub(s, e) CType(s, TextBox).SelectAll()
            DataObject.AddPastingHandler(part, AddressOf Octet_Paste)
        Next
    End Sub

    ''' <summary>目前輸入的 IP；四段皆為空時回傳空字串</summary>
    Public Property IPAddress As String
        Get
            If _parts.All(Function(p) String.IsNullOrEmpty(p.Text)) Then Return ""
            Return String.Join(".", _parts.Select(Function(p) If(String.IsNullOrEmpty(p.Text), "0", p.Text)))
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                Clear()
                Return
            End If

            Dim segments = value.Trim().Split("."c)
            For i = 0 To _parts.Length - 1
                _parts(i).Text = If(i < segments.Length, segments(i), "")
            Next
        End Set
    End Property

    ''' <summary>是否四段皆未輸入</summary>
    Public ReadOnly Property IsEmpty As Boolean
        Get
            Return _parts.All(Function(p) String.IsNullOrEmpty(p.Text))
        End Get
    End Property

    Public Sub Clear()
        For Each part In _parts
            part.Text = ""
        Next
    End Sub

    Public Shadows Function Focus() As Boolean
        Return Part1.Focus()
    End Function

    ' 只允許數字，且輸入後不得超過 255
    Private Sub Octet_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim textBox = CType(sender, TextBox)

        If Not e.Text.All(AddressOf Char.IsDigit) Then
            e.Handled = True
            Return
        End If

        Dim selectionEnd = textBox.SelectionStart + textBox.SelectionLength
        Dim newText = textBox.Text.Substring(0, textBox.SelectionStart) & e.Text & textBox.Text.Substring(selectionEnd)
        Dim value As Integer
        If Integer.TryParse(newText, value) AndAlso value > 255 Then
            e.Handled = True
            Return
        End If

        ' 填滿三位數後自動跳到下一格
        If newText.Length >= 3 AndAlso textBox.SelectionStart + e.Text.Length >= 3 Then
            textBox.Text = newText
            e.Handled = True
            MoveToNext(textBox)
        End If
    End Sub

    ' 貼上完整 IP（例如 192.168.1.10）時自動分配到四格
    Private Sub Octet_Paste(sender As Object, e As DataObjectPastingEventArgs)
        If Not e.DataObject.GetDataPresent(DataFormats.Text) Then Return
        Dim text = CStr(e.DataObject.GetData(DataFormats.Text)).Trim()
        If text.Contains("."c) Then
            e.CancelCommand()
            IPAddress = text
            Part4.Focus()
            Part4.CaretIndex = Part4.Text.Length
        End If
    End Sub

    Private Sub Octet_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim textBox = CType(sender, TextBox)
        Dim value As Integer
        ' 透過程式設定或輸入法繞過 PreviewTextInput 時的保險
        If Integer.TryParse(textBox.Text, value) AndAlso value > 255 Then
            textBox.Text = "255"
            textBox.CaretIndex = textBox.Text.Length
        End If
    End Sub

    Private Sub Octet_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        Dim textBox = CType(sender, TextBox)

        Select Case e.Key
            Case Key.OemPeriod, Key.Decimal, Key.Space
                If textBox.Text.Length > 0 Then MoveToNext(textBox)
                e.Handled = True

            Case Key.Right
                If textBox.CaretIndex = textBox.Text.Length AndAlso textBox.SelectionLength = 0 Then
                    MoveToNext(textBox)
                    e.Handled = True
                End If

            Case Key.Left
                If textBox.CaretIndex = 0 AndAlso textBox.SelectionLength = 0 Then
                    MoveToPrevious(textBox)
                    e.Handled = True
                End If

            Case Key.Back
                If textBox.Text.Length = 0 Then
                    MoveToPrevious(textBox)
                    e.Handled = True
                End If
        End Select
    End Sub

    Private Sub MoveToNext(current As TextBox)
        Dim index = Array.IndexOf(_parts, current)
        If index >= 0 AndAlso index < _parts.Length - 1 Then
            _parts(index + 1).Focus()
            _parts(index + 1).SelectAll()
        End If
    End Sub

    Private Sub MoveToPrevious(current As TextBox)
        Dim index = Array.IndexOf(_parts, current)
        If index > 0 Then
            Dim prev = _parts(index - 1)
            prev.Focus()
            prev.CaretIndex = prev.Text.Length
        End If
    End Sub

End Class
