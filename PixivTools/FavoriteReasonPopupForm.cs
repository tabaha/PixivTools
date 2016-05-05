using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixivTools {
    public partial class FavoriteReasonPopupForm : Form {
        public FavoriteReasonPopupForm() {
            InitializeComponent();
        }

        private void comfirmButton_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string GetReason() {
            return favReasonTextBox.Text;
        }
    }
}
