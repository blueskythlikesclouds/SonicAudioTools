using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLCUE")]
    public class SerializationCueTable
    {
        private string nameField = string.Empty;
        private uint idField = 0;
        private string synthField = string.Empty;
        private string udataField = string.Empty;
        private byte flagsField = 0;

        [CriField("name", 0)]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        [CriField("id", 1)]
        public uint Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        [CriField("synth", 2)]
        public string SynthPath
        {
            get
            {
                return synthField;
            }
            set
            {
                synthField = value;
            }
        }

        [CriField("udata", 3)]
        public string UserData
        {
            get
            {
                return udataField;
            }
            set
            {
                udataField = value;
            }
        }

        [CriField("flags", 4)]
        public byte Flags
        {
            get
            {
                return flagsField;
            }
            set
            {
                flagsField = value;
            }
        }
    }
}
