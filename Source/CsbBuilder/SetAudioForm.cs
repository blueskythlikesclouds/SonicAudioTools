using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CsbBuilder
{
    public partial class SetAudioForm : Form
    {
        public SetAudioForm(string intro, string loop)
        {
            InitializeComponent();

            introTextBox.Text = intro;
            loopTextBox.Text = loop;
        }

        public string Intro
        {
            get
            {
                return introTextBox.Text;
            }
        }

        public string Loop
        {
            get
            {
                return loopTextBox.Text;
            }
        }

        private void Swap(object sender, EventArgs e)
        {
            string intro = introTextBox.Text;
            string loop = loopTextBox.Text;

            introTextBox.Text = loop;
            loopTextBox.Text = intro;
        }

        private void BrowseIntro(object sender, EventArgs e)
        {
            using (OpenFileDialog openAdx = new OpenFileDialog
            {
                Title = "Select Your Audio File",
                Filter = "All Files|*.adx;*.wav|ADX Files|*.adx|WAV Files|*.wav",
            })
            {
                if (openAdx.ShowDialog() == DialogResult.OK)
                {
                    introTextBox.Text = openAdx.FileName;
                }
            }
        }

        private void BrowseLoop(object sender, EventArgs e)
        {
            using (OpenFileDialog openAdx = new OpenFileDialog
            {
                Title = "Select Your Audio File",
                Filter = "All Files|*.adx;*.wav|ADX Files|*.adx|WAV Files|*.wav",
            })
            {
                if (openAdx.ShowDialog() == DialogResult.OK)
                {
                    loopTextBox.Text = openAdx.FileName;
                }
            }
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if ((!string.IsNullOrEmpty(Intro) && !File.Exists(Intro)) || (!string.IsNullOrEmpty(Loop) && !File.Exists(Loop)))
                {
                    MessageBox.Show("File(s) not found.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }

                string introExtension = Path.GetExtension(Intro);
                string loopExtension = Path.GetExtension(Loop);

                if (!string.IsNullOrEmpty(Intro) && !string.IsNullOrEmpty(Loop) && !introExtension.Equals(loopExtension, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Please use the same types of audio files for Intro and Loop.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }
    }
}
