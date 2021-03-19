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
        [Description("Determines whether a node is going to be named after its parent when created.")]
        public bool NameNodeAfterParent { get; set; }

        [DisplayName("Buffer size"), Category("Stream")]
        [Description("Buffer size to use for I/O operations.")]
        public int BufferSize { get; set; }

        [DisplayName("Default directory of new CSB projects"), Category("Project")]
        [Description("Default output directory of new CSB projects.")]
        public string ProjectsDirectory { get; set; }

        [DisplayName("Default name of new CSB projects"), Category("Project")]
        [Description("Default name of new CSB projects.")]
        public string ProjectsName { get; set; }

        [DisplayName("Default project directory of imported CSB files"), Category("Project")]
        [Description("Default project directory of imported CSB files.")]
        public ProjectDirectory ImportedCsbProjectDirectory { get; set; }

        [DisplayName("Rename Sound node to referenced Sound Element node"), Category("Application")]
        public bool RenameToSoundElement { get; set; }

        [DisplayName("Enable multi-threading"), Category("Stream")]
        [Description("Determines whether I/O operations are going to be multi-threaded.")]
        public bool EnableThreading { get; set; }

        [DisplayName("Max thread count"), Category("Stream")]
        [Description("Max amount of threads to use for multi-threaded I/O operations.")]
        public int MaxThreads { get; set; }

        [DisplayName("Sound device"), Category("Sound")]
        [Description("Sound device to use for audio playback. Application is going to crash if sound device is not supported.")]
        public NAudioWavePlayer WavePlayer { get; set; }

        [DisplayName("Loop count"), Category("Audio converter")]
        [Description("How many times the audio is going to be looped when converting to .wav.")]
        public int LoopCount { get; set; }

        [DisplayName("Fade Out Time"), Category("Audio Converter")]
        [Description("How much time it takes to fade out when converting to .wav.")]
        public double FadeTime { get; set; }

        [DisplayName("Fade Out Delay Time"), Category("Audio Converter")]
        [Description("How much time it takes before starting to fade out when converting to .wav.")]
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
