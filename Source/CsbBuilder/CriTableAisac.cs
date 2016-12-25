using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLISC")]
    public class CriTableAisac
    {
        private string _name = string.Empty;
        private string _ptname = string.Empty;
        private byte _type = 0;
        private byte _rndrng = 0;

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

        [CriIgnore]
        public List<CriTableAisacGraph> GraphList { get; set; }
        
        [XmlIgnore]
        [CriField("grph", 3)]
        public byte[] Graph
        {
            get
            {
                return CriTableSerializer.Serialize(GraphList, CriTableWriterSettings.AdxSettings);
            }

            set
            {
                GraphList = CriTableSerializer.Deserialize<CriTableAisacGraph>(value);
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
