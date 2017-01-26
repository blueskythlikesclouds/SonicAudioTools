using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using CsbBuilder.Project;
using CsbBuilder.BuilderNode;
using CsbBuilder.Serialization;

using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;
using SonicAudioLib.Archive;

namespace CsbBuilder.Builder
{
    public static class CsbBuilder
    {
        public static void Build(CsbProject project, string outputFileName)
        {
            CriCpkArchive cpkArchive = new CriCpkArchive();

            DirectoryInfo outputDirectory = new DirectoryInfo(Path.GetDirectoryName(outputFileName));

            List<SerializationCueSheetTable> cueSheetTables = new List<SerializationCueSheetTable>();

            SerializationVersionInfoTable versionInfoTable = new SerializationVersionInfoTable();
            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(new List<SerializationVersionInfoTable>() { versionInfoTable }, CriTableWriterSettings.AdxSettings),
                Name = "INFO",
                TableType = 7,
            });

            // Serialize cues.
            List<SerializationCueTable> cueTables = new List<SerializationCueTable>();
            foreach (BuilderCueNode cueNode in project.CueNodes)
            {
                cueTables.Add(new SerializationCueTable
                {
                    Name = cueNode.Name,
                    Id = cueNode.Identifier,
                    UserData = cueNode.UserComment,
                    Flags = cueNode.Flags,
                    SynthPath = cueNode.SynthReference,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(cueTables, CriTableWriterSettings.AdxSettings),
                Name = "CUE",
                TableType = 1,
            });

            // Serialize synth tables.
            List<SerializationSynthTable> synthTables = new List<SerializationSynthTable>();
            foreach (BuilderSynthNode synthNode in project.SynthNodes)
            {
                SerializationSynthTable synthTable = new SerializationSynthTable
                {
                    SynthName = synthNode.Name,
                    SynthType = (byte)synthNode.Type,
                    ComplexType = (byte)synthNode.PlaybackType,
                    Volume = synthNode.Volume,
                    Pitch = synthNode.Pitch,
                    DelayTime = synthNode.DelayTime,
                    SControl = synthNode.SControl,
                    EgDelay = synthNode.EgDelay,
                    EgAttack = synthNode.EgAttack,
                    EgHold = synthNode.EgHold,
                    EgDecay = synthNode.EgDecay,
                    EgRelease = synthNode.EgRelease,
                    EgSustain = synthNode.EgSustain,
                    FType = synthNode.FilterType,
                    FCof1 = synthNode.FilterCutoff1,
                    FCof2 = synthNode.FilterCutoff2,
                    FReso = synthNode.FilterReso,
                    FReleaseOffset = synthNode.FilterReleaseOffset,
                    DryOName = synthNode.DryOName,
                    Mtxrtr = synthNode.Mtxrtr,
                    Dry0 = synthNode.Dry0,
                    Dry1 = synthNode.Dry1,
                    Dry2 = synthNode.Dry2,
                    Dry3 = synthNode.Dry3,
                    Dry4 = synthNode.Dry4,
                    Dry5 = synthNode.Dry5,
                    Dry6 = synthNode.Dry6,
                    Dry7 = synthNode.Dry7,
                    WetOName = synthNode.WetOName,
                    Wet0 = synthNode.Wet0,
                    Wet1 = synthNode.Wet1,
                    Wet2 = synthNode.Wet2,
                    Wet3 = synthNode.Wet3,
                    Wet4 = synthNode.Wet4,
                    Wet5 = synthNode.Wet5,
                    Wet6 = synthNode.Wet6,
                    Wet7 = synthNode.Wet7,
                    Wcnct0 = synthNode.Wcnct0,
                    Wcnct1 = synthNode.Wcnct1,
                    Wcnct2 = synthNode.Wcnct2,
                    Wcnct3 = synthNode.Wcnct3,
                    Wcnct4 = synthNode.Wcnct4,
                    Wcnct5 = synthNode.Wcnct5,
                    Wcnct6 = synthNode.Wcnct6,
                    Wcnct7 = synthNode.Wcnct7,
                    VoiceLimitGroupName = synthNode.VoiceLimitGroupReference,
                    VoiceLimitType = synthNode.VoiceLimitType,
                    VoiceLimitPriority = synthNode.VoiceLimitPriority,
                    VoiceLimitPhTime = synthNode.VoiceLimitProhibitionTime,
                    VoiceLimitPcdlt = synthNode.VoiceLimitPcdlt,
                    Pan3dVolumeOffset = synthNode.Pan3dVolumeOffset,
                    Pan3dVolumeGain = synthNode.Pan3dVolumeGain,
                    Pan3dAngleOffset = synthNode.Pan3dAngleOffset,
                    Pan3dAngleGain = synthNode.Pan3dAngleGain,
                    Pan3dDistanceOffset = synthNode.Pan3dDistanceOffset,
                    Pan3dDistanceGain = synthNode.Pan3dDistanceGain,
                    Dry0g = synthNode.Dry0g,
                    Dry1g = synthNode.Dry1g,
                    Dry2g = synthNode.Dry2g,
                    Dry3g = synthNode.Dry3g,
                    Dry4g = synthNode.Dry4g,
                    Dry5g = synthNode.Dry5g,
                    Dry6g = synthNode.Dry6g,
                    Dry7g = synthNode.Dry7g,
                    Wet0g = synthNode.Wet0g,
                    Wet1g = synthNode.Wet1g,
                    Wet2g = synthNode.Wet2g,
                    Wet3g = synthNode.Wet3g,
                    Wet4g = synthNode.Wet4g,
                    Wet5g = synthNode.Wet5g,
                    Wet6g = synthNode.Wet6g,
                    Wet7g = synthNode.Wet7g,
                    F1Type = synthNode.Filter1Type,
                    F1CofOffset = synthNode.Filter1CutoffOffset,
                    F1CofGain = synthNode.Filter1CutoffGain,
                    F1ResoOffset = synthNode.Filter1ResoOffset,
                    F1ResoGain = synthNode.Filter1ResoGain,
                    F2Type = synthNode.Filter2Type,
                    F2CofLowOffset = synthNode.Filter2CutoffLowerOffset,
                    F2CofLowGain = synthNode.Filter2CutoffLowerGain,
                    F2CofHighOffset = synthNode.Filter2CutoffHigherOffset,
                    F2CofHighGain = synthNode.Filter2CutoffHigherGain,
                    Probability = synthNode.PlaybackProbability,
                    NumberLmtChildren = synthNode.NLmtChildren,
                    Repeat = synthNode.Repeat,
                    ComboTime = synthNode.ComboTime,
                    ComboLoopBack = synthNode.ComboLoopBack,
                };

                if (synthNode.Type == BuilderSynthType.Single)
                {
                    synthTable.LinkName = synthNode.SoundElementReference;
                }

                else if (synthNode.Type == BuilderSynthType.WithChildren)
                {
                    foreach (string trackReference in synthNode.Children)
                    {
                        synthTable.LinkName += trackReference + (char)0x0A;
                    }
                }

                if (!string.IsNullOrEmpty(synthNode.AisacReference))
                {
                    BuilderAisacNode aisacNode = project.AisacNodes.First(aisac => aisac.Name == synthNode.AisacReference);
                    synthTable.AisacSetName = aisacNode.AisacName + "::" + aisacNode.Name + (char)0x0A;
                }

                synthTables.Add(synthTable);
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(synthTables, CriTableWriterSettings.AdxSettings),
                Name = "SYNTH",
                TableType = 2,
            });

