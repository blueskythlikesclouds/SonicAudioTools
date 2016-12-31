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
        public enum EnumFormat : byte
        {
            Adx = 0,
            Dsp = 4,
        };

        private string _name = string.Empty;
        private EnumFormat _fmt = EnumFormat.Adx;
        private byte _nch = 0;
        private bool _stmflg = false;
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
                if (value.Length > 0)
                {
                    DataList = CriTableSerializer.Deserialize<CriTableAax>(value);
                }
            }
        }

        [CriField("fmt", 2)]
        public EnumFormat Format
        {
            get
            {
                return _fmt;
            }
            set
            {
                _fmt = value;
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
        public bool Streaming
        {
            get
            {
                return _stmflg;
            }
            set
            {
                _stmflg = value;
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
