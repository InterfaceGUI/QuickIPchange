Imports System.Text.RegularExpressions
Imports System.Windows.Input

Public Class IPAddressControl

    Public Sub New()
        InitializeComponent()
        AddEventHandlers()
    End Sub

    ' 添加事件處理器
    Private Sub AddEventHandlers()
        AddHandler Part1.PreviewTextInput, AddressOf ValidateNumberInput
        AddHandler Part2.PreviewTextInput, AddressOf ValidateNumberInput
        AddHandler Part3.PreviewTextInput, AddressOf ValidateNumberInput
        AddHandler Part4.PreviewTextInput, AddressOf ValidateNumberInput

        AddHandler Part1.TextChanged, AddressOf OnPartTextChanged
        AddHandler Part2.TextChanged, AddressOf OnPartTextChanged
        AddHandler Part3.TextChanged, AddressOf OnPartTextChanged
        AddHandler Part4.TextChanged, AddressOf OnPartTextChanged

        AddHandler Part1.PreviewKeyDown, AddressOf OnPartKeyDown
        AddHandler Part2.PreviewKeyDown, AddressOf OnPartKeyDown
        AddHandler Part3.PreviewKeyDown, AddressOf OnPartKeyDown
        AddHandler Part4.PreviewKeyDown, AddressOf OnPartKeyDown
    End Sub

    ' IP 地址屬性
    Public Property IPAddress As String
        Get
            Return $"{GetPartText(Part1)}.{GetPartText(Part2)}.{GetPartText(Part3)}.{GetPartText(Part4)}"
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then
                Part1.Text = ""
                Part2.Text = ""
                Part3.Text = ""
                Part4.Text = ""
                Return
            End If

            Dim parts = value.Split("."c)
            If parts.Length >= 1 Then Part1.Text = parts(0)
            If parts.Length >= 2 Then Part2.Text = parts(1)
            If parts.Length >= 3 Then Part3.Text = parts(2)
            If parts.Length >= 4 Then Part4.Text = parts(3)
        End Set
    End Property

    Private Function GetPartText(textBox As TextBox) As String
        Return If(String.IsNullOrEmpty(textBox.Text), "0", textBox.Text)
    End Function

    ' 驗證數字輸入
    Private Sub ValidateNumberInput(sender As Object, e As TextCompositionEventArgs)
        ' 只允許數字
        Dim regex As New Regex("[^0-9]+")
        If regex.IsMatch(e.Text) Then
            e.Handled = True
            Return
        End If

        Dim textBox = TryCast(sender, TextBox)
        If textBox IsNot Nothing Then
            ' 如果已經有 3 位數字，阻止輸入並跳到下一格
            If textBox.Text.Length >= 3 Then
                e.Handled = True
                MoveToNextBox(textBox)
                Return
            End If

            ' 檢查輸入後的值是否超過 255
            Dim newText = textBox.Text.Insert(textBox.SelectionStart, e.Text)
            Dim value As Integer
            If Integer.TryParse(newText, value) Then
                If value > 255 Then
                    e.Handled = True
                End If
            End If
        End If
    End Sub

    ' 文本變更事件
    Private Sub OnPartTextChanged(sender As Object, e As TextChangedEventArgs)
        Dim textBox = TryCast(sender, TextBox)
        If textBox Is Nothing Then Return

        ' 驗證並限制範圍 0-255
        If Not String.IsNullOrEmpty(textBox.Text) Then
            Dim value As Integer
            If Integer.TryParse(textBox.Text, value) Then
                If value > 255 Then
                    textBox.Text = "255"
                    textBox.SelectionStart = textBox.Text.Length
                    Return
                End If
            End If
        End If

        ' 不再自動跳格，由 PreviewTextInput 處理
    End Sub

    ' 鍵盤事件處理
    Private Sub OnPartKeyDown(sender As Object, e As KeyEventArgs)
        Dim textBox = TryCast(sender, TextBox)
        If textBox Is Nothing Then Return

        ' 按下點號，跳到下一格
        If e.Key = Key.OemPeriod OrElse e.Key = Key.Decimal Then
            MoveToNextBox(textBox)
            e.Handled = True
            Return
        End If

        ' 按下向右鍵在末尾時，跳到下一格
        If e.Key = Key.Right AndAlso textBox.SelectionStart = textBox.Text.Length Then
            MoveToNextBox(textBox)
            e.Handled = True
            Return
        End If

        ' 按下向左鍵在開頭時，跳到上一格
        If e.Key = Key.Left AndAlso textBox.SelectionStart = 0 Then
            MoveToPreviousBox(textBox)
            e.Handled = True
            Return
        End If

        ' Backspace 在開頭時跳到上一格
        If e.Key = Key.Back AndAlso textBox.SelectionStart = 0 AndAlso textBox.SelectionLength = 0 Then
            MoveToPreviousBox(textBox)
            e.Handled = True
            Return
        End If
    End Sub

    ' 移動到下一個輸入框
    Private Sub MoveToNextBox(currentBox As TextBox)
        Dim nextBox As TextBox = Nothing

        If currentBox Is Part1 Then
            nextBox = Part2
        ElseIf currentBox Is Part2 Then
            nextBox = Part3
        ElseIf currentBox Is Part3 Then
            nextBox = Part4
        End If

        If nextBox IsNot Nothing Then
            nextBox.Focus()
            nextBox.SelectAll()
        End If
    End Sub

    ' 移動到上一個輸入框
    Private Sub MoveToPreviousBox(currentBox As TextBox)
        Dim previousBox As TextBox = Nothing

        If currentBox Is Part2 Then
            previousBox = Part1
        ElseIf currentBox Is Part3 Then
            previousBox = Part2
        ElseIf currentBox Is Part4 Then
            previousBox = Part3
        End If

        If previousBox IsNot Nothing Then
            previousBox.Focus()
            previousBox.SelectionStart = previousBox.Text.Length
        End If
    End Sub

End Class
