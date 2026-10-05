namespace Stm32OneClickUploader.UI
{
    partial class MainForm
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
            txtFilePath = new TextBox();
            btnBrowse = new Button();
            btnUpload = new Button();
            rtbLogs = new RichTextBox();
            lblStatus = new Label();
            btnErase = new Button();
            SuspendLayout();
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new Point(112, 34);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(249, 27);
            txtFilePath.TabIndex = 0;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(384, 32);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(94, 29);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "Gözat";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnUpload
            // 
            btnUpload.BackColor = Color.Lime;
            btnUpload.Location = new Point(484, 70);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(94, 29);
            btnUpload.TabIndex = 2;
            btnUpload.Text = "Yükle";
            btnUpload.UseVisualStyleBackColor = false;
            btnUpload.Click += btnUpload_Click;
            // 
            // rtbLogs
            // 
            rtbLogs.Location = new Point(112, 92);
            rtbLogs.Name = "rtbLogs";
            rtbLogs.ReadOnly = true;
            rtbLogs.Size = new Size(366, 279);
            rtbLogs.TabIndex = 3;
            rtbLogs.Text = "";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(484, 131);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(96, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Durum: Hazır";
            // 
            // btnErase
            // 
            btnErase.BackColor = Color.Red;
            btnErase.Location = new Point(484, 31);
            btnErase.Name = "btnErase";
            btnErase.Size = new Size(94, 29);
            btnErase.TabIndex = 5;
            btnErase.Text = "Chip Erase";
            btnErase.UseVisualStyleBackColor = false;
            btnErase.Click += btnErase_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnErase);
            Controls.Add(lblStatus);
            Controls.Add(rtbLogs);
            Controls.Add(btnUpload);
            Controls.Add(btnBrowse);
            Controls.Add(txtFilePath);
            Name = "MainForm";
            Text = "MainForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnUpload;
        private RichTextBox rtbLogs;
        private Label lblStatus;
        private Button btnErase;
    }
}