namespace CsbBuilder
{
    partial class SetAudioForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.swapButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.introBrowseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.loopBrowseButton = new System.Windows.Forms.Button();
            this.loopTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.introTextBox = new System.Windows.Forms.MaskedTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.swapButton);
            this.groupBox1.Controls.Add(this.okButton);
            this.groupBox1.Controls.Add(this.introBrowseButton);
            this.groupBox1.Controls.Add(this.cancelButton);
            this.groupBox1.Controls.Add(this.loopBrowseButton);
            this.groupBox1.Controls.Add(this.loopTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.introTextBox);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 102);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Audio";
            // 
            // swapButton
            // 
            this.swapButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.swapButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.swapButton.Location = new System.Drawing.Point(499, 17);
            this.swapButton.Name = "swapButton";
            this.swapButton.Size = new System.Drawing.Size(24, 50);
            this.swapButton.TabIndex = 6;
            this.swapButton.Text = "↕";
            this.swapButton.UseVisualStyleBackColor = true;
            this.swapButton.Click += new System.EventHandler(this.Swap);
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(367, 73);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // introBrowseButton
            // 
            this.introBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.introBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.introBrowseButton.Location = new System.Drawing.Point(418, 17);
            this.introBrowseButton.Name = "introBrowseButton";
            this.introBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.introBrowseButton.TabIndex = 5;
            this.introBrowseButton.Text = "Browse";
            this.introBrowseButton.UseVisualStyleBackColor = true;
            this.introBrowseButton.Click += new System.EventHandler(this.BrowseIntro);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(448, 73);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // loopBrowseButton
            // 
            this.loopBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.loopBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.loopBrowseButton.Location = new System.Drawing.Point(418, 44);
            this.loopBrowseButton.Name = "loopBrowseButton";
            this.loopBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.loopBrowseButton.TabIndex = 4;
            this.loopBrowseButton.Text = "Browse";
            this.loopBrowseButton.UseVisualStyleBackColor = true;
            this.loopBrowseButton.Click += new System.EventHandler(this.BrowseLoop);
            // 
            // loopTextBox
            // 
            this.loopTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.loopTextBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.loopTextBox.Location = new System.Drawing.Point(43, 45);
            this.loopTextBox.Name = "loopTextBox";
            this.loopTextBox.Size = new System.Drawing.Size(369, 20);
            this.loopTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Loop:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Intro:";
            // 
            // introTextBox
            // 
            this.introTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.introTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.introTextBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.introTextBox.Location = new System.Drawing.Point(43, 19);
            this.introTextBox.Name = "introTextBox";
            this.introTextBox.Size = new System.Drawing.Size(369, 20);
            this.introTextBox.TabIndex = 0;
            // 
            // SetAudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 124);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetAudioForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Audio";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClose);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button loopBrowseButton;
        private System.Windows.Forms.MaskedTextBox loopTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox introTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button swapButton;
        private System.Windows.Forms.Button introBrowseButton;
    }
}