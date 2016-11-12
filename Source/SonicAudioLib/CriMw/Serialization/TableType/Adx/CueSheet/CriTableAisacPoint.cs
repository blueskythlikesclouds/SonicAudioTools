using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLIPT")]
    public class CriTableAisacPoint
    {
        private ushort _in;
        private ushort _out;

        [CriField("in", 0)]
        public ushort In
        {
            get
            {
                return _in;
            }
            set
            {
                _in = value;
            }
        }

        [CriField("out", 1)]
        public ushort Out
        {
            get
            {
                return _out;
            }
            set
            {
                _out = value;
            }
        }
    }
}
