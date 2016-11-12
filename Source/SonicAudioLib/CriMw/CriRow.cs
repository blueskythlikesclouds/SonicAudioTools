using SonicAudioLib.Collections;
using System.Collections;
using System.Linq;

namespace SonicAudioLib.CriMw
{
    public class CriRow : IEnumerable
    {
        private OrderedDictionary<CriField, object> records = new OrderedDictionary<CriField, object>();
        private CriTable parent;

        public object this[CriField criField]
        {
            get
            {
                return records[criField];
            }

            set
            {
                records[criField] = value;
            }
        }

        public object this[int index]
        {
            get
            {
                return records[index];
            }

            set
            {
                records[index] = value;
            }
        }

        public object this[string name]
        {
            get
            {
                return this[records.Single(k => (k.Key).FieldName == name).Key];
            }

            set
            {
                this[records.Single(k => (k.Key).FieldName == name).Key] = value;
            }
        }

        public CriTable Parent
        {
            get
            {
                return parent;
            }

            internal set
            {
                parent = value;
            }
        }

        internal OrderedDictionary<CriField, object> Records
        {
            get
            {
                return records;
            }
        }

        public int FieldCount
        {
            get
            {
                return records.Count;
            }
        }

        public object[] GetValueArray()
        {
            object[] values = new object[records.Count];
            
            for (int i = 0; i < records.Count; i++)
            {
                values[i] = records[i];
            }

            return values;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var keyValPair in records)
            {
                yield return keyValPair.Value;
            }

            yield break;
        }

        internal CriRow(CriTable parent)
        {
            this.parent = parent;
        }
    }
}
