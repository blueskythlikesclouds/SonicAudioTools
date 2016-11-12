using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SonicAudioLib.IO;
using SonicAudioLib.Module;
using System.Collections;

namespace SonicAudioLib.Archive
{
    public abstract class EntryBase
    {
        protected long length;

        public virtual long Position { get; set; }

        public virtual long Length
        {
            get
            {
                if (FilePath != null)
                {
                    return FilePath.Length;
                }

                return length;
            }

            set
            {
                length = value;
            }
        }

        public virtual FileInfo FilePath { get; set; }

        public virtual Stream Open(Stream source)
        {
            return new Substream(source, Position, length);
        }

        public virtual Stream Open()
        {
            return FilePath.OpenRead();
        }
    }

    public abstract class ArchiveBase<T> : ModuleBase, IEnumerable<T>
    {
        protected List<T> entries = new List<T>();

        public virtual T this[int index]
        {
            get
            {
                return entries[index];
            }
        }

        public virtual int Count
        {
            get
            {
                return entries.Count;
            }
        }

        public virtual void Add(T item)
        {
            entries.Add(item);
        }

        public virtual T Get(int index)
        {
            return entries[index];
        }

        public virtual void Clear()
        {
            entries.Clear();
        }

        public virtual bool Contains(T item)
        {
            return entries.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            entries.CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        public virtual bool Remove(T item)
        {
            return entries.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }
    }
}
