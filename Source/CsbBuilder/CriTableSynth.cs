using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Linq;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLSYN")]
    public class CriTableSynth
    {
        public enum EnumSynthType : byte
        {
            Waveform = 0,
            Polyphonic = 1,
            Random = 3,
        };

        private string _synname = string.Empty;
        private EnumSynthType _syntype = EnumSynthType.Waveform;
        private EnumSynthType _cmplxtype = EnumSynthType.Waveform;
        private string _lnkname = string.Empty;
        private string _issetname = string.Empty;
        private short _volume = 1000;
        private short _pitch = 0;
        private uint _dlytim = 0;
        private byte _s_cntrl = 0;
        private ushort _eg_dly = 0;
        private ushort _eg_atk = 0;
        private ushort _eg_hld = 0;
        private ushort _eg_dcy = 0;
        private ushort _eg_rel = 0;
        private ushort _eg_sus = 1000;
        private byte _f_type = 0;
        private ushort _f_cof1 = 0;
        private ushort _f_cof2 = 0;
        private ushort _f_reso = 0;
        private byte _f_roff = 0;
        private string _dryoname = string.Empty;
        private string _mtxrtr = string.Empty;
        private ushort _dry0 = 0;
        private ushort _dry1 = 0;
        private ushort _dry2 = 0;
        private ushort _dry3 = 0;
        private ushort _dry4 = 0;
        private ushort _dry5 = 0;
        private ushort _dry6 = 0;
        private ushort _dry7 = 0;
        private string _wetoname = string.Empty;
        private ushort _wet0 = 0;
        private ushort _wet1 = 0;
        private ushort _wet2 = 0;
        private ushort _wet3 = 0;
        private ushort _wet4 = 0;
        private ushort _wet5 = 0;
        private ushort _wet6 = 0;
        private ushort _wet7 = 0;
        private string _wcnct0 = string.Empty;
        private string _wcnct1 = string.Empty;
        private string _wcnct2 = string.Empty;
        private string _wcnct3 = string.Empty;
        private string _wcnct4 = string.Empty;
        private string _wcnct5 = string.Empty;
        private string _wcnct6 = string.Empty;
        private string _wcnct7 = string.Empty;
        private string _vl_gname = string.Empty;
        private byte _vl_type = 0;
        private byte _vl_prio = 0;
        private ushort _vl_phtime = 0;
        private sbyte _vl_pcdlt = 0;
        private short _p3d_vo = 0;
        private short _p3d_vg = 1000;
        private short _p3d_ao = 0;
        private short _p3d_ag = 1000;
        private short _p3d_ido = 0;
        private short _p3d_idg = 1000;
        private byte _dry0g = 255;
        private byte _dry1g = 255;
        private byte _dry2g = 255;
        private byte _dry3g = 255;
        private byte _dry4g = 255;
        private byte _dry5g = 255;
        private byte _dry6g = 255;
        private byte _dry7g = 255;
        private byte _wet0g = 255;
        private byte _wet1g = 255;
        private byte _wet2g = 255;
        private byte _wet3g = 255;
        private byte _wet4g = 255;
        private byte _wet5g = 255;
        private byte _wet6g = 255;
        private byte _wet7g = 255;
        private byte _f1_type = 0;
        private ushort _f1_cofo = 0;
        private ushort _f1_cofg = 0;
        private ushort _f1_resoo = 0;
        private ushort _f1_resog = 0;
        private byte _f2_type = 0;
        private ushort _f2_coflo = 0;
        private ushort _f2_coflg = 1000;
        private ushort _f2_cofho = 0;
        private ushort _f2_cofhg = 1000;
        private byte _probability = 100;
        private byte _n_lmt_children = 0;
        private byte _repeat = 0;
        private uint _combo_time = 0;
        private byte _combo_loop_back = 0;

        [CriField("synname", 0)]
        public string SynthName
        {
            get
            {
                return _synname;
            }
            set
            {
                _synname = value;
            }
        }

        [CriField("syntype", 1)]
        public EnumSynthType SynthType
        {
            get
            {
                return _syntype;
            }
            set
            {
                _syntype = value;
            }
        }

        [CriField("cmplxtype", 2)]
        public EnumSynthType ComplexType
        {
            get
            {
                return _cmplxtype;
            }
            set
            {
                _cmplxtype = value;
            }
        }

        [CriIgnore]
        public List<string> LinkNameList { get; set; }

        [CriIgnore]
        public List<string> AisacSetNameList { get; set; }

        [XmlIgnore]
        [CriField("lnkname", 3)]
        public string LinkName
        {
            get
            {
                if (_syntype == EnumSynthType.Waveform && LinkNameList.Count > 0)
                {
                    return LinkNameList[0];
                }

                string result = string.Empty;
                foreach (string linkName in LinkNameList)
                {
                    result += linkName + (char)0x0A;
                }

                return result;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    LinkNameList = value.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
        }

        [XmlIgnore]
        [CriField("issetname", 4)]
        public string AisacSetName
        {
            get
            {
                string result = string.Empty;
                foreach (string aisacSetName in AisacSetNameList)
                {
                    result += aisacSetName + (char)0x0A;
                }

                return result;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    AisacSetNameList = value.Split(new char[] { (char)0x0A }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
        }

        /*[CriField("lnkname", 3)]
        public string LinkName
        {
            get
            {
                return _lnkname;
            }
            set
            {
                _lnkname = value;
            }
        }

        [CriField("issetname", 4)]
        public string AisacSetName
        {
            get
            {
                return _issetname;
            }
            set
            {
                _issetname = value;
            }
        }*/

        [CriField("volume", 5)]
        public short Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
            }
        }

        [CriField("pitch", 6)]
        public short Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                _pitch = value;
            }
        }

        [CriField("dlytim", 7)]
        public uint DelayTime
        {
            get
            {
                return _dlytim;
            }
            set
            {
                _dlytim = value;
            }
        }

        [CriField("s_cntrl", 8)]
        public byte SoundControl
        {
            get
            {
                return _s_cntrl;
            }
            set
            {
                _s_cntrl = value;
            }
        }

        [CriField("eg_dly", 9)]
        public ushort EgDelay
        {
            get
            {
                return _eg_dly;
            }
            set
            {
                _eg_dly = value;
            }
        }

        [CriField("eg_atk", 10)]
        public ushort EgAttack
        {
            get
            {
                return _eg_atk;
            }
            set
            {
                _eg_atk = value;
            }
        }

        [CriField("eg_hld", 11)]
        public ushort EgHold
        {
            get
            {
                return _eg_hld;
            }
            set
            {
                _eg_hld = value;
            }
        }

        [CriField("eg_dcy", 12)]
        public ushort EgDecay
        {
            get
            {
                return _eg_dcy;
            }
            set
            {
                _eg_dcy = value;
            }
        }

        [CriField("eg_rel", 13)]
        public ushort EgRelease
        {
            get
            {
                return _eg_rel;
            }
            set
            {
                _eg_rel = value;
            }
        }

        [CriField("eg_sus", 14)]
        public ushort EgSustain
        {
            get
            {
                return _eg_sus;
            }
            set
            {
                _eg_sus = value;
            }
        }

        [CriField("f_type", 15)]
        public byte FType
        {
            get
            {
                return _f_type;
            }
            set
            {
                _f_type = value;
            }
        }

        [CriField("f_cof1", 16)]
        public ushort FCof1
        {
            get
            {
                return _f_cof1;
            }
            set
            {
                _f_cof1 = value;
            }
        }

        [CriField("f_cof2", 17)]
        public ushort FCof2
        {
            get
            {
                return _f_cof2;
            }
            set
            {
                _f_cof2 = value;
            }
        }

        [CriField("f_reso", 18)]
        public ushort FReso
        {
            get
            {
                return _f_reso;
            }
            set
            {
                _f_reso = value;
            }
        }

        [CriField("f_roff", 19)]
        public byte FReleaseOffset
        {
            get
            {
                return _f_roff;
            }
            set
            {
                _f_roff = value;
            }
        }

        [CriField("dryoname", 20)]
        public string DryOName
        {
            get
            {
                return _dryoname;
            }
            set
            {
                _dryoname = value;
            }
        }

        [CriField("mtxrtr", 21)]
        public string Mtxrtr
        {
            get
            {
                return _mtxrtr;
            }
            set
            {
                _mtxrtr = value;
            }
        }

        [CriField("dry0", 22)]
        public ushort Dry0
        {
            get
            {
                return _dry0;
            }
            set
            {
                _dry0 = value;
            }
        }

        [CriField("dry1", 23)]
        public ushort Dry1
        {
            get
            {
                return _dry1;
            }
            set
            {
                _dry1 = value;
            }
        }

        [CriField("dry2", 24)]
        public ushort Dry2
        {
            get
            {
                return _dry2;
            }
            set
            {
                _dry2 = value;
            }
        }

        [CriField("dry3", 25)]
        public ushort Dry3
        {
            get
            {
                return _dry3;
            }
            set
            {
                _dry3 = value;
            }
        }

        [CriField("dry4", 26)]
        public ushort Dry4
        {
            get
            {
                return _dry4;
            }
            set
            {
                _dry4 = value;
            }
        }

        [CriField("dry5", 27)]
        public ushort Dry5
        {
            get
            {
                return _dry5;
            }
            set
            {
                _dry5 = value;
            }
        }

        [CriField("dry6", 28)]
        public ushort Dry6
        {
            get
            {
                return _dry6;
            }
            set
            {
                _dry6 = value;
            }
        }

        [CriField("dry7", 29)]
        public ushort Dry7
        {
            get
            {
                return _dry7;
            }
            set
            {
                _dry7 = value;
            }
        }

        [CriField("wetoname", 30)]
        public string WetOName
        {
            get
            {
                return _wetoname;
            }
            set
            {
                _wetoname = value;
            }
        }

        [CriField("wet0", 31)]
        public ushort Wet0
        {
            get
            {
                return _wet0;
            }
            set
            {
                _wet0 = value;
            }
        }

        [CriField("wet1", 32)]
        public ushort Wet1
        {
            get
            {
                return _wet1;
            }
            set
            {
                _wet1 = value;
            }
        }

        [CriField("wet2", 33)]
        public ushort Wet2
        {
            get
            {
                return _wet2;
            }
            set
            {
                _wet2 = value;
            }
        }

        [CriField("wet3", 34)]
        public ushort Wet3
        {
            get
            {
                return _wet3;
            }
            set
            {
                _wet3 = value;
            }
        }

        [CriField("wet4", 35)]
        public ushort Wet4
        {
            get
            {
                return _wet4;
            }
            set
            {
                _wet4 = value;
            }
        }

        [CriField("wet5", 36)]
        public ushort Wet5
        {
            get
            {
                return _wet5;
            }
            set
            {
                _wet5 = value;
            }
        }

        [CriField("wet6", 37)]
        public ushort Wet6
        {
            get
            {
                return _wet6;
            }
            set
            {
                _wet6 = value;
            }
        }

        [CriField("wet7", 38)]
        public ushort Wet7
        {
            get
            {
                return _wet7;
            }
            set
            {
                _wet7 = value;
            }
        }

        [CriField("wcnct0", 39)]
        public string Wcnct0
        {
            get
            {
                return _wcnct0;
            }
            set
            {
                _wcnct0 = value;
            }
        }

        [CriField("wcnct1", 40)]
        public string Wcnct1
        {
            get
            {
                return _wcnct1;
            }
            set
            {
                _wcnct1 = value;
            }
        }

        [CriField("wcnct2", 41)]
        public string Wcnct2
        {
            get
            {
                return _wcnct2;
            }
            set
            {
                _wcnct2 = value;
            }
        }

        [CriField("wcnct3", 42)]
        public string Wcnct3
        {
            get
            {
                return _wcnct3;
            }
            set
            {
                _wcnct3 = value;
            }
        }

        [CriField("wcnct4", 43)]
        public string Wcnct4
        {
            get
            {
                return _wcnct4;
            }
            set
            {
                _wcnct4 = value;
            }
        }

        [CriField("wcnct5", 44)]
        public string Wcnct5
        {
            get
            {
                return _wcnct5;
            }
            set
            {
                _wcnct5 = value;
            }
        }

        [CriField("wcnct6", 45)]
        public string Wcnct6
        {
            get
            {
                return _wcnct6;
            }
            set
            {
                _wcnct6 = value;
            }
        }

        [CriField("wcnct7", 46)]
        public string Wcnct7
        {
            get
            {
                return _wcnct7;
            }
            set
            {
                _wcnct7 = value;
            }
        }

        [CriField("vl_gname", 47)]
        public string VoiceLimitGroupName
        {
            get
            {
                return _vl_gname;
            }
            set
            {
                _vl_gname = value;
            }
        }

        [CriField("vl_type", 48)]
        public byte VoiceLimitType
        {
            get
            {
                return _vl_type;
            }
            set
            {
                _vl_type = value;
            }
        }

        [CriField("vl_prio", 49)]
        public byte VoiceLimitPriority
        {
            get
            {
                return _vl_prio;
            }
            set
            {
                _vl_prio = value;
            }
        }

        [CriField("vl_phtime", 50)]
        public ushort VoiceLimitPhTime
        {
            get
            {
                return _vl_phtime;
            }
            set
            {
                _vl_phtime = value;
            }
        }

        [CriField("vl_pcdlt", 51)]
        public sbyte VoiceLimitPcdlt
        {
            get
            {
                return _vl_pcdlt;
            }
            set
            {
                _vl_pcdlt = value;
            }
        }

        [CriField("p3d_vo", 52)]
        public short Pan3dVolumeOffset
        {
            get
            {
                return _p3d_vo;
            }
            set
            {
                _p3d_vo = value;
            }
        }

        [CriField("p3d_vg", 53)]
        public short Pan3dVolumeGain
        {
            get
            {
                return _p3d_vg;
            }
            set
            {
                _p3d_vg = value;
            }
        }

        [CriField("p3d_ao", 54)]
        public short Pan3dAngleOffset
        {
            get
            {
                return _p3d_ao;
            }
            set
            {
                _p3d_ao = value;
            }
        }

        [CriField("p3d_ag", 55)]
        public short Pan3dAngleGain
        {
            get
            {
                return _p3d_ag;
            }
            set
            {
                _p3d_ag = value;
            }
        }

        [CriField("p3d_ido", 56)]
        public short Pan3dDistanceOffset
        {
            get
            {
                return _p3d_ido;
            }
            set
            {
                _p3d_ido = value;
            }
        }

        [CriField("p3d_idg", 57)]
        public short Pan3dDistanceGain
        {
            get
            {
                return _p3d_idg;
            }
            set
            {
                _p3d_idg = value;
            }
        }

        [CriField("dry0g", 58)]
        public byte Dry0Gain
        {
            get
            {
                return _dry0g;
            }
            set
            {
                _dry0g = value;
            }
        }

        [CriField("dry1g", 59)]
        public byte Dry1Gain
        {
            get
            {
                return _dry1g;
            }
            set
            {
                _dry1g = value;
            }
        }

        [CriField("dry2g", 60)]
        public byte Dry2Gain
        {
            get
            {
                return _dry2g;
            }
            set
            {
                _dry2g = value;
            }
        }

        [CriField("dry3g", 61)]
        public byte Dry3Gain
        {
            get
            {
                return _dry3g;
            }
            set
            {
                _dry3g = value;
            }
        }

        [CriField("dry4g", 62)]
        public byte Dry4Gain
        {
            get
            {
                return _dry4g;
            }
            set
            {
                _dry4g = value;
            }
        }

        [CriField("dry5g", 63)]
        public byte Dry5Gain
        {
            get
            {
                return _dry5g;
            }
            set
            {
                _dry5g = value;
            }
        }

        [CriField("dry6g", 64)]
        public byte Dry6Gain
        {
            get
            {
                return _dry6g;
            }
            set
            {
                _dry6g = value;
            }
        }

        [CriField("dry7g", 65)]
        public byte Dry7Gain
        {
            get
            {
                return _dry7g;
            }
            set
            {
                _dry7g = value;
            }
        }

        [CriField("wet0g", 66)]
        public byte Wet0Gain
        {
            get
            {
                return _wet0g;
            }
            set
            {
                _wet0g = value;
            }
        }

        [CriField("wet1g", 67)]
        public byte Wet1Gain
        {
            get
            {
                return _wet1g;
            }
            set
            {
                _wet1g = value;
            }
        }

        [CriField("wet2g", 68)]
        public byte Wet2Gain
        {
            get
            {
                return _wet2g;
            }
            set
            {
                _wet2g = value;
            }
        }

        [CriField("wet3g", 69)]
        public byte Wet3Gain
        {
            get
            {
                return _wet3g;
            }
            set
            {
                _wet3g = value;
            }
        }

        [CriField("wet4g", 70)]
        public byte Wet4Gain
        {
            get
            {
                return _wet4g;
            }
            set
            {
                _wet4g = value;
            }
        }

        [CriField("wet5g", 71)]
        public byte Wet5Gain
        {
            get
            {
                return _wet5g;
            }
            set
            {
                _wet5g = value;
            }
        }

        [CriField("wet6g", 72)]
        public byte Wet6Gain
        {
            get
            {
                return _wet6g;
            }
            set
            {
                _wet6g = value;
            }
        }

        [CriField("wet7g", 73)]
        public byte Wet7Gain
        {
            get
            {
                return _wet7g;
            }
            set
            {
                _wet7g = value;
            }
        }

        [CriField("f1_type", 74)]
        public byte F1Type
        {
            get
            {
                return _f1_type;
            }
            set
            {
                _f1_type = value;
            }
        }

        [CriField("f1_cofo", 75)]
        public ushort F1CofOffset
        {
            get
            {
                return _f1_cofo;
            }
            set
            {
                _f1_cofo = value;
            }
        }

        [CriField("f1_cofg", 76)]
        public ushort F1CofGain
        {
            get
            {
                return _f1_cofg;
            }
            set
            {
                _f1_cofg = value;
            }
        }

        [CriField("f1_resoo", 77)]
        public ushort F1ResoOffset
        {
            get
            {
                return _f1_resoo;
            }
            set
            {
                _f1_resoo = value;
            }
        }

        [CriField("f1_resog", 78)]
        public ushort F1ResoGain
        {
            get
            {
                return _f1_resog;
            }
            set
            {
                _f1_resog = value;
            }
        }

        [CriField("f2_type", 79)]
        public byte F2Type
        {
            get
            {
                return _f2_type;
            }
            set
            {
                _f2_type = value;
            }
        }

        [CriField("f2_coflo", 80)]
        public ushort F2CofLowOffset
        {
            get
            {
                return _f2_coflo;
            }
            set
            {
                _f2_coflo = value;
            }
        }

        [CriField("f2_coflg", 81)]
        public ushort F2CofLowGain
        {
            get
            {
                return _f2_coflg;
            }
            set
            {
                _f2_coflg = value;
            }
        }

        [CriField("f2_cofho", 82)]
        public ushort F2CofHighOffset
        {
            get
            {
                return _f2_cofho;
            }
            set
            {
                _f2_cofho = value;
            }
        }

        [CriField("f2_cofhg", 83)]
        public ushort F2CofHighGain
        {
            get
            {
                return _f2_cofhg;
            }
            set
            {
                _f2_cofhg = value;
            }
        }

        [CriField("probability", 84)]
        public byte Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                _probability = value;
            }
        }

        [CriField("n_lmt_children", 85)]
        public byte NumberLmtChildren
        {
            get
            {
                return _n_lmt_children;
            }
            set
            {
                _n_lmt_children = value;
            }
        }

        [CriField("repeat", 86)]
        public byte Repeat
        {
            get
            {
                return _repeat;
            }
            set
            {
                _repeat = value;
            }
        }

        [CriField("combo_time", 87)]
        public uint ComboTime
        {
            get
            {
                return _combo_time;
            }
            set
            {
                _combo_time = value;
            }
        }

        [CriField("combo_loop_back", 88)]
        public byte ComboLoopBack
        {
            get
            {
                return _combo_loop_back;
            }
            set
            {
                _combo_loop_back = value;
            }
        }
    }
}