            List<FileInfo> junks = new List<FileInfo>();

            // Serialize the sound elements.
            List<SerializationSoundElementTable> soundElementTables = new List<SerializationSoundElementTable>();
            foreach (BuilderSoundElementNode soundElementNode in project.SoundElementNodes)
            {
                CriAaxArchive aaxArchive = new CriAaxArchive();

                if (!string.IsNullOrEmpty(soundElementNode.Intro))
                {
                    aaxArchive.Add(new CriAaxEntry
                    {
                        Flag = CriAaxEntryFlag.Intro,
                        FilePath = new FileInfo(Path.Combine(project.AudioDirectory.FullName, soundElementNode.Intro)),
                    });
                }

                if (!string.IsNullOrEmpty(soundElementNode.Loop))
                {
                    aaxArchive.Add(new CriAaxEntry
                    {
                        Flag = CriAaxEntryFlag.Loop,
                        FilePath = new FileInfo(Path.Combine(project.AudioDirectory.FullName, soundElementNode.Loop)),
                    });
                }

                byte[] data = new byte[0];

                if (soundElementNode.Streaming)
                {
                    CriCpkEntry entry = new CriCpkEntry();
                    entry.Name = Path.GetFileName(soundElementNode.Name);
                    entry.DirectoryName = Path.GetDirectoryName(soundElementNode.Name);
                    entry.Id = (uint)cpkArchive.Count;
                    entry.UpdateDateTime = DateTime.Now;
                    entry.FilePath = new FileInfo(Path.GetTempFileName());
                    cpkArchive.Add(entry);

                    aaxArchive.Save(entry.FilePath.FullName);
                    junks.Add(entry.FilePath);
                }

                else
                {
                    data = aaxArchive.Save();
                }

                soundElementTables.Add(new SerializationSoundElementTable
                {
                    Name = soundElementNode.Name,
                    Data = data,
                    FormatType = 0,
                    SoundFrequency = soundElementNode.SampleRate,
                    NumberChannels = soundElementNode.ChannelCount,
                    Streaming = soundElementNode.Streaming,
                    NumberSamples = soundElementNode.SampleCount,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(soundElementTables, CriTableWriterSettings.AdxSettings),
                Name = "SOUND_ELEMENT",
                TableType = 4,
            });

            // Serialize the aisacs.
            List<SerializationAisacTable> aisacTables = new List<SerializationAisacTable>();
            foreach (BuilderAisacNode aisacNode in project.AisacNodes)
            {
                List<SerializationAisacGraphTable> graphTables = new List<SerializationAisacGraphTable>();
                foreach (BuilderAisacGraphNode graphNode in aisacNode.Graphs)
                {
                    List<SerializationAisacPointTable> pointTables = new List<SerializationAisacPointTable>();
                    foreach (BuilderAisacPointNode pointNode in graphNode.Points)
                    {
                        pointTables.Add(new SerializationAisacPointTable
                        {
                            In = pointNode.X,
                            Out = pointNode.Y,
                        });
                    }

                    graphTables.Add(new SerializationAisacGraphTable
                    {
                        Points = CriTableSerializer.Serialize(pointTables, CriTableWriterSettings.AdxSettings),
                        Type = graphNode.Type,
                        InMax = graphNode.MaximumX,
                        InMin = graphNode.MinimumX,
                        OutMax = graphNode.MaximumY,
                        OutMin = graphNode.MinimumY,
                    });
                }

                aisacTables.Add(new SerializationAisacTable
                {
                    Graph = CriTableSerializer.Serialize(graphTables, CriTableWriterSettings.AdxSettings),
                    Name = aisacNode.AisacName,
                    PathName = aisacNode.Name,
                    Type = aisacNode.Type,
                    RandomRange = aisacNode.RandomRange,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(aisacTables, CriTableWriterSettings.AdxSettings),
                Name = "ISAAC",
                TableType = 5,
            });

            // Serialize the voice limit groups.
            List<SerializationVoiceLimitGroupTable> voiceLimitGroupTables = new List<SerializationVoiceLimitGroupTable>();
            foreach (BuilderVoiceLimitGroupNode voiceLimitGroupNode in project.VoiceLimitGroupNodes)
            {
                voiceLimitGroupTables.Add(new SerializationVoiceLimitGroupTable
                {
                    VoiceLimitGroupName = voiceLimitGroupNode.Name,
                    VoiceLimitGroupNum = voiceLimitGroupNode.MaxAmountOfInstances,
                });
            }

            cueSheetTables.Add(new SerializationCueSheetTable
            {
                TableData = CriTableSerializer.Serialize(voiceLimitGroupTables, CriTableWriterSettings.AdxSettings),
                Name = "VOICE_LIMIT_GROUP",
                TableType = 6,
            });

            // Finally, serialize the CSB file.
            CriTableSerializer.Serialize(outputFileName, cueSheetTables, CriTableWriterSettings.AdxSettings);

            if (cpkArchive.Count > 0)
            {
                cpkArchive.Save(Path.ChangeExtension(outputFileName, "cpk"));
            }

            foreach (FileInfo junk in junks)
            {
                junk.Delete();
            }
        }
    }
}
