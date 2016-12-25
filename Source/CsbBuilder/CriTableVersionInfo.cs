using System.IO;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBL_INFO")]
    public class CriTableVersionInfo
    {
        private uint _DataFmtVer = 0x940000;
        private uint _ExtSize = 0;

        [CriField("DataFmtVer", 0)]
        public uint DataFormatVersion
        {
            get
            {
                return _DataFmtVer;
            }
            set
            {
                _DataFmtVer = value;
            }
        }

        [CriField("ExtSize", 1)]
        public uint ExtensionSize
        {
            get
            {
                return _ExtSize;
            }
            set
            {
                _ExtSize = value;
            }
        }
    }
}
