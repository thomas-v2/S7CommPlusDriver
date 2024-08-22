namespace S7CommPlusGUIBrowser {
    partial class Form1 {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            groupBox1 = new System.Windows.Forms.GroupBox();
            lbStatus = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            btnDisconnect = new System.Windows.Forms.Button();
            btnConnect = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            tbPassword = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbIpAddress = new System.Windows.Forms.TextBox();
            treeView1 = new System.Windows.Forms.TreeView();
            imageListTreeIcons = new System.Windows.Forms.ImageList(components);
            groupBox2 = new System.Windows.Forms.GroupBox();
            btnRead = new System.Windows.Forms.Button();
            tbValue = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            tbSymbolicAddress = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            tbSymbol = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbComment = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lbStatus);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(btnDisconnect);
            groupBox1.Controls.Add(btnConnect);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(tbPassword);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(tbIpAddress);
            groupBox1.Location = new System.Drawing.Point(14, 14);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(200, 164);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Connection";
            // 
            // lbStatus
            // 
            lbStatus.Location = new System.Drawing.Point(51, 142);
            lbStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new System.Drawing.Size(135, 18);
            lbStatus.TabIndex = 7;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(7, 142);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(42, 15);
            label6.TabIndex = 6;
            label6.Text = "Status:";
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new System.Drawing.Point(99, 112);
            btnDisconnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new System.Drawing.Size(88, 27);
            btnDisconnect.TabIndex = 5;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new System.Drawing.Point(10, 112);
            btnConnect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(88, 27);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 63);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(115, 15);
            label2.TabIndex = 3;
            label2.Text = "Password (optional):";
            // 
            // tbPassword
            // 
            tbPassword.Location = new System.Drawing.Point(10, 82);
            tbPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPassword.Name = "tbPassword";
            tbPassword.PasswordChar = '*';
            tbPassword.Size = new System.Drawing.Size(176, 23);
            tbPassword.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 18);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(67, 15);
            label1.TabIndex = 1;
            label1.Text = "IP-Address:";
            // 
            // tbIpAddress
            // 
            tbIpAddress.Location = new System.Drawing.Point(10, 37);
            tbIpAddress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbIpAddress.Name = "tbIpAddress";
            tbIpAddress.Size = new System.Drawing.Size(176, 23);
            tbIpAddress.TabIndex = 0;
            // 
            // treeView1
            // 
            treeView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeView1.ImageIndex = 0;
            treeView1.ImageList = imageListTreeIcons;
            treeView1.Location = new System.Drawing.Point(14, 227);
            treeView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeView1.Name = "treeView1";
            treeView1.SelectedImageIndex = 0;
            treeView1.Size = new System.Drawing.Size(905, 278);
            treeView1.TabIndex = 1;
            treeView1.AfterExpand += treeView1_AfterExpand;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // imageListTreeIcons
            // 
            imageListTreeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListTreeIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListTreeIcons.ImageStream");
            imageListTreeIcons.TransparentColor = System.Drawing.Color.Transparent;
            imageListTreeIcons.Images.SetKeyName(0, "Default");
            imageListTreeIcons.Images.SetKeyName(1, "Datablock");
            imageListTreeIcons.Images.SetKeyName(2, "Tag");
            imageListTreeIcons.Images.SetKeyName(3, "Boolean");
            imageListTreeIcons.Images.SetKeyName(4, "Boolean1");
            imageListTreeIcons.Images.SetKeyName(5, "Boolean2");
            imageListTreeIcons.Images.SetKeyName(6, "Any");
            imageListTreeIcons.Images.SetKeyName(7, "Binary");
            imageListTreeIcons.Images.SetKeyName(8, "Binary2");
            imageListTreeIcons.Images.SetKeyName(9, "Date");
            imageListTreeIcons.Images.SetKeyName(10, "Integer");
            imageListTreeIcons.Images.SetKeyName(11, "Integer2");
            imageListTreeIcons.Images.SetKeyName(12, "Number");
            imageListTreeIcons.Images.SetKeyName(13, "Number2");
            imageListTreeIcons.Images.SetKeyName(14, "Time");
            imageListTreeIcons.Images.SetKeyName(15, "Datetime");
            imageListTreeIcons.Images.SetKeyName(16, "Datetimezone");
            imageListTreeIcons.Images.SetKeyName(17, "Duration");
            imageListTreeIcons.Images.SetKeyName(18, "Timer");
            imageListTreeIcons.Images.SetKeyName(19, "Char");
            imageListTreeIcons.Images.SetKeyName(20, "Text");
            imageListTreeIcons.Images.SetKeyName(21, "Text2");
            imageListTreeIcons.Images.SetKeyName(22, "Text3");
            imageListTreeIcons.Images.SetKeyName(23, "Structure");
            imageListTreeIcons.Images.SetKeyName(24, "Currency");
            imageListTreeIcons.Images.SetKeyName(25, "Percentage");
            imageListTreeIcons.Images.SetKeyName(26, "UserDataType");
            imageListTreeIcons.Images.SetKeyName(27, "UserDefinedDataType");
            imageListTreeIcons.Images.SetKeyName(28, "CountCollection");
            imageListTreeIcons.Images.SetKeyName(29, "FolderOpened");
            imageListTreeIcons.Images.SetKeyName(30, "FolderClosed");
            imageListTreeIcons.Images.SetKeyName(31, "FolderTopPanel");
            imageListTreeIcons.Images.SetKeyName(32, "Indexer");
            imageListTreeIcons.Images.SetKeyName(33, "Library");
            imageListTreeIcons.Images.SetKeyName(34, "NamedSet");
            imageListTreeIcons.Images.SetKeyName(35, "TypeDefinition");
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox2.Controls.Add(tbComment);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(btnRead);
            groupBox2.Controls.Add(tbValue);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(tbSymbolicAddress);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(tbSymbol);
            groupBox2.Controls.Add(label3);
            groupBox2.Location = new System.Drawing.Point(220, 14);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(699, 207);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Data";
            // 
            // btnRead
            // 
            btnRead.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnRead.Location = new System.Drawing.Point(604, 35);
            btnRead.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRead.Name = "btnRead";
            btnRead.Size = new System.Drawing.Size(88, 27);
            btnRead.TabIndex = 11;
            btnRead.Text = "Read";
            btnRead.UseVisualStyleBackColor = true;
            btnRead.Click += btnRead_Click;
            // 
            // tbValue
            // 
            tbValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbValue.Location = new System.Drawing.Point(7, 127);
            tbValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbValue.Name = "tbValue";
            tbValue.ReadOnly = true;
            tbValue.Size = new System.Drawing.Size(684, 23);
            tbValue.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(7, 108);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(38, 15);
            label5.TabIndex = 9;
            label5.Text = "Value:";
            // 
            // tbSymbolicAddress
            // 
            tbSymbolicAddress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbSymbolicAddress.Location = new System.Drawing.Point(7, 82);
            tbSymbolicAddress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbSymbolicAddress.Name = "tbSymbolicAddress";
            tbSymbolicAddress.ReadOnly = true;
            tbSymbolicAddress.Size = new System.Drawing.Size(684, 23);
            tbSymbolicAddress.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(7, 63);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(102, 15);
            label4.TabIndex = 7;
            label4.Text = "Symbolic address:";
            // 
            // tbSymbol
            // 
            tbSymbol.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbSymbol.Location = new System.Drawing.Point(7, 37);
            tbSymbol.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbSymbol.Name = "tbSymbol";
            tbSymbol.Size = new System.Drawing.Size(590, 23);
            tbSymbol.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 18);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(50, 15);
            label3.TabIndex = 0;
            label3.Text = "Symbol:";
            // 
            // tbComment
            // 
            tbComment.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbComment.Location = new System.Drawing.Point(8, 172);
            tbComment.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbComment.Name = "tbComment";
            tbComment.ReadOnly = true;
            tbComment.Size = new System.Drawing.Size(684, 23);
            tbComment.TabIndex = 13;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(8, 153);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(64, 15);
            label7.TabIndex = 12;
            label7.Text = "Comment:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(933, 519);
            Controls.Add(groupBox2);
            Controls.Add(treeView1);
            Controls.Add(groupBox1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "S7CommPlusGUIBrowser";
            FormClosed += Form1_FormClosed;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbIpAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbValue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSymbolicAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSymbol;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.ImageList imageListTreeIcons;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label label7;
    }
}

