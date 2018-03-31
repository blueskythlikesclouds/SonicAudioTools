namespace HedgeEdit.UI
{
    partial class TxtBxDialog
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
            this.label = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label.Location = new System.Drawing.Point(0, 0);
            this.label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label.Name = "label";
            this.label.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.label.Size = new System.Drawing.Size(252, 94);
            this.label.TabIndex = 3;
            this.label.Text = "Select the ID of waveform:";
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(8, 41);
            this.textBox.Margin = new System.Windows.Forms.Padding(2);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(237, 20);
            this.textBox.TabIndex = 0;
            this.textBox.TextChanged += new System.EventHandler(this.ValueChanged);
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okBtn.Location = new System.Drawing.Point(187, 65);
            this.okBtn.Margin = new System.Windows.Forms.Padding(2);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(57, 21);
            this.okBtn.TabIndex = 1;
            this.okBtn.Text = "&OK";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.OkBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelBtn.Location = new System.Drawing.Point(127, 65);
            this.cancelBtn.Margin = new System.Windows.Forms.Padding(2);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(57, 21);
            this.cancelBtn.TabIndex = 2;
            this.cancelBtn.Text = "&Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            // 
            // comboBox
            // 
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBox.Location = new System.Drawing.Point(8, 38);
            this.comboBox.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(237, 21);
            this.comboBox.TabIndex = 4;
            this.comboBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // WaveformIdDialog
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(252, 94);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaveformIdDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ACB Injector";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.OnHelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Label label;
        protected System.Windows.Forms.TextBox textBox;
        protected System.Windows.Forms.Button okBtn;
        protected System.Windows.Forms.Button cancelBtn;
        protected System.Windows.Forms.ComboBox comboBox;
    }
}