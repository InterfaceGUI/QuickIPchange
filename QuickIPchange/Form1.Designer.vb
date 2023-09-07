<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Form1))
        Button1 = New Button()
        Button2 = New Button()
        Button3 = New Button()
        Button4 = New Button()
        Button5 = New Button()
        ListBox1 = New ListBox()
        ComboBox1 = New ComboBox()
        Timer1 = New Timer(components)
        ToolStrip1 = New ToolStrip()
        ToolStripSeparator1 = New ToolStripSeparator()
        ToolStripLabel1 = New ToolStripLabel()
        ToolStripTextBox1 = New ToolStripTextBox()
        ToolStripSeparator2 = New ToolStripSeparator()
        BackgroundWorker1 = New ComponentModel.BackgroundWorker()
        ToolStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Button1.BackColor = Color.FromArgb(CByte(61), CByte(61), CByte(61))
        Button1.FlatAppearance.BorderColor = Color.Black
        Button1.FlatStyle = FlatStyle.Flat
        Button1.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Button1.ForeColor = Color.White
        Button1.Location = New Point(12, 74)
        Button1.Name = "Button1"
        Button1.Size = New Size(435, 47)
        Button1.TabIndex = 0
        Button1.Text = "設定為DCHP"
        Button1.UseVisualStyleBackColor = False
        ' 
        ' Button2
        ' 
        Button2.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Button2.BackColor = Color.FromArgb(CByte(61), CByte(61), CByte(61))
        Button2.FlatAppearance.BorderColor = Color.Black
        Button2.FlatStyle = FlatStyle.Flat
        Button2.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Button2.ForeColor = Color.White
        Button2.Location = New Point(12, 131)
        Button2.Name = "Button2"
        Button2.Size = New Size(435, 47)
        Button2.TabIndex = 0
        Button2.Text = "設定為 192.168.1.66"
        Button2.UseVisualStyleBackColor = False
        ' 
        ' Button3
        ' 
        Button3.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Button3.BackColor = Color.FromArgb(CByte(61), CByte(61), CByte(61))
        Button3.FlatAppearance.BorderColor = Color.Black
        Button3.FlatStyle = FlatStyle.Flat
        Button3.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Button3.ForeColor = Color.White
        Button3.Location = New Point(12, 188)
        Button3.Name = "Button3"
        Button3.Size = New Size(435, 47)
        Button3.TabIndex = 0
        Button3.Text = "設定為 ABB"
        Button3.UseVisualStyleBackColor = False
        ' 
        ' Button4
        ' 
        Button4.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Button4.BackColor = Color.FromArgb(CByte(61), CByte(61), CByte(61))
        Button4.FlatAppearance.BorderColor = Color.Black
        Button4.FlatStyle = FlatStyle.Flat
        Button4.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Button4.ForeColor = Color.White
        Button4.Location = New Point(12, 245)
        Button4.Name = "Button4"
        Button4.Size = New Size(435, 47)
        Button4.TabIndex = 0
        Button4.Text = "設定為 192.168.0.66"
        Button4.UseVisualStyleBackColor = False
        ' 
        ' Button5
        ' 
        Button5.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Button5.BackColor = Color.FromArgb(CByte(61), CByte(61), CByte(61))
        Button5.FlatAppearance.BorderColor = Color.Black
        Button5.FlatStyle = FlatStyle.Flat
        Button5.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        Button5.ForeColor = Color.White
        Button5.Location = New Point(12, 302)
        Button5.Name = "Button5"
        Button5.Size = New Size(435, 47)
        Button5.TabIndex = 0
        Button5.Text = "設定為192.168.100.66"
        Button5.UseVisualStyleBackColor = False
        ' 
        ' ListBox1
        ' 
        ListBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        ListBox1.BackColor = Color.FromArgb(CByte(37), CByte(37), CByte(38))
        ListBox1.BorderStyle = BorderStyle.None
        ListBox1.Font = New Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point)
        ListBox1.ForeColor = Color.White
        ListBox1.FormattingEnabled = True
        ListBox1.ItemHeight = 19
        ListBox1.Location = New Point(12, 359)
        ListBox1.Name = "ListBox1"
        ListBox1.Size = New Size(435, 171)
        ListBox1.TabIndex = 1
        ' 
        ' ComboBox1
        ' 
        ComboBox1.BackColor = Color.FromArgb(CByte(37), CByte(37), CByte(38))
        ComboBox1.DropDownStyle = ComboBoxStyle.DropDownList
        ComboBox1.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        ComboBox1.ForeColor = Color.White
        ComboBox1.FormattingEnabled = True
        ComboBox1.Location = New Point(12, 40)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(435, 28)
        ComboBox1.TabIndex = 2
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 500
        ' 
        ' ToolStrip1
        ' 
        ToolStrip1.Font = New Font("Microsoft JhengHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point)
        ToolStrip1.GripStyle = ToolStripGripStyle.Hidden
        ToolStrip1.ImageScalingSize = New Size(25, 25)
        ToolStrip1.Items.AddRange(New ToolStripItem() {ToolStripSeparator1, ToolStripLabel1, ToolStripTextBox1, ToolStripSeparator2})
        ToolStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow
        ToolStrip1.Location = New Point(0, 0)
        ToolStrip1.Name = "ToolStrip1"
        ToolStrip1.Padding = New Padding(0)
        ToolStrip1.Size = New Size(459, 28)
        ToolStrip1.TabIndex = 3
        ToolStrip1.Text = "ToolStrip1"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(6, 28)
        ' 
        ' ToolStripLabel1
        ' 
        ToolStripLabel1.Name = "ToolStripLabel1"
        ToolStripLabel1.Size = New Size(97, 25)
        ToolStripLabel1.Text = "192.168.x."
        ' 
        ' ToolStripTextBox1
        ' 
        ToolStripTextBox1.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold Or FontStyle.Underline, GraphicsUnit.Point)
        ToolStripTextBox1.Name = "ToolStripTextBox1"
        ToolStripTextBox1.Size = New Size(30, 28)
        ToolStripTextBox1.Text = "66"
        ' 
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        ToolStripSeparator2.Size = New Size(6, 28)
        ' 
        ' BackgroundWorker1
        ' 
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        ClientSize = New Size(459, 545)
        Controls.Add(ToolStrip1)
        Controls.Add(ComboBox1)
        Controls.Add(ListBox1)
        Controls.Add(Button5)
        Controls.Add(Button4)
        Controls.Add(Button3)
        Controls.Add(Button2)
        Controls.Add(Button1)
        ForeColor = Color.Black
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Form1"
        Text = "QuickIPChange"
        ToolStrip1.ResumeLayout(False)
        ToolStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents ToolStrip1 As ToolStrip
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents ToolStripTextBox1 As ToolStripTextBox
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
End Class
