<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UpdataMsgBox
    Inherits System.Windows.Forms.Form

    'Form 覆寫 Dispose 以清除元件清單。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    '為 Windows Form 設計工具的必要項
    Private components As System.ComponentModel.IContainer

    '注意: 以下為 Windows Form 設計工具所需的程序
    '可以使用 Windows Form 設計工具進行修改。
    '請勿使用程式碼編輯器進行修改。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        ProgressBar1 = New ProgressBar()
        TextBox1 = New TextBox()
        SuspendLayout()
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        ProgressBar1.Location = New Point(12, 293)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.Size = New Size(294, 23)
        ProgressBar1.TabIndex = 0
        ' 
        ' TextBox1
        ' 
        TextBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        TextBox1.BackColor = Color.FromArgb(CByte(33), CByte(33), CByte(33))
        TextBox1.ForeColor = Color.White
        TextBox1.Location = New Point(12, 12)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(294, 271)
        TextBox1.TabIndex = 1
        ' 
        ' UpdataMsgBox
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        ClientSize = New Size(318, 324)
        Controls.Add(TextBox1)
        Controls.Add(ProgressBar1)
        ForeColor = Color.White
        Name = "UpdataMsgBox"
        Text = "更新程式"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents TextBox1 As TextBox
End Class
