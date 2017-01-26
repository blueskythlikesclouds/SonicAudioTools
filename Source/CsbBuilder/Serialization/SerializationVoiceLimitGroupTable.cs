using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLVLG")]
    public class SerializationVoiceLimitGroupTable
    {
        private string vlgnameField = string.Empty;
        private uint vlgnvoiceField = 0;

        [CriField("vlgname", 0)]
        public string VoiceLimitGroupName
        {
            get
            {
                return vlgnameField;
            }
            set
            {
                vlgnameField = value;
            }
        }

        [CriField("vlgnvoice", 1)]
        public uint VoiceLimitGroupNum
        {
            get
            {
                return vlgnvoiceField;
            }
            set
            {
                vlgnvoiceField = value;
            }
        }
    }
}
