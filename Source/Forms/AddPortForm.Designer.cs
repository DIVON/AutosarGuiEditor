namespace AutosarGuiEditor.Source.Forms
{
    partial class AddPortForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddPortForm));
            this.AddButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.clientPort = new System.Windows.Forms.RadioButton();
            this.TransmitterPort = new System.Windows.Forms.RadioButton();
            this.serverPort = new System.Windows.Forms.RadioButton();
            this.receiverPort = new System.Windows.Forms.RadioButton();
            this.portNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(272, 168);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 0;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(12, 168);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::AutosarGuiEditor.Properties.Resources.ClientPort;
            this.pictureBox1.Location = new System.Drawing.Point(209, 60);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(43, 43);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::AutosarGuiEditor.Properties.Resources.ServerPort;
            this.pictureBox2.Location = new System.Drawing.Point(21, 60);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(43, 43);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::AutosarGuiEditor.Properties.Resources.TransmitterPort;
            this.pictureBox3.Location = new System.Drawing.Point(209, 109);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(43, 43);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox3.TabIndex = 4;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::AutosarGuiEditor.Properties.Resources.ReceiverPort;
            this.pictureBox4.Location = new System.Drawing.Point(21, 109);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(43, 43);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox4.TabIndex = 5;
            this.pictureBox4.TabStop = false;
            // 
            // clientPort
            // 
            this.clientPort.AutoSize = true;
            this.clientPort.Checked = true;
            this.clientPort.Location = new System.Drawing.Point(260, 74);
            this.clientPort.Name = "clientPort";
            this.clientPort.Size = new System.Drawing.Size(72, 17);
            this.clientPort.TabIndex = 6;
            this.clientPort.TabStop = true;
            this.clientPort.Text = "Client port";
            this.clientPort.UseVisualStyleBackColor = true;
            // 
            // TransmitterPort
            // 
            this.TransmitterPort.AutoSize = true;
            this.TransmitterPort.Location = new System.Drawing.Point(260, 124);
            this.TransmitterPort.Name = "TransmitterPort";
            this.TransmitterPort.Size = new System.Drawing.Size(80, 17);
            this.TransmitterPort.TabIndex = 7;
            this.TransmitterPort.Text = "Sender port";
            this.TransmitterPort.UseVisualStyleBackColor = true;
            // 
            // serverPort
            // 
            this.serverPort.AutoSize = true;
            this.serverPort.Location = new System.Drawing.Point(72, 74);
            this.serverPort.Name = "serverPort";
            this.serverPort.Size = new System.Drawing.Size(77, 17);
            this.serverPort.TabIndex = 8;
            this.serverPort.Text = "Server port";
            this.serverPort.UseVisualStyleBackColor = true;
            // 
            // receiverPort
            // 
            this.receiverPort.AutoSize = true;
            this.receiverPort.Location = new System.Drawing.Point(72, 124);
            this.receiverPort.Name = "receiverPort";
            this.receiverPort.Size = new System.Drawing.Size(89, 17);
            this.receiverPort.TabIndex = 9;
            this.receiverPort.Text = "Receiver port";
            this.receiverPort.UseVisualStyleBackColor = true;
            // 
            // portNameTextBox
            // 
            this.portNameTextBox.Location = new System.Drawing.Point(79, 26);
            this.portNameTextBox.Name = "portNameTextBox";
            this.portNameTextBox.Size = new System.Drawing.Size(268, 20);
            this.portNameTextBox.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Port name";
            // 
            // AddPortForm
            // 
            this.AcceptButton = this.AddButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(361, 209);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.portNameTextBox);
            this.Controls.Add(this.receiverPort);
            this.Controls.Add(this.serverPort);
            this.Controls.Add(this.TransmitterPort);
            this.Controls.Add(this.clientPort);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.AddButton);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddPortForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add port to component";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.RadioButton clientPort;
        private System.Windows.Forms.RadioButton TransmitterPort;
        private System.Windows.Forms.RadioButton serverPort;
        private System.Windows.Forms.RadioButton receiverPort;
        private System.Windows.Forms.TextBox portNameTextBox;
        private System.Windows.Forms.Label label1;
    }
}