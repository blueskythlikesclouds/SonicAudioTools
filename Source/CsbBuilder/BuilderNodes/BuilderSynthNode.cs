using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace CsbBuilder.BuilderNodes
{
    public enum BuilderSynthType
    {
        Single,
        WithChildren,
    }

    public enum BuilderSynthPlaybackType
    {
        [Browsable(false)]
        Normal = -1,
        Polyphonic = 0,
        RandomNoRepeat = 1,
        Sequential = 2,
        Random = 3,
        SequentialNoLoop = 6,
    }

    public class BuilderSynthNode : BuilderBaseNode
    {
        private BuilderSynthPlaybackType playbackType = BuilderSynthPlaybackType.Polyphonic;
        private Random random = new Random();
        private int previousChild = -1;
        private int nextChild = -1;
        private byte playbackProbability = 100;

        [Browsable(false)]
        public BuilderSynthType Type { get; set; }

        [Category("General"), DisplayName("Playback Type")]
        [Description("Playback type of this synth.")]
        public BuilderSynthPlaybackType PlaybackType
        {
            get
            {
                if (Type == BuilderSynthType.Single)
                {
                    return BuilderSynthPlaybackType.Normal;
                }

                return playbackType;
            }

            set
            {
                if (Type == BuilderSynthType.Single || value == BuilderSynthPlaybackType.Normal)
                {
                    return;
                }

                playbackType = value;
            }
        }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sound Element Reference Path")]
        [Description("Full reference path of sound element node. This is going to be empty if the node is a track.")]
        public string SoundElementReference { get; set; }

        [Browsable(false)]
        public List<string> Children { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Aisac Reference Path")]
        [Description("Full reference path of aisac node.")]
        public string AisacReference { get; set; }

        [Category("General")]
        [Description("Volume of this synth. 1000 equals to 100%.")]
        public short Volume { get; set; }

        [Category("General")]
        [Description("Pitch of this synth. Use positive/negative values to make it higher/lower pitched.")]
        public short Pitch { get; set; }

        [Category("General"), DisplayName("Delay Time")]
        [Description("How much time it takes to play this synth. The time is in milliseconds.")]
        public uint DelayTime { get; set; }

        [Category("Unknown"), DisplayName("S Control")]
        public byte SControl { get; set; }

        [Category("EG"), DisplayName("EG Delay")]
        public ushort EgDelay { get; set; }

        [Category("EG"), DisplayName("EG Attack")]
        public ushort EgAttack { get; set; }

        [Category("EG"), DisplayName("EG Hold")]
        public ushort EgHold { get; set; }

        [Category("EG"), DisplayName("EG Decay")]
        public ushort EgDecay { get; set; }

        [Category("EG"), DisplayName("EG Release")]
        public ushort EgRelease { get; set; }

        [Category("EG"), DisplayName("EG Sustain")]
        public ushort EgSustain { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Type")]
        public byte FilterType { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Cutoff 1")]
        public ushort FilterCutoff1 { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Cutoff 2")]
        public ushort FilterCutoff2 { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter RESO (Unknown)")]
        public ushort FilterReso { get; set; }

        [Category("Filter (Unknown)"), DisplayName("Filter Release Offset")]
        public byte FilterReleaseOffset { get; set; }

        [Category("Dryness"), DisplayName("Dry O Name (Unknown)")]
        public string DryOName { get; set; }

        [Category("Unknown")]
        public string Mtxrtr { get; set; }

        [Category("Dryness"), DisplayName("Dry 0")]
        public ushort Dry0 { get; set; }

        [Category("Dryness"), DisplayName("Dry 1")]
        public ushort Dry1 { get; set; }

        [Category("Dryness"), DisplayName("Dry 2")]
        public ushort Dry2 { get; set; }

        [Category("Dryness"), DisplayName("Dry 3")]
        public ushort Dry3 { get; set; }

        [Category("Dryness"), DisplayName("Dry 4")]
        public ushort Dry4 { get; set; }

        [Category("Dryness"), DisplayName("Dry 5")]
        public ushort Dry5 { get; set; }

        [Category("Dryness"), DisplayName("Dry 6")]
        public ushort Dry6 { get; set; }

        [Category("Dryness"), DisplayName("Dry 7")]
        public ushort Dry7 { get; set; }

        [Category("Wetness"), DisplayName("Wet O Name (Unknown)")]
        public string WetOName { get; set; }

        [Category("Wetness"), DisplayName("Wet 0")]
        public ushort Wet0 { get; set; }

        [Category("Wetness"), DisplayName("Wet 1")]
        public ushort Wet1 { get; set; }

        [Category("Wetness"), DisplayName("Wet 2")]
        public ushort Wet2 { get; set; }

        [Category("Wetness"), DisplayName("Wet 3")]
        public ushort Wet3 { get; set; }

        [Category("Wetness"), DisplayName("Wet 4")]
        public ushort Wet4 { get; set; }

        [Category("Wetness"), DisplayName("Wet 5")]
        public ushort Wet5 { get; set; }

        [Category("Wetness"), DisplayName("Wet 6")]
        public ushort Wet6 { get; set; }

        [Category("Wetness"), DisplayName("Wet 7")]
        public ushort Wet7 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 0")]
        public string Wcnct0 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 1")]
        public string Wcnct1 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 2")]
        public string Wcnct2 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 3")]
        public string Wcnct3 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 4")]
        public string Wcnct4 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 5")]
        public string Wcnct5 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 6")]
        public string Wcnct6 { get; set; }

        [Category("Wcnct (Unknown)"), DisplayName("Wcnct 7")]
        public string Wcnct7 { get; set; }

        [ReadOnly(true)]
        [Category("Voice Limit"), DisplayName("Voice Limit Group Reference")]
        public string VoiceLimitGroupReference { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Type")]
        public byte VoiceLimitType { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Priority")]
        public byte VoiceLimitPriority { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Prohibition Time")]
        public ushort VoiceLimitProhibitionTime { get; set; }

        [Category("Voice Limit"), DisplayName("Voice Limit Pcdlt (Unknown)")]
        public sbyte VoiceLimitPcdlt { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Offset")]
        public short Pan3dVolumeOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Volume Gain")]
        public short Pan3dVolumeGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Offset")]
        public short Pan3dAngleOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Angle Gain")]
        public short Pan3dAngleGain { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Offset")]
        public short Pan3dDistanceOffset { get; set; }

        [Category("Pan 3D"), DisplayName("Pan 3D Distance Gain")]
        public short Pan3dDistanceGain { get; set; }

        [Category("Dryness"), DisplayName("Dry 0 Gain")]
        public byte Dry0g { get; set; }

        [Category("Dryness"), DisplayName("Dry 1 Gain")]
        public byte Dry1g { get; set; }

        [Category("Dryness"), DisplayName("Dry 2 Gain")]
        public byte Dry2g { get; set; }

        [Category("Dryness"), DisplayName("Dry 3 Gain")]
        public byte Dry3g { get; set; }

        [Category("Dryness"), DisplayName("Dry 4 Gain")]
        public byte Dry4g { get; set; }

        [Category("Dryness"), DisplayName("Dry 5 Gain")]
        public byte Dry5g { get; set; }

        [Category("Dryness"), DisplayName("Dry 6 Gain")]
        public byte Dry6g { get; set; }

        [Category("Dryness"), DisplayName("Dry 7 Gain")]
        public byte Dry7g { get; set; }

        [Category("Wetness"), DisplayName("Wet 0 Gain")]
        public byte Wet0g { get; set; }

        [Category("Wetness"), DisplayName("Wet 1 Gain")]
        public byte Wet1g { get; set; }

        [Category("Wetness"), DisplayName("Wet 2 Gain")]
        public byte Wet2g { get; set; }

        [Category("Wetness"), DisplayName("Wet 3 Gain")]
        public byte Wet3g { get; set; }

        [Category("Wetness"), DisplayName("Wet 4 Gain")]
        public byte Wet4g { get; set; }

        [Category("Wetness"), DisplayName("Wet 5 Gain")]
        public byte Wet5g { get; set; }

        [Category("Wetness"), DisplayName("Wet 6 Gain")]
        public byte Wet6g { get; set; }

        [Category("Wetness"), DisplayName("Wet 7 Gain")]
        public byte Wet7g { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Type")]
        public byte Filter1Type { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Cutoff Offset")]
        public ushort Filter1CutoffOffset { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 Cutoff Gain")]
        public ushort Filter1CutoffGain { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 RESO (Unknown) Offset")]
        public ushort Filter1ResoOffset { get; set; }

        [Category("Filter 1 (Unknown)"), DisplayName("Filter 1 RESO (Unknown) Gain")]
        public ushort Filter1ResoGain { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Type")]
        public byte Filter2Type { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Lower Offset")]
        public ushort Filter2CutoffLowerOffset { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Lower Gain")]
        public ushort Filter2CutoffLowerGain { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Higher Offset")]
        public ushort Filter2CutoffHigherOffset { get; set; }

        [Category("Filter 2 (Unknown)"), DisplayName("Filter 2 Cutoff Higher Gain")]
        public ushort Filter2CutoffHigherGain { get; set; }

        [Category("General"), DisplayName("Playback Probability")]
        [Description("Probability of this synth being played. Lower values make it less probable to play. Max is 100.")]
        public byte PlaybackProbability
        {
            get
            {
                if (Type == BuilderSynthType.WithChildren)
                {
                    return 0;
                }

                return playbackProbability;
            }

            set
            {
                playbackProbability = value > 100 ? (byte)100 : value;
            }
        }

        [Category("Unknown"), DisplayName("N LMT Children")]
        public byte NLmtChildren { get; set; }

        [Category("General"), DisplayName("Repeat")]
        public byte Repeat { get; set; }

        [Category("General"), DisplayName("Combo Time")]
        public uint ComboTime { get; set; }

        [Category("General"), DisplayName("Combo Loop Back")]
        public byte ComboLoopBack { get; set; }

        [Browsable(false)]
        public bool PlayThisTurn
        {
            get
            {
                return random.Next(100) <= PlaybackProbability;
            }
        }
		
		[Browsable(false)]
        public int RandomChildNode
        {
            get
            {
                if (playbackType == BuilderSynthPlaybackType.RandomNoRepeat)
                {
                    int randomChild = random.Next(Children.Count);

                    while (randomChild == previousChild)
                    {
                        randomChild = random.Next(Children.Count);
                    }

                    previousChild = randomChild;
                    return randomChild;
                }

                return random.Next(Children.Count);
            }
        }

        [Browsable(false)]
        public int NextChildNode
        {
            get
            {
                if (nextChild + 1 == Children.Count)
                {
                    nextChild = -1;
                }

                return ++nextChild;
            }
        }

        public BuilderSynthNode()
        {
            Children = new List<string>();

            Volume = 1000;

            EgSustain = 1000;

            Dry0g = 255;
            Dry1g = 255;
            Dry2g = 255;
            Dry3g = 255;
            Dry4g = 255;
            Dry5g = 255;
            Dry6g = 255;
            Dry7g = 255;

            Wet0g = 255;
            Wet1g = 255;
            Wet2g = 255;
            Wet3g = 255;
            Wet4g = 255;
            Wet5g = 255;
            Wet6g = 255;
            Wet7g = 255;

            Pan3dAngleGain = 1000;
            Pan3dDistanceGain = 1000;
            Pan3dDistanceOffset = 1000;
            Pan3dVolumeGain = 1000;
            Pan3dVolumeOffset = 1000;

            Filter2CutoffHigherGain = 1000;
            Filter2CutoffHigherOffset = 1000;
            Filter2CutoffLowerGain = 1000;
        }
    }
}
