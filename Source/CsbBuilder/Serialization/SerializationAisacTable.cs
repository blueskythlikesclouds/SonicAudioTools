using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLISC")]
    public class SerializationAisacTable
    {
        private string nameField = string.Empty;
        private string ptnameField = string.Empty;
        private byte typeField = 0;
        private byte[] grphField = null;
        private byte rndrngField = 0;

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

        [CriField("ptname", 1)]
        public string PathName
        {
            get
            {
                return ptnameField;
            }
            set
            {
                ptnameField = value;
            }
        }

        [CriField("type", 2)]
        public byte Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }

        [CriField("grph", 3)]
        public byte[] Graph
        {
            get
            {
                return grphField;
            }
            set
            {
                grphField = value;
            }
        }

        [CriField("rndrng", 4)]
        public byte RandomRange
        {
            get
            {
                return rndrngField;
            }
            set
            {
                rndrngField = value;
            }
        }
    }
}
