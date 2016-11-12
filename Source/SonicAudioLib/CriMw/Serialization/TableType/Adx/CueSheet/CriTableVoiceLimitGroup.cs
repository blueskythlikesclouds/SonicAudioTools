using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLVLG")]
    public class CriTableVoiceLimitGroup
    {
        private string _vlgname = string.Empty;
        private uint _vlgnvoice = 0;

        [CriField("vlgname", 0)]
        public string VoiceLimitGroupName
        {
            get
            {
                return _vlgname;
            }
            set
            {
                _vlgname = value;
            }
        }

        [CriField("vlgnvoice", 1)]
        public uint VoiceLimitGroupNum
        {
            get
            {
                return _vlgnvoice;
            }
            set
            {
                _vlgnvoice = value;
            }
        }
    }
}
