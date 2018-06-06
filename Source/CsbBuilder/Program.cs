using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Globalization;

using NAudio.Wave;

namespace CsbBuilder
{
    static class Program
    {
        public static string ProjectsPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Projects");
            }
        }

        public static string ApplicationTitle
        {
            get
            {
                AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
                return $"CSB Builder {assemblyName.Version.ToString()}";
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
#if !DEBUG
            Application.ThreadException += OnException;
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void OnException(object sender, ThreadExceptionEventArgs e)
        {
            new ExceptionForm(e.Exception).ShowDialog();

            if (MessageBox.Show("Do you want to continue?", "CSB Builder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
