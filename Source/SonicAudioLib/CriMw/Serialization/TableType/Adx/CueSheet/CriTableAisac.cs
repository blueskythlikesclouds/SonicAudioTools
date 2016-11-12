using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLISC")]
    public class CriTableAisac
    {
        private string _name;
        private string _ptname;
        private byte _type;
        private byte[] _grph;
        private byte _rndrng;

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

        [CriField("ptname", 1)]
        public string PathName
        {
            get
            {
                return _ptname;
            }
            set
            {
                _ptname = value;
            }
        }

        [CriField("type", 2)]
        public byte Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        [CriField("grph", 3)]
        public byte[] Graph
        {
            get
            {
                return _grph;
            }
            set
            {
                _grph = value;
            }
        }

        [CriField("rndrng", 4)]
        public byte RandomRange
        {
            get
            {
                return _rndrng;
            }
            set
            {
                _rndrng = value;
            }
        }
    }
}
