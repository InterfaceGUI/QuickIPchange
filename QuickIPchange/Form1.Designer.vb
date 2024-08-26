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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        StatusStrip1 = New StatusStrip()
        ToolStripStatusLabel1 = New ToolStripStatusLabel()
        ToolStripTextBox2 = New ToolStripTextBox()
        ToolStripStatusLabel2 = New ToolStripStatusLabel()
        ToolStripStatusLabel3 = New ToolStripStatusLabel()
        ToolStripStatusLabel4 = New ToolStripStatusLabel()
        ToolStripStatusLabel5 = New ToolStripStatusLabel()
        ToolStripStatusLabel6 = New ToolStripStatusLabel()
        ToolStripStatusLabel7 = New ToolStripStatusLabel()
        Timer_update_chk = New Timer(components)
        FlowLayoutPanel1 = New FlowLayoutPanel()
        btn_template = New Button()
        GroupBox1 = New GroupBox()
        Timer1 = New Timer(components)
        Label1 = New Label()
        GroupBox2 = New GroupBox()
        Label4 = New Label()
        Label3 = New Label()
        Label2 = New Label()
        GroupBox3 = New GroupBox()
        FlowLayoutPanel2 = New FlowLayoutPanel()
        btn_dhcp = New Button()
        Button2 = New Button()
        Button3 = New Button()
        Button4 = New Button()
        Button5 = New Button()
        GroupBox4 = New GroupBox()
        Button1 = New Button()
        Panel1 = New Panel()
        IpTextBox3 = New lars_IPtxtBox.IPTextBox()
        IpTextBox1 = New lars_IPtxtBox.IPTextBox()
        IpTextBox2 = New lars_IPtxtBox.IPTextBox()
        Label6 = New Label()
        Label7 = New Label()
        Label5 = New Label()
        Timer2 = New Timer(components)
        StatusStrip1.SuspendLayout()
        FlowLayoutPanel1.SuspendLayout()
        GroupBox1.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox3.SuspendLayout()
        FlowLayoutPanel2.SuspendLayout()
        GroupBox4.SuspendLayout()
        Panel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.BackColor = SystemColors.Control
        StatusStrip1.Items.AddRange(New ToolStripItem() {ToolStripStatusLabel1, ToolStripTextBox2, ToolStripStatusLabel2, ToolStripStatusLabel3, ToolStripStatusLabel4, ToolStripStatusLabel5, ToolStripStatusLabel6, ToolStripStatusLabel7})
        StatusStrip1.Location = New Point(0, 493)
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.Size = New Size(742, 23)
        StatusStrip1.TabIndex = 4
        StatusStrip1.Text = "StatusStrip1"
        ' 
        ' ToolStripStatusLabel1
        ' 
        ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        ToolStripStatusLabel1.Size = New Size(70, 18)
        ToolStripStatusLabel1.Text = "xxx.xxx.xxx."
        ' 
        ' ToolStripTextBox2
        ' 
        ToolStripTextBox2.Name = "ToolStripTextBox2"
        ToolStripTextBox2.Size = New Size(35, 23)
        ' 
        ' ToolStripStatusLabel2
        ' 
        ToolStripStatusLabel2.BackColor = SystemColors.Control
        ToolStripStatusLabel2.Name = "ToolStripStatusLabel2"
        ToolStripStatusLabel2.Size = New Size(10, 18)
        ToolStripStatusLabel2.Text = "|"
        ' 
        ' ToolStripStatusLabel3
        ' 
        ToolStripStatusLabel3.Name = "ToolStripStatusLabel3"
        ToolStripStatusLabel3.Size = New Size(564, 18)
        ToolStripStatusLabel3.Spring = True
        ' 
        ' ToolStripStatusLabel4
        ' 
        ToolStripStatusLabel4.Name = "ToolStripStatusLabel4"
        ToolStripStatusLabel4.Size = New Size(34, 18)
        ToolStripStatusLabel4.Text = "版本:"
        ' 
        ' ToolStripStatusLabel5
        ' 
        ToolStripStatusLabel5.Name = "ToolStripStatusLabel5"
        ToolStripStatusLabel5.Size = New Size(12, 18)
        ToolStripStatusLabel5.Text = "-"
        ' 
        ' ToolStripStatusLabel6
        ' 
        ToolStripStatusLabel6.Name = "ToolStripStatusLabel6"
        ToolStripStatusLabel6.Size = New Size(0, 18)
        ' 
        ' ToolStripStatusLabel7
        ' 
        ToolStripStatusLabel7.Image = CType(resources.GetObject("ToolStripStatusLabel7.Image"), Image)
        ToolStripStatusLabel7.Name = "ToolStripStatusLabel7"
        ToolStripStatusLabel7.Size = New Size(16, 18)
        ToolStripStatusLabel7.Visible = False
        ' 
        ' Timer_update_chk
        ' 
        Timer_update_chk.Interval = 3000
        ' 
        ' FlowLayoutPanel1
        ' 
        FlowLayoutPanel1.AutoScroll = True
        FlowLayoutPanel1.Controls.Add(btn_template)
        FlowLayoutPanel1.Dock = DockStyle.Fill
        FlowLayoutPanel1.Location = New Point(3, 19)
        FlowLayoutPanel1.Name = "FlowLayoutPanel1"
        FlowLayoutPanel1.Padding = New Padding(5)
        FlowLayoutPanel1.Size = New Size(306, 452)
        FlowLayoutPanel1.TabIndex = 5
        ' 
        ' btn_template
        ' 
        btn_template.BackColor = SystemColors.Control
        btn_template.ForeColor = Color.Black
        btn_template.Image = My.Resources.Resources.wired_network_connection
        btn_template.ImageAlign = ContentAlignment.MiddleLeft
        btn_template.Location = New Point(8, 8)
        btn_template.Name = "btn_template"
        btn_template.Padding = New Padding(5, 0, 0, 0)
        btn_template.Size = New Size(273, 88)
        btn_template.TabIndex = 9
        btn_template.Text = "Wi-Fi" & vbCrLf & "Intel(R) Wi-Fi 6 AX200 160MHz" & vbCrLf & "DC41A91ED0DE" & vbCrLf & "192.168.1.173" & vbCrLf
        btn_template.TextAlign = ContentAlignment.MiddleLeft
        btn_template.TextImageRelation = TextImageRelation.ImageBeforeText
        btn_template.UseVisualStyleBackColor = False
        btn_template.Visible = False
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        GroupBox1.Controls.Add(FlowLayoutPanel1)
        GroupBox1.ForeColor = Color.White
        GroupBox1.Location = New Point(9, 10)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(312, 474)
        GroupBox1.TabIndex = 6
        GroupBox1.TabStop = False
        GroupBox1.Text = "網路介面"
        ' 
        ' Timer1
        ' 
        Timer1.Interval = 1000
        ' 
        ' Label1
        ' 
        Label1.Font = New Font("Microsoft JhengHei UI", 15.75F, FontStyle.Bold)
        Label1.ForeColor = Color.White
        Label1.Location = New Point(6, 18)
        Label1.Name = "Label1"
        Label1.Size = New Size(391, 23)
        Label1.TabIndex = 7
        Label1.Text = "Wi-Fi"
        Label1.TextAlign = ContentAlignment.TopCenter
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(Label4)
        GroupBox2.Controls.Add(Label3)
        GroupBox2.Controls.Add(Label2)
        GroupBox2.Controls.Add(Label1)
        GroupBox2.ForeColor = Color.White
        GroupBox2.Location = New Point(327, 10)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(403, 115)
        GroupBox2.TabIndex = 8
        GroupBox2.TabStop = False
        GroupBox2.Text = "網卡詳情"
        ' 
        ' Label4
        ' 
        Label4.Font = New Font("Microsoft JhengHei UI", 12F)
        Label4.ForeColor = Color.White
        Label4.Location = New Point(6, 87)
        Label4.Name = "Label4"
        Label4.Size = New Size(391, 20)
        Label4.TabIndex = 8
        Label4.Text = "192.168.1.173"
        Label4.TextAlign = ContentAlignment.TopCenter
        ' 
        ' Label3
        ' 
        Label3.Font = New Font("Microsoft JhengHei UI", 12F)
        Label3.ForeColor = Color.White
        Label3.Location = New Point(6, 66)
        Label3.Name = "Label3"
        Label3.Size = New Size(391, 20)
        Label3.TabIndex = 8
        Label3.Text = "DC41A91ED0DE"
        Label3.TextAlign = ContentAlignment.TopCenter
        ' 
        ' Label2
        ' 
        Label2.Font = New Font("Microsoft JhengHei UI", 12F)
        Label2.ForeColor = Color.White
        Label2.Location = New Point(6, 45)
        Label2.Name = "Label2"
        Label2.Size = New Size(391, 20)
        Label2.TabIndex = 7
        Label2.Text = "Intel(R) Wi-Fi 6 AX200 160MHz"
        Label2.TextAlign = ContentAlignment.TopCenter
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left
        GroupBox3.Controls.Add(FlowLayoutPanel2)
        GroupBox3.ForeColor = Color.White
        GroupBox3.Location = New Point(327, 131)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Padding = New Padding(5, 5, 5, 10)
        GroupBox3.Size = New Size(403, 221)
        GroupBox3.TabIndex = 9
        GroupBox3.TabStop = False
        GroupBox3.Text = "快速設定"
        ' 
        ' FlowLayoutPanel2
        ' 
        FlowLayoutPanel2.AutoScroll = True
        FlowLayoutPanel2.Controls.Add(btn_dhcp)
        FlowLayoutPanel2.Controls.Add(Button2)
        FlowLayoutPanel2.Controls.Add(Button3)
        FlowLayoutPanel2.Controls.Add(Button4)
        FlowLayoutPanel2.Controls.Add(Button5)
        FlowLayoutPanel2.Dock = DockStyle.Fill
        FlowLayoutPanel2.Location = New Point(5, 21)
        FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        FlowLayoutPanel2.Size = New Size(393, 190)
        FlowLayoutPanel2.TabIndex = 0
        ' 
        ' btn_dhcp
        ' 
        btn_dhcp.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        btn_dhcp.ForeColor = Color.Black
        btn_dhcp.Location = New Point(3, 3)
        btn_dhcp.Name = "btn_dhcp"
        btn_dhcp.Padding = New Padding(15, 0, 15, 0)
        btn_dhcp.Size = New Size(365, 31)
        btn_dhcp.TabIndex = 0
        btn_dhcp.Text = "DHCP 模式"
        btn_dhcp.TextAlign = ContentAlignment.MiddleLeft
        btn_dhcp.UseVisualStyleBackColor = True
        ' 
        ' Button2
        ' 
        Button2.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        Button2.ForeColor = Color.Black
        Button2.Location = New Point(3, 40)
        Button2.Name = "Button2"
        Button2.Padding = New Padding(15, 0, 15, 0)
        Button2.Size = New Size(365, 31)
        Button2.TabIndex = 0
        Button2.Tag = "125"
        Button2.Text = "ABB (192.168.125.250)"
        Button2.TextAlign = ContentAlignment.MiddleLeft
        Button2.UseVisualStyleBackColor = True
        ' 
        ' Button3
        ' 
        Button3.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        Button3.ForeColor = Color.Black
        Button3.Location = New Point(3, 77)
        Button3.Name = "Button3"
        Button3.Padding = New Padding(15, 0, 15, 0)
        Button3.Size = New Size(365, 31)
        Button3.TabIndex = 0
        Button3.Tag = "1"
        Button3.Text = "WSI CNC(192.168.1.250)"
        Button3.TextAlign = ContentAlignment.MiddleLeft
        Button3.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        Button4.ForeColor = Color.Black
        Button4.Location = New Point(3, 114)
        Button4.Name = "Button4"
        Button4.Padding = New Padding(15, 0, 15, 0)
        Button4.Size = New Size(365, 31)
        Button4.TabIndex = 1
        Button4.Tag = "100"
        Button4.Text = "WSI AutoLine (192.168.100.250)"
        Button4.TextAlign = ContentAlignment.MiddleLeft
        Button4.UseVisualStyleBackColor = True
        ' 
        ' Button5
        ' 
        Button5.Font = New Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        Button5.ForeColor = Color.Black
        Button5.Location = New Point(3, 151)
        Button5.Name = "Button5"
        Button5.Padding = New Padding(15, 0, 15, 0)
        Button5.Size = New Size(365, 31)
        Button5.TabIndex = 2
        Button5.Tag = "0"
        Button5.Text = "192.168.0.250"
        Button5.TextAlign = ContentAlignment.MiddleLeft
        Button5.UseVisualStyleBackColor = True
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        GroupBox4.Controls.Add(Button1)
        GroupBox4.Controls.Add(Panel1)
        GroupBox4.ForeColor = Color.White
        GroupBox4.Location = New Point(327, 357)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(403, 127)
        GroupBox4.TabIndex = 10
        GroupBox4.TabStop = False
        GroupBox4.Text = "手動設定"
        ' 
        ' Button1
        ' 
        Button1.Font = New Font("Microsoft JhengHei UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, CByte(136))
        Button1.ForeColor = Color.Black
        Button1.Image = My.Resources.Resources.icons8_save_48
        Button1.ImageAlign = ContentAlignment.MiddleLeft
        Button1.Location = New Point(232, 24)
        Button1.Name = "Button1"
        Button1.Padding = New Padding(6, 0, 0, 0)
        Button1.Size = New Size(157, 87)
        Button1.TabIndex = 11
        Button1.Text = "   套 用"
        Button1.TextImageRelation = TextImageRelation.ImageBeforeText
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Panel1
        ' 
        Panel1.Controls.Add(IpTextBox3)
        Panel1.Controls.Add(IpTextBox1)
        Panel1.Controls.Add(IpTextBox2)
        Panel1.Controls.Add(Label6)
        Panel1.Controls.Add(Label7)
        Panel1.Controls.Add(Label5)
        Panel1.Location = New Point(6, 22)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(220, 92)
        Panel1.TabIndex = 10
        ' 
        ' IpTextBox3
        ' 
        IpTextBox3.AutoSize = True
        IpTextBox3.BackColor = Color.Transparent
        IpTextBox3.Location = New Point(62, 66)
        IpTextBox3.Margin = New Padding(4, 2, 4, 2)
        IpTextBox3.Name = "IpTextBox3"
        IpTextBox3.Size = New Size(142, 18)
        IpTextBox3.TabIndex = 3
        IpTextBox3.Tag = "gateway"
        IpTextBox3.ToolTipText = ""
        IpTextBox3.txtbox_BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        IpTextBox3.txtbox_ForeColor = SystemColors.Window
        ' 
        ' IpTextBox1
        ' 
        IpTextBox1.AutoSize = True
        IpTextBox1.BackColor = Color.Transparent
        IpTextBox1.Location = New Point(62, 15)
        IpTextBox1.Margin = New Padding(4, 2, 4, 2)
        IpTextBox1.Name = "IpTextBox1"
        IpTextBox1.Size = New Size(142, 18)
        IpTextBox1.TabIndex = 1
        IpTextBox1.Tag = "ip"
        IpTextBox1.ToolTipText = ""
        IpTextBox1.txtbox_BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        IpTextBox1.txtbox_ForeColor = SystemColors.Window
        ' 
        ' IpTextBox2
        ' 
        IpTextBox2.AutoSize = True
        IpTextBox2.BackColor = Color.Transparent
        IpTextBox2.Location = New Point(62, 42)
        IpTextBox2.Margin = New Padding(4, 2, 4, 2)
        IpTextBox2.Name = "IpTextBox2"
        IpTextBox2.Size = New Size(142, 16)
        IpTextBox2.TabIndex = 2
        IpTextBox2.Tag = "mask"
        IpTextBox2.ToolTipText = ""
        IpTextBox2.txtbox_BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        IpTextBox2.txtbox_ForeColor = SystemColors.Window
        ' 
        ' Label6
        ' 
        Label6.BorderStyle = BorderStyle.FixedSingle
        Label6.Location = New Point(5, 36)
        Label6.Name = "Label6"
        Label6.Size = New Size(203, 24)
        Label6.TabIndex = 3
        Label6.Text = "Mask    "
        Label6.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label7
        ' 
        Label7.BorderStyle = BorderStyle.FixedSingle
        Label7.Location = New Point(5, 61)
        Label7.Name = "Label7"
        Label7.Size = New Size(203, 24)
        Label7.TabIndex = 3
        Label7.Text = "Gateway"
        Label7.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label5
        ' 
        Label5.BorderStyle = BorderStyle.FixedSingle
        Label5.Location = New Point(5, 10)
        Label5.Name = "Label5"
        Label5.Size = New Size(203, 24)
        Label5.TabIndex = 1
        Label5.Text = "IPv4       "
        Label5.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Timer2
        ' 
        Timer2.Interval = 1000
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(31), CByte(31), CByte(31))
        ClientSize = New Size(742, 516)
        Controls.Add(GroupBox4)
        Controls.Add(GroupBox3)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Controls.Add(StatusStrip1)
        Font = New Font("Microsoft JhengHei UI", 9F)
        ForeColor = Color.Black
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximumSize = New Size(758, 9999)
        Name = "Form1"
        StartPosition = FormStartPosition.CenterParent
        Text = "QuickIPChange"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        FlowLayoutPanel1.ResumeLayout(False)
        GroupBox1.ResumeLayout(False)
        GroupBox2.ResumeLayout(False)
        GroupBox3.ResumeLayout(False)
        FlowLayoutPanel2.ResumeLayout(False)
        GroupBox4.ResumeLayout(False)
        Panel1.ResumeLayout(False)
        Panel1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents ToolStripStatusLabel2 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel3 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel4 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel5 As ToolStripStatusLabel
    Friend WithEvents ToolStripStatusLabel6 As ToolStripStatusLabel
    Friend WithEvents Timer_update_chk As Timer
    Friend WithEvents ToolStripStatusLabel7 As ToolStripStatusLabel
    Friend WithEvents FlowLayoutPanel1 As FlowLayoutPanel
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Timer1 As Timer
    Friend WithEvents btn_template As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents FlowLayoutPanel2 As FlowLayoutPanel
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents IpTextBox1 As lars_IPtxtBox.IPTextBox
    Friend WithEvents IpTextBox3 As lars_IPtxtBox.IPTextBox
    Friend WithEvents IpTextBox2 As lars_IPtxtBox.IPTextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Button1 As Button
    Friend WithEvents btn_dhcp As Button
    Friend WithEvents Timer2 As Timer
    Friend WithEvents Button2 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents Button5 As Button
    Friend WithEvents ToolStripStatusLabel1 As ToolStripStatusLabel
    Friend WithEvents ToolStripTextBox2 As ToolStripTextBox
End Class
