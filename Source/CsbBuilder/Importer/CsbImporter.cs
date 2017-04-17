using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using CsbBuilder.Audio;
using CsbBuilder.Project;
using CsbBuilder.BuilderNode;
using CsbBuilder.Serialization;

using SonicAudioLib.IO;
using SonicAudioLib.CriMw.Serialization;
using SonicAudioLib.Archive;

using System.Windows.Forms;

namespace CsbBuilder.Importer
{
    public static class CsbImporter
    {
        public static void Import(string path, CsbProject project)
        {
            // Find the CPK first
            string cpkPath = Path.ChangeExtension(path, "cpk");
            bool exists = File.Exists(cpkPath);

            CriCpkArchive cpkArchive = new CriCpkArchive();

            // First, deserialize the main tables
            List<SerializationCueSheetTable> cueSheets = CriTableSerializer.Deserialize<SerializationCueSheetTable>(path, MainForm.Settings.BufferSize);

            /* Deserialize all the tables we need to import.
             * None = 0,
             * Cue = 1,
             * Synth = 2,
             * SoundElement = 4,
             * Aisac = 5,
             * VoiceLimitGroup = 6,
             * VersionInfo = 7,
             */

            List<SerializationCueTable> cueTables = CriTableSerializer.Deserialize<SerializationCueTable>(cueSheets.FirstOrDefault(table => table.TableType == 1).TableData);
            List<SerializationSynthTable> synthTables = CriTableSerializer.Deserialize<SerializationSynthTable>(cueSheets.FirstOrDefault(table => table.TableType == 2).TableData);
            List<SerializationSoundElementTable> soundElementTables = CriTableSerializer.Deserialize<SerializationSoundElementTable>(cueSheets.FirstOrDefault(table => table.TableType == 4).TableData);
            List<SerializationAisacTable> aisacTables = CriTableSerializer.Deserialize<SerializationAisacTable>(cueSheets.FirstOrDefault(table => table.TableType == 5).TableData);

            // voice limit groups appeared in the later versions, so check if it exists.
            List<SerializationVoiceLimitGroupTable> voiceLimitGroupTables = new List<SerializationVoiceLimitGroupTable>();

            if (cueSheets.Exists(table => table.TableType == 6))
            {
                voiceLimitGroupTables = CriTableSerializer.Deserialize<SerializationVoiceLimitGroupTable>(cueSheets.FirstOrDefault(table => table.TableType == 6).TableData);
            }

            // Deserialize Sound Element tables

            // BUT BEFORE THAT, see if there's any sound element with Streamed on
            if (soundElementTables.Exists(soundElementTable => soundElementTable.Streaming))
            {
                if (!exists)
                {
                    throw new Exception("Cannot find CPK file for this CSB file. Please ensure that the CPK file is in the directory where the CSB file is, and has the same name as the CSB file, but with .CPK extension.");
                }

                cpkArchive.Load(cpkPath);
            }

            foreach (SerializationSoundElementTable soundElementTable in soundElementTables)
            {
                BuilderSoundElementNode soundElementNode = new BuilderSoundElementNode();
                soundElementNode.Name = soundElementTable.Name;
                soundElementNode.ChannelCount = soundElementTable.NumberChannels;
                soundElementNode.SampleRate = soundElementTable.SoundFrequency;
                soundElementNode.Streaming = soundElementTable.Streaming;

                CriAaxArchive aaxArchive = new CriAaxArchive();

                byte[] aaxData = soundElementTable.Data;

                if (exists && soundElementNode.Streaming)
                {
                    using (Stream source = File.OpenRead(cpkPath))
                    using (Stream entrySource = cpkArchive.GetByPath(soundElementTable.Name).Open(source))
                    {
                        aaxData = ((Substream)entrySource).ToArray();
                    }
                }

                aaxArchive.Load(aaxData);

                foreach (CriAaxEntry entry in aaxArchive)
                {
                    byte[] data = new byte[entry.Length];
                    Array.Copy(aaxData, entry.Position, data, 0, data.Length);

                    string outputFileName = Path.Combine(project.AudioDirectory.FullName, soundElementTable.Name.Replace('/', '_'));
                    if (entry.Flag == CriAaxEntryFlag.Intro)
                    {
                        outputFileName += "_Intro.adx";
                        soundElementNode.Intro = Path.GetFileName(outputFileName);
                    }

                    else if (entry.Flag == CriAaxEntryFlag.Loop)
                    {
                        outputFileName += "_Loop.adx";
                        soundElementNode.Loop = Path.GetFileName(outputFileName);
                    }

                    File.WriteAllBytes(outputFileName, data);

                    // Read the samples just in case
                    soundElementNode.SampleCount += AdxFileReader.LoadHeader(outputFileName).SampleCount;
                }

                project.SoundElementNodes.Add(soundElementNode);
            }

            // Deserialize Voice Limit Group tables
            foreach (SerializationVoiceLimitGroupTable voiceLimitGroupTable in voiceLimitGroupTables)
            {
                project.VoiceLimitGroupNodes.Add(new BuilderVoiceLimitGroupNode
                {
                    Name = voiceLimitGroupTable.VoiceLimitGroupName,
                    MaxAmountOfInstances = voiceLimitGroupTable.VoiceLimitGroupNum,
                });
            }

            // Deserialize Aisac tables
            foreach (SerializationAisacTable aisacTable in aisacTables)
            {
                BuilderAisacNode aisacNode = new BuilderAisacNode();
                aisacNode.Name = aisacTable.PathName;
                aisacNode.AisacName = aisacTable.Name;
                aisacNode.Type = aisacTable.Type;
                aisacNode.RandomRange = aisacTable.RandomRange;

                // Deserialize the graphs
                List<SerializationAisacGraphTable> graphTables = CriTableSerializer.Deserialize<SerializationAisacGraphTable>(aisacTable.Graph);
                foreach (SerializationAisacGraphTable graphTable in graphTables)
                {
                    BuilderAisacGraphNode graphNode = new BuilderAisacGraphNode();
                    graphNode.Name = $"Graph{aisacNode.Graphs.Count}";
                    graphNode.Type = graphTable.Type;
                    graphNode.MaximumX = graphTable.InMax;
                    graphNode.MinimumX = graphTable.InMin;
                    graphNode.MaximumY = graphTable.OutMax;
                    graphNode.MinimumY = graphTable.OutMin;

                    // Deserialize the points
                    List<SerializationAisacPointTable> pointTables = CriTableSerializer.Deserialize<SerializationAisacPointTable>(graphTable.Points);
                    foreach (SerializationAisacPointTable pointTable in pointTables)
                    {
                        BuilderAisacPointNode pointNode = new BuilderAisacPointNode();
                        pointNode.Name = $"Point{graphNode.Points.Count}";
                        pointNode.X = pointTable.In;
                        pointNode.Y = pointTable.Out;
                        graphNode.Points.Add(pointNode);
                    }

                    aisacNode.Graphs.Add(graphNode);
                }

                project.AisacNodes.Add(aisacNode);
            }

            // Deserialize Synth tables
            foreach (SerializationSynthTable synthTable in synthTables)
            {
                BuilderSynthNode synthNode = new BuilderSynthNode();
                synthNode.Name = synthTable.SynthName;
                synthNode.Type = (BuilderSynthType)synthTable.SynthType;
                synthNode.PlaybackType = (BuilderSynthPlaybackType)synthTable.ComplexType;
                synthNode.Volume = synthTable.Volume;
                synthNode.Pitch = synthTable.Pitch;
                synthNode.DelayTime = synthTable.DelayTime;
                synthNode.SControl = synthTable.SControl;
                synthNode.EgDelay = synthTable.EgDelay;
                synthNode.EgAttack = synthTable.EgAttack;
                synthNode.EgHold = synthTable.EgHold;
                synthNode.EgDecay = synthTable.EgDecay;
                synthNode.EgRelease = synthTable.EgRelease;
                synthNode.EgSustain = synthTable.EgSustain;
                synthNode.FilterType = synthTable.FType;
                synthNode.FilterCutoff1 = synthTable.FCof1;
                synthNode.FilterCutoff2 = synthTable.FCof2;
                synthNode.FilterReso = synthTable.FReso;
                synthNode.FilterReleaseOffset = synthTable.FReleaseOffset;
                synthNode.DryOName = synthTable.DryOName;
                synthNode.Mtxrtr = synthTable.Mtxrtr;
                synthNode.Dry0 = synthTable.Dry0;
                synthNode.Dry1 = synthTable.Dry1;
                synthNode.Dry2 = synthTable.Dry2;
                synthNode.Dry3 = synthTable.Dry3;
                synthNode.Dry4 = synthTable.Dry4;
                synthNode.Dry5 = synthTable.Dry5;
                synthNode.Dry6 = synthTable.Dry6;
                synthNode.Dry7 = synthTable.Dry7;
                synthNode.WetOName = synthTable.WetOName;
                synthNode.Wet0 = synthTable.Wet0;
                synthNode.Wet1 = synthTable.Wet1;
                synthNode.Wet2 = synthTable.Wet2;
                synthNode.Wet3 = synthTable.Wet3;
                synthNode.Wet4 = synthTable.Wet4;
                synthNode.Wet5 = synthTable.Wet5;
                synthNode.Wet6 = synthTable.Wet6;
                synthNode.Wet7 = synthTable.Wet7;
                synthNode.Wcnct0 = synthTable.Wcnct0;
                synthNode.Wcnct1 = synthTable.Wcnct1;
                synthNode.Wcnct2 = synthTable.Wcnct2;
                synthNode.Wcnct3 = synthTable.Wcnct3;
                synthNode.Wcnct4 = synthTable.Wcnct4;
                synthNode.Wcnct5 = synthTable.Wcnct5;
                synthNode.Wcnct6 = synthTable.Wcnct6;
                synthNode.Wcnct7 = synthTable.Wcnct7;
                synthNode.VoiceLimitType = synthTable.VoiceLimitType;
                synthNode.VoiceLimitPriority = synthTable.VoiceLimitPriority;
                synthNode.VoiceLimitProhibitionTime = synthTable.VoiceLimitPhTime;
                synthNode.VoiceLimitPcdlt = synthTable.VoiceLimitPcdlt;
                synthNode.Pan3dVolumeOffset = synthTable.Pan3dVolumeOffset;
                synthNode.Pan3dVolumeGain = synthTable.Pan3dVolumeGain;
                synthNode.Pan3dAngleOffset = synthTable.Pan3dAngleOffset;
                synthNode.Pan3dAngleGain = synthTable.Pan3dAngleGain;
                synthNode.Pan3dDistanceOffset = synthTable.Pan3dDistanceOffset;
                synthNode.Pan3dDistanceGain = synthTable.Pan3dDistanceGain;
                synthNode.Dry0g = synthTable.Dry0g;
                synthNode.Dry1g = synthTable.Dry1g;
                synthNode.Dry2g = synthTable.Dry2g;
                synthNode.Dry3g = synthTable.Dry3g;
                synthNode.Dry4g = synthTable.Dry4g;
                synthNode.Dry5g = synthTable.Dry5g;
                synthNode.Dry6g = synthTable.Dry6g;
                synthNode.Dry7g = synthTable.Dry7g;
                synthNode.Wet0g = synthTable.Wet0g;
                synthNode.Wet1g = synthTable.Wet1g;
                synthNode.Wet2g = synthTable.Wet2g;
                synthNode.Wet3g = synthTable.Wet3g;
                synthNode.Wet4g = synthTable.Wet4g;
                synthNode.Wet5g = synthTable.Wet5g;
                synthNode.Wet6g = synthTable.Wet6g;
                synthNode.Wet7g = synthTable.Wet7g;
                synthNode.Filter1Type = synthTable.F1Type;
                synthNode.Filter1CutoffOffset = synthTable.F1CofOffset;
                synthNode.Filter1CutoffGain = synthTable.F1CofGain;
                synthNode.Filter1ResoOffset = synthTable.F1ResoOffset;
                synthNode.Filter1ResoGain = synthTable.F1ResoGain;
                synthNode.Filter2Type = synthTable.F2Type;
                synthNode.Filter2CutoffLowerOffset = synthTable.F2CofLowOffset;
                synthNode.Filter2CutoffLowerGain = synthTable.F2CofLowGain;
                synthNode.Filter2CutoffHigherOffset = synthTable.F2CofHighOffset;
                synthNode.Filter2CutoffHigherGain = synthTable.F2CofHighGain;
                synthNode.PlaybackProbability = synthTable.Probability;
                synthNode.NLmtChildren = synthTable.NumberLmtChildren;
                synthNode.Repeat = synthTable.Repeat;
                synthNode.ComboTime = synthTable.ComboTime;
                synthNode.ComboLoopBack = synthTable.ComboLoopBack;

                project.SynthNodes.Add(synthNode);
            }

            // Convert the cue tables
            foreach (SerializationCueTable cueTable in cueTables)
            {
                BuilderCueNode cueNode = new BuilderCueNode();
                cueNode.Name = cueTable.Name;
                cueNode.Identifier = cueTable.Id;
                cueNode.UserComment = cueTable.UserData;
                cueNode.Flags = cueTable.Flags;
                cueNode.SynthReference = cueTable.SynthPath;
                project.CueNodes.Add(cueNode);
            }

            // Fix links
            for (int i = 0; i < synthTables.Count; i++)
            {
                SerializationSynthTable synthTable = synthTables[i];
                BuilderSynthNode synthNode = project.SynthNodes[i];

                if (synthNode.Type == BuilderSynthType.Single)
                {
                    synthNode.SoundElementReference = synthTable.LinkName;
                }

                // Polyphonic
                else if (synthNode.Type == BuilderSynthType.WithChildren)
                {
                    synthNode.Children = synthTable.LinkName.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                if (!string.IsNullOrEmpty(synthTable.AisacSetName))
                {
                    string[] aisacs = synthTable.AisacSetName.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries);
                    string[] name = aisacs[0].Split(new string[] { "::" }, StringSplitOptions.None);
                    synthNode.AisacReference = name[1]; // will add support for multiple aisacs (I'm actually not even sure if csbs support multiple aisacs...)
                }

                if (!string.IsNullOrEmpty(synthTable.VoiceLimitGroupName))
                {
                    synthNode.VoiceLimitGroupReference = synthTable.VoiceLimitGroupName;
                }
            }
        }
    }
}
