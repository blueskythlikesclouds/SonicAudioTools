using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLIPT")]
    public class SerializationAisacPointTable
    {
        private ushort inField = 0;
        private ushort outField = 0;

        [CriField("in", 0)]
        public ushort In
        {
            get
            {
                return inField;
            }
            set
            {
                inField = value;
            }
        }

        [CriField("out", 1)]
        public ushort Out
        {
            get
            {
                return outField;
            }
            set
            {
                outField = value;
            }
        }
    }
}
