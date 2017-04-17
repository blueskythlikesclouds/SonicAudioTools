using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CsbBuilder.Properties;
using CsbBuilder.Project;

namespace CsbBuilder
{
    public partial class SettingsForm : Form
    {
        private bool saved = true;

        public SettingsForm()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = MainForm.Settings.Clone();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            saved = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saved = true;

            MainForm.Settings = (Settings)propertyGrid1.SelectedObject;
            MainForm.Settings.Save();

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved && !MainForm.Settings.Equals(propertyGrid1.SelectedObject))
            {
                DialogResult result = MessageBox.Show("Do you want to save your changes?", "CSB Builder", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                    button1_Click(null, null);
                }

                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
