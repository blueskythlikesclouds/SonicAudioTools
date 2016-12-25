using System.IO;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [CriSerializable("AAX")]
    [Serializable]
    public class CriTableAax
    {
        public enum EnumLoopFlag
        {
            Intro = 0,
            Loop = 1,
        };

        private byte[] _data = new byte[0];
        private EnumLoopFlag _lpflg = EnumLoopFlag.Intro;

        [CriField("data", 0)]
        public byte[] Data
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
