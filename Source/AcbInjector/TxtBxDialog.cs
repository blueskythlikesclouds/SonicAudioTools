using System;
using System.Windows.Forms;

// Stolen and modified from HedgeEdit (https://github.com/Radfordhound/HedgeLib)
namespace HedgeEdit.UI
{
    public partial class TxtBxDialog : Form
    {
        // Variables/Constants
        public string Result;

        public TxtBxDialog(string[] choices)
        {
            InitializeComponent();

            textBox.Visible = false;
            comboBox.Items.AddRange(choices);
            UpdateOKEnabled();
        }

        // Methods
        protected void UpdateOKEnabled()
        {
            okBtn.Enabled = (textBox.Visible &&
                !string.IsNullOrWhiteSpace(textBox.Text)) || (comboBox.Visible &&
                comboBox.SelectedIndex >= 0);
        }

        // GUI Events
        protected void OkBtn_Click(object sender, EventArgs e)
        {
            Result = (comboBox.Visible) ? (string)comboBox.SelectedItem : textBox.Text;
        }

        protected void ValueChanged(object sender, EventArgs e)
        {
            UpdateOKEnabled();
        }

        private void OnHelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("This is the ID of audio file you are going to replace.\n\nTo get the ID, get the numerical part of file name. (eg. 00005_streaming, where the ID is 5)\n\nPlease note that you need to use files that ACB Editor extracts for reference.", "ACB Injector", MessageBoxButtons.OK, MessageBoxIcon.Information);
            e.Cancel = true;
        }
    }
}