using System;
using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLCSB")]
    public class CriTableCueSheet
    {
        public enum EnumTableType
        {
            Cue = 1,
            Synth = 2,
            SoundElement = 4,
            Aisac = 5,
            VoiceLimitGroup = 6,
            VersionInfo = 7,
        };

        private string _name;
        private EnumTableType _ttype;
        private byte[] _utf;

        [CriField("name", 0)]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [CriField("ttype", 1)]
        public byte TableType
        {
            get
            {
                return (byte)_ttype;
            }
            set
            {
                _ttype = (EnumTableType)value;
            }
        }

        [CriField("utf", 2)]
        public byte[] Data
        {
            get
            {
                return _utf;
            }
            set
            {
                _utf = value;
            }
        }
    }
}
