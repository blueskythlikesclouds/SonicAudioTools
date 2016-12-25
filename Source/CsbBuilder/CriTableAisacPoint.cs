using System.IO;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLIPT")]
    public class CriTableAisacPoint
    {
        private ushort _in = 0;
        private ushort _out = 0;

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
