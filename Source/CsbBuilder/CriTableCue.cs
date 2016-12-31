using System.IO;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLCUE")]
    public class CriTableCue
    {
        private string _name = string.Empty;
        private uint _id = 0;
        private string _synth = string.Empty;
        private string _udata = string.Empty;
        private byte _flags = 0;

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
        public uint Id
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
