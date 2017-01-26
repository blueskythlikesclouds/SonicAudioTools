using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Globalization;


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

        public static string ApplicationVersion
        {
            get
            {
                AssemblyName assemblyName = Assembly.GetEntryAssembly().GetName();
                return $"{assemblyName.Name} (Version {assemblyName.Version.ToString()})";
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Audio.AdxConverter.ConvertToWav(args[0]);
                return;
            }

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Application.ThreadException += OnException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void OnException(object sender, ThreadExceptionEventArgs e)
        {
            new ExceptionForm(e.Exception).ShowDialog();
            Application.Exit();
        }
    }
}
