using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsbBuilder
{
    public partial class ExceptionForm : Form
    {
        private Exception exception;
        
        public ExceptionForm(Exception exception)
        {
            InitializeComponent();

            this.exception = exception;

            if (this.exception == null)
            {
                this.exception = new Exception("The exception reference is for some reason set to null! Please don't forget to report what you were doing before this happened.");
            }

            richTextBox1.Text = exception.Message;
            richTextBox2.Text = exception.StackTrace;
        }

        private void CopyReport(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Application: {Program.ApplicationVersion}");
            stringBuilder.AppendLine($"Exception Message: {exception.Message}");
            stringBuilder.AppendLine($"Exception Details:");
            stringBuilder.AppendLine(exception.StackTrace);
            Clipboard.SetText(stringBuilder.ToString());
        }
    }
}
