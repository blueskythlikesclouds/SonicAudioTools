using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLCUE")]
    public class CriTableCue
    {
        private string _name;
        private uint _id;
        private string _synth;
        private string _udata;
        private byte _flags;

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

        [CriField("id", 1)]
        public uint Index
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        [CriField("synth", 2)]
        public string Synth
        {
            get
            {
                return _synth;
            }
            set
            {
                _synth = value;
            }
        }

        [CriField("udata", 3)]
        public string UserData
        {
            get
            {
                return _udata;
            }
            set
            {
                _udata = value;
            }
        }

        [CriField("flags", 4)]
        public byte Flags
        {
            get
            {
                return _flags;
            }
            set
            {
                _flags = value;
            }
        }
    }
}
