using System.Collections.Generic;
using System.IO;
using SonicAudioLib.CriMw;

namespace SonicAudioLib.CriMw.Serialization.TableType.Adx.CueSheet
{
    [CriSerializable("TBLIGR")]
    public class CriTableAisacGraph
    {
        private byte _type;
        private float _imax;
        private float _imin;
        private float _omax;
        private float _omin;
        private byte[] _points;

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

        [CriField("points", 5)]
        public byte[] Points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
            }
        }
    }
}
