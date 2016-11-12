using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("AAX")]
    public class CriTableAax
    {
        public enum EnumLoopFlag
        {
            Intro = 0,
            Loop = 1,
        };

        private FileInfo _data;
        private EnumLoopFlag _lpflg;

        [CriField("data", 0)]
        public FileInfo Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        [CriField("lpflg", 1)]
        public byte LoopFlag
        {
            get
            {
                return (byte)_lpflg;
            }
            set
            {
                _lpflg = (EnumLoopFlag)value;
            }
        }
    }
}
