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

using CsbBuilder.Project;

namespace CsbBuilder
{
    public partial class CreateNewProjectForm : Form
    {
        private CsbProject project = new CsbProject();

        public CsbProject Project
        {
            get
            {
                return project;
            }
        }

        public CreateNewProjectForm(string name) : this()
        {
            maskedTextBox1.Text = Path.GetFileNameWithoutExtension(name);

            string directoryName = Path.GetDirectoryName(name);
            maskedTextBox2.Text = !string.IsNullOrEmpty(directoryName) ? Path.ChangeExtension(name, null) : Path.Combine(Program.ProjectsPath, name);
        }

        public CreateNewProjectForm()
        {
            InitializeComponent();

            maskedTextBox1.Text = project.Name;
            maskedTextBox2.Text = project.Directory.FullName;
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maskedTextBox1.Text))
            {
                MessageBox.Show("Name cannot be empty.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            project.Name = maskedTextBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog selectFolder = new FolderBrowserDialog())
            {
                if (selectFolder.ShowDialog() == DialogResult.OK)
                {
                    maskedTextBox2.Text = selectFolder.SelectedPath;
                }
            }
        }

        private void maskedTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maskedTextBox2.Text))
            {
                MessageBox.Show("Path cannot be empty.", "CSB Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            project.Directory = new DirectoryInfo(maskedTextBox2.Text);
        }
    }
}
