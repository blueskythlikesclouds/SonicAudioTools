using System.IO;
using SonicAudioLib.CriMw.Serialization;

namespace CsbBuilder.Serialization
{
    [CriSerializable("TBLIGR")]
    public class SerializationAisacGraphTable
    {
        private byte typeField = 0;
        private float imaxField = 0;
        private float iminField = 0;
        private float omaxField = 0;
        private float ominField = 0;
        private byte[] pointsField = null;

        [CriField("type", 0)]
        public byte Type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
            }
        }

        [CriField("imax", 1)]
        public float InMax
        {
            get
            {
                return imaxField;
            }
            set
            {
                imaxField = value;
            }
        }

        [CriField("imin", 2)]
        public float InMin
        {
            get
            {
                return iminField;
            }
            set
            {
                iminField = value;
            }
        }

        [CriField("omax", 3)]
        public float OutMax
        {
            get
            {
                return omaxField;
            }
            set
            {
                omaxField = value;
            }
        }

        [CriField("omin", 4)]
        public float OutMin
        {
            get
            {
                return ominField;
            }
            set
            {
                ominField = value;
            }
        }

        [CriField("points", 5)]
        public byte[] Points
        {
            get
            {
                return pointsField;
            }
            set
            {
                pointsField = value;
            }
        }
    }
}
