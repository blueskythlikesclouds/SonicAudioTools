using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
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

        private string _name;
        private FileInfo _data;
        private EnumFormat _fmt = 0;
        private byte _nch;
        private EnumStreamFlag _stmflg = 0;
        private uint _sfreq;
        private uint _nsmpl;

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

        [CriField("data", 1)]
        public FileInfo Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
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
