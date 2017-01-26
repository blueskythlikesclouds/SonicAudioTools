using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLCSB")]
    public class SerializationCueSheetTable
    {
        private string nameField = string.Empty;
        private byte ttypeField = 0;
        private byte[] utfField = null;

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

        [CriField("ttype", 1)]
        public byte TableType
        {
            get
            {
                return ttypeField;
            }
            set
            {
                ttypeField = value;
            }
        }

        [CriField("utf", 2)]
        public byte[] TableData
        {
            get
            {
                return utfField;
            }
            set
            {
                utfField = value;
            }
        }
    }
}
