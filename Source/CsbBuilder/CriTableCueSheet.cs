using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;
using System.Linq;

using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLCSB")]
    public class CriTableCueSheet
    {
        public enum EnumTableType
        {
            None = 0,
            Cue = 1,
            Synth = 2,
            SoundElement = 4,
            Aisac = 5,
            VoiceLimitGroup = 6,
            VersionInfo = 7,
        };

        private string _name = string.Empty;
        private EnumTableType _ttype = EnumTableType.None;

        [CriField("name", 0)]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [CriField("ttype", 1)]
        public byte TableType
        {
            get
            {
                return (byte)_ttype;
            }
            set
            {
                _ttype = (EnumTableType)value;
            }
        }

        [CriIgnore]
        [XmlArray]
        [XmlArrayItem(typeof(CriTableCue))]
        [XmlArrayItem(typeof(CriTableSynth))]
        [XmlArrayItem(typeof(CriTableSoundElement))]
        [XmlArrayItem(typeof(CriTableAisac))]
        [XmlArrayItem(typeof(CriTableVoiceLimitGroup))]
        [XmlArrayItem(typeof(CriTableVersionInfo))]
        public ArrayList DataList { get; set; } 

        [XmlIgnore]
        [CriField("utf", 2)]
        public byte[] Data
        {
            get
            {
                switch (_ttype)
                {
                    case EnumTableType.None:
                        return new byte[0];

                    case EnumTableType.Cue:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableCue>().ToList(), CriTableWriterSettings.AdxSettings);

                    case EnumTableType.Synth:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableSynth>().ToList(), CriTableWriterSettings.AdxSettings);

                    case EnumTableType.SoundElement:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableSoundElement>().ToList(), CriTableWriterSettings.AdxSettings);

                    case EnumTableType.Aisac:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableAisac>().ToList(), CriTableWriterSettings.AdxSettings);

                    case EnumTableType.VoiceLimitGroup:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableVoiceLimitGroup>().ToList(), CriTableWriterSettings.AdxSettings);

                    case EnumTableType.VersionInfo:
                        return CriTableSerializer.Serialize(DataList.OfType<CriTableVersionInfo>().ToList(), CriTableWriterSettings.AdxSettings);
                }

                throw new ArgumentException($"Unknown table type {_ttype}, please report the error with the file.", "_ttype");
            }

            set
            {
                switch (_ttype)
                {
                    case EnumTableType.Cue:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableCue));
                        break;

                    case EnumTableType.Synth:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableSynth));
                        break;

                    case EnumTableType.SoundElement:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableSoundElement));
                        break;

                    case EnumTableType.Aisac:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableAisac));
                        break;

                    case EnumTableType.VoiceLimitGroup:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableVoiceLimitGroup));
                        break;

                    case EnumTableType.VersionInfo:
                        DataList = CriTableSerializer.Deserialize(value, typeof(CriTableVersionInfo));
                        break;

                    default:
                        throw new ArgumentException($"Unknown table type {_ttype}, please report the error with the file.", "_ttype");
                }
            }
        }
    }
}
