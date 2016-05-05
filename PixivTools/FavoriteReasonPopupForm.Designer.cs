namespace PixivTools {
    partial class FavoriteReasonPopupForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.comfirmButton = new System.Windows.Forms.Button();
            this.favReasonTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comfirmButton
            // 
            this.comfirmButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comfirmButton.Location = new System.Drawing.Point(0, 104);
            this.comfirmButton.Name = "comfirmButton";
            this.comfirmButton.Size = new System.Drawing.Size(623, 23);
            this.comfirmButton.TabIndex = 0;
            this.comfirmButton.Text = "OK";
            this.comfirmButton.UseVisualStyleBackColor = true;
            this.comfirmButton.Click += new System.EventHandler(this.comfirmButton_Click);
            // 
            // favReasonTextBox
            // 
            this.favReasonTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.favReasonTextBox.Location = new System.Drawing.Point(0, 0);
            this.favReasonTextBox.Multiline = true;
            this.favReasonTextBox.Name = "favReasonTextBox";
            this.favReasonTextBox.Size = new System.Drawing.Size(623, 104);
            this.favReasonTextBox.TabIndex = 1;
            // 
            // FavoriteReasonPopupForm
            // 
            this.AcceptButton = this.comfirmButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 127);
            this.Controls.Add(this.favReasonTextBox);
            this.Controls.Add(this.comfirmButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FavoriteReasonPopupForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FavoriteReasonPopupForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button comfirmButton;
        private System.Windows.Forms.TextBox favReasonTextBox;
    }
}