using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLSDL")]
    public class CriTableSoundElement
    {
        public enum EnumFormat
        {
            Adx = 0,
            Dsp = 4,
        };

        public enum EnumStreamFlag
        {
            Internal = 0,
            External = 1,
        };

        private string _name = string.Empty;
        private EnumFormat _fmt = EnumFormat.Adx;
        private byte _nch = 0;
        private EnumStreamFlag _stmflg = EnumStreamFlag.Internal;
        private uint _sfreq = 0;
        private uint _nsmpl = 0;

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

        [CriIgnore]
        public List<CriTableAax> DataList { get; set; } 

        [XmlIgnore]
        [CriField("data", 1)]
        public byte[] Data
        {
            get
            {
                return CriTableSerializer.Serialize(DataList, CriTableWriterSettings.AdxSettings);
            }

            set
            {
                DataList = CriTableSerializer.Deserialize<CriTableAax>(value);
            }
        }

        [CriField("fmt", 2)]
        public byte Format
        {
            get
            {
                return (byte)_fmt;
            }
            set
            {
                _fmt = (EnumFormat)value;
            }
        }

        [CriField("nch", 3)]
        public byte NumberChannels
        {
            get
            {
                return _nch;
            }
            set
            {
                _nch = value;
            }
        }

        [CriField("stmflg", 4)]
        public byte StreamFlag
        {
            get
            {
                return (byte)_stmflg;
            }
            set
            {
                _stmflg = (EnumStreamFlag)value;
            }
        }

        [CriField("sfreq", 5)]
        public uint SoundFrequence
        {
            get
            {
                return _sfreq;
            }
            set
            {
                _sfreq = value;
            }
        }

        [CriField("nsmpl", 6)]
        public uint NumberSamples
        {
            get
            {
                return _nsmpl;
            }
            set
            {
                _nsmpl = value;
            }
        }
    }
}
