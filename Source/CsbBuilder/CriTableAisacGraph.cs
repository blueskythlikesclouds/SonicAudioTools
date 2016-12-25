using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;
using SonicAudioLib.CriMw.Serialization;

using System;
using System.Xml.Serialization;

namespace CsbBuilder
{
    [Serializable]
    [CriSerializable("TBLIGR")]
    public class CriTableAisacGraph
    {
        private byte _type = 0;
        private float _imax = 0;
        private float _imin = 0;
        private float _omax = 0;
        private float _omin = 0;

        [CriField("type", 0)]
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

        [CriField("imax", 1)]
        public float InMaximum
        {
            get
            {
                return _imax;
            }
            set
            {
                _imax = value;
            }
        }

        [CriField("imin", 2)]
        public float InMinimum
        {
            get
            {
                return _imin;
            }
            set
            {
                _imin = value;
            }
        }

        [CriField("omax", 3)]
        public float OutMaximum
        {
            get
            {
                return _omax;
            }
            set
            {
                _omax = value;
            }
        }

        [CriField("omin", 4)]
        public float OutMinimum
        {
            get
            {
                return _omin;
            }
            set
            {
                _omin = value;
            }
        }

        [CriIgnore]
        public List<CriTableAisacPoint> PointsList { get; set; }

        [XmlIgnore]
        [CriField("points", 5)]
        public byte[] Points
        {
            get
            {
                return CriTableSerializer.Serialize(PointsList, CriTableWriterSettings.AdxSettings);
            }

            set
            {
                PointsList = CriTableSerializer.Deserialize<CriTableAisacPoint>(value);
            }
        }
    }
}
