using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBL_INFO")]
    public class SerializationVersionInfoTable
    {
        private uint DataFmtVerField = 0x940000;
        private uint ExtSizeField = 0;

        [CriField("DataFmtVer", 0)]
        public uint DataFormatVersion
        {
            get
            {
                return DataFmtVerField;
            }
            set
            {
                DataFmtVerField = value;
            }
        }

        [CriField("ExtSize", 1)]
        public uint ExtensionSize
        {
            get
            {
                return ExtSizeField;
            }
            set
            {
                ExtSizeField = value;
            }
        }
    }
}
