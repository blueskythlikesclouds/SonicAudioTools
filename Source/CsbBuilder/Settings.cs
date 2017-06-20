using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using System.Xml.Serialization;

namespace CsbBuilder.Project
{
    public class Settings : ICloneable
    {
        public enum ProjectDirectory
        {
            DirectoryOfProjects,
            DirectoryOfCsb,
        }

        public enum NAudioWavePlayer
        {
            WaveOut,
            WasapiOut,
            DirectSoundOut,
            AsioOut,
        }

        [DisplayName("Name node after its parent"), Category("General")]
        [Description("Names a node after its parent if it exists, or the node tree name.")]
        public bool NameNodeAfterParent { get; set; }

        [DisplayName("Buffer size"), Category("Stream")]
        [Description("Buffer size used to copy data from streams. Higher values may make streams faster or slower. (e.g. importing/building CSB files)")]
        public int BufferSize { get; set; }

        [DisplayName("Default directory of new CSB projects"), Category("Project")]
        [Description("Default output directory of new CSB projects (relative to where .EXE is) in New Project window.")]
        public string ProjectsDirectory { get; set; }

        [DisplayName("Default name of new CSB projects"), Category("Project")]
        [Description("Default name of new CSB projects in New Project window.")]
        public string ProjectsName { get; set; }

        [DisplayName("Default project directory of imported CSB files"), Category("Project")]
        [Description("Default project output directory of imported CSB files.")]
        public ProjectDirectory ImportedCsbProjectDirectory { get; set; }

        [DisplayName("Rename Sound node to referenced Sound Element node"), Category("Application")]
        public bool RenameToSoundElement { get; set; }

        [DisplayName("Enable threading"), Category("Stream")]
        [Description("Determines whether to use threads to extract data from CSB/CPK files during importing. It may make the importing faster or slower.")]
        public bool EnableThreading { get; set; }

        [DisplayName("Maximum amount of cores"), Category("Stream")]
        [Description("Maximum amount of threads used to extract data from CSB/CPK files during importing.")]
        public int MaxThreads { get; set; }

        [DisplayName("Wave Player"), Category("Sound")]
        [Description("Sound device for audio playback. If not supported, application is going to crash.")]
        public NAudioWavePlayer WavePlayer { get; set; }

        [DisplayName("Loop Count"), Category("Audio Converter")]
        [Description("Count of loop times for audio converter if input audio has loop information.")]
        public int LoopCount { get; set; }

        [DisplayName("Fade Out Time"), Category("Audio Converter")]
        [Description("Fade out time in seconds after total 'Loop Count' loops for audio with loop information.")]
        public double FadeTime { get; set; }

        [DisplayName("Fade Out Delay Time"), Category("Audio Converter")]
        [Description("Delay time in seconds before audio starts to fade out.")]
        public double FadeDelay { get; set; }

        public static Settings Load()
        {
            string path = Path.ChangeExtension(Application.ExecutablePath, "xml");

            Settings settings = null;
            
                if (File.Exists(path))
                {   
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                using (Stream source = File.OpenRead(path))
                {
                    settings = (Settings)serializer.Deserialize(source);
                }
            }

            else
            {
                settings = new Settings();
                settings.Save();
            }

            return settings;
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));

            using (Stream destination = File.Create(Path.ChangeExtension(Application.ExecutablePath, "xml"), BufferSize))
            {
                serializer.Serialize(destination, this);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Settings()
        {
            WavePlayer = NAudioWavePlayer.WaveOut;
            NameNodeAfterParent = true;
            BufferSize = 4096;
            ProjectsDirectory = "Projects";
            ProjectsName = "New CSB Project";
            ImportedCsbProjectDirectory = ProjectDirectory.DirectoryOfCsb;
            RenameToSoundElement = true;
            EnableThreading = true;
            MaxThreads = 4;
            LoopCount = 2;
            FadeTime = 10;
            FadeDelay = 0;
        }
    }
}
