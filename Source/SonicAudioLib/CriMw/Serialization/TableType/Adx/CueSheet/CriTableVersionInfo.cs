using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBL_INFO")]
    public class CriTableVersionInfo
    {
        private uint _DataFmtVer;
        private uint _ExtSize;

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
