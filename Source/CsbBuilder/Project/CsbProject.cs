using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CsbBuilder.BuilderNode;

using System.Xml.Serialization;
using System.Windows.Forms;

using CsbBuilder;

namespace CsbBuilder.Project
{
    public class CsbProject
    {
        private string name;
        private DirectoryInfo directory;

        private List<BuilderCueNode> cueNodes = new List<BuilderCueNode>();
        private List<BuilderSynthNode> synthNodes = new List<BuilderSynthNode>();
        private List<BuilderSoundElementNode> soundElementNodes = new List<BuilderSoundElementNode>();
        private List<BuilderAisacNode> aisacNodes = new List<BuilderAisacNode>();
        private List<BuilderVoiceLimitGroupNode> voiceLimitGroupNodes = new List<BuilderVoiceLimitGroupNode>();

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        [XmlIgnore]
        public DirectoryInfo Directory
        {
            get
            {
                return directory;
            }

            set
            {
                directory = value;
            }
        }

        [XmlIgnore]
        public DirectoryInfo AudioDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(directory.FullName, "Audio"));
            }
        }

        [XmlIgnore]
        public DirectoryInfo BinaryDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(directory.FullName, "Binary"));
            }
        }

        [XmlIgnore]
        public FileInfo ProjectFile
        {
            get
            {
                return new FileInfo(Path.Combine(directory.FullName, $"{name}.csbproject"));
            }
        }

        [XmlArray("CueNodes"), XmlArrayItem(typeof(BuilderCueNode))]
        public List<BuilderCueNode> CueNodes
        {
            get
            {
                return cueNodes;
            }
        }

        [XmlArray("SynthNodes"), XmlArrayItem(typeof(BuilderSynthNode))]
        public List<BuilderSynthNode> SynthNodes
        {
            get
            {
                return synthNodes;
            }
        }

        [XmlArray("SoundElementNodes"), XmlArrayItem(typeof(BuilderSoundElementNode))]
        public List<BuilderSoundElementNode> SoundElementNodes
        {
            get
            {
                return soundElementNodes;
            }
        }

        [XmlArray("AisacNodes"), XmlArrayItem(typeof(BuilderAisacNode))]
        public List<BuilderAisacNode> AisacNodes
        {
            get
            {
                return aisacNodes;
            }
        }

        [XmlArray("VoiceLimitGroupNodes"), XmlArrayItem(typeof(BuilderVoiceLimitGroupNode))]
        public List<BuilderVoiceLimitGroupNode> VoiceLimitGroupNodes
        {
            get
            {
                return voiceLimitGroupNodes;
            }
        }

        private void GetAbsoluteIndex(string path, TreeNode treeNode, ref int index, ref bool stop)
        {
            if (!stop)
            {
                index++;

                if (treeNode.FullPath == path)
                {
                    stop = true;
                }

                else
                {
                    foreach (TreeNode childNode in treeNode.Nodes)
                    {
                        GetAbsoluteIndex(path, childNode, ref index, ref stop);
                    }
                }
            }
        }

        private int GetAbsoluteIndex(string path, TreeView treeView)
        {
            bool stop = false;
            int index = -1;

            foreach (TreeNode treeNode in treeView.Nodes)
            {
                GetAbsoluteIndex(path, treeNode, ref index, ref stop);
            }

            return index;
        }

        public void Order(TreeView cueTree, TreeView synthTree, TreeView soundElementTree, TreeView aisacTree, TreeView voiceLimitGroupTree)
        {
            cueNodes = cueNodes.OrderBy(cue => cueTree.Nodes.IndexOfKey(cue.Name)).ToList();
            synthNodes = synthNodes.OrderBy(synth => GetAbsoluteIndex(synth.Name, synthTree)).ToList();
            soundElementNodes = soundElementNodes.OrderBy(soundElement => soundElementTree.FindNodeByFullPath(soundElement.Name).Index).ToList();
            aisacNodes = aisacNodes.OrderBy(aisac => aisacTree.FindNodeByFullPath(aisac.Name).Index).ToList();
            voiceLimitGroupNodes = voiceLimitGroupNodes.OrderBy(voiceLimitGroup => voiceLimitGroupTree.Nodes.IndexOfKey(voiceLimitGroup.Name)).ToList();

            synthNodes.ForEach(synth =>
            {
                if (synth.Children.Count > 0)
                {
                    synth.Children = synth.Children.OrderBy(child => synthTree.FindNodeByFullPath(child).Index).ToList();
                }
            });
        }

        public static CsbProject Load(string projectFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CsbProject));

            CsbProject csbProject = null;
            using (Stream source = File.OpenRead(projectFile))
            {
                csbProject = (CsbProject)serializer.Deserialize(source);
            }

            csbProject.Directory = new DirectoryInfo(Path.GetDirectoryName(projectFile));
            return csbProject;
        }

        public string AddAudio(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string name = Path.GetFileName(path);
            string nameNoExtension = Path.GetFileNameWithoutExtension(name);
            string outputPath = Path.Combine(AudioDirectory.FullName, name);

            if (path != outputPath)
            {
                string uniqueName = nameNoExtension;

                int index = -1;
                while (File.Exists(Path.Combine(AudioDirectory.FullName, $"{uniqueName}.adx")))
                {
                    uniqueName = $"{nameNoExtension}_{++index}";
                }

                outputPath = Path.Combine(AudioDirectory.FullName, $"{uniqueName}.adx");
                File.Copy(path, outputPath, true);

                name = $"{uniqueName}.adx";
            }

            return name;
        }

        public string GetFullAudioPath(string name)
        {
            return Path.Combine(AudioDirectory.FullName, name);
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CsbProject));

            using (Stream destination = ProjectFile.Create())
            {
                serializer.Serialize(destination, this);
            }
        }

        public void SaveAs(string outputDirectory)
        {
            DirectoryInfo oldAudioDirectory = AudioDirectory;

            name = Path.GetFileNameWithoutExtension(outputDirectory);
            directory = new DirectoryInfo(Path.GetDirectoryName(outputDirectory));

            Create();
            Save();

            if (oldAudioDirectory.Exists)
            {
                foreach (FileInfo audioFile in oldAudioDirectory.EnumerateFiles())
                {
                    audioFile.CopyTo(Path.Combine(AudioDirectory.FullName, audioFile.Name), true);
                }
            }
        }

        public void Create()
        {
            directory.Create();
            AudioDirectory.Create();
        }

        public CsbProject()
        {
            name = MainForm.Settings.ProjectsName;
            directory = new DirectoryInfo(Path.Combine(MainForm.Settings.ProjectsDirectory, name));
        }
    }
}
