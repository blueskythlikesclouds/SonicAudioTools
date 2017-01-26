using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLSDL")]
    public class SerializationSoundElementTable
    {
        private string nameField = string.Empty;
        private byte[] dataField = null;
        private byte fmtField = 0;
        private byte nchField = 0;
        private bool stmflgField = false;
        private uint sfreqField = 0;
        private uint nsmplField = 0;

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

        [CriField("data", 1)]
        public byte[] Data
        {
            get
            {
                return dataField;
            }
            set
            {
                dataField = value;
            }
        }

        [CriField("fmt", 2)]
        public byte FormatType
        {
            get
            {
                return fmtField;
            }
            set
            {
                fmtField = value;
            }
        }

        [CriField("nch", 3)]
        public byte NumberChannels
        {
            get
            {
                return nchField;
            }
            set
            {
                nchField = value;
            }
        }

        [CriField("stmflg", 4)]
        public bool Streaming
        {
            get
            {
                return stmflgField;
            }
            set
            {
                stmflgField = value;
            }
        }

        [CriField("sfreq", 5)]
        public uint SoundFrequency
        {
            get
            {
                return sfreqField;
            }
            set
            {
                sfreqField = value;
            }
        }

        [CriField("nsmpl", 6)]
        public uint NumberSamples
        {
            get
            {
                return nsmplField;
            }
            set
            {
                nsmplField = value;
            }
        }
    }
}
