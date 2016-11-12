using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SonicAudioLib.Module
{
    public abstract class ModuleBase
    {
        public abstract void Read(Stream source);
        public abstract void Write(Stream destination);

        public virtual void Load(string sourceFileName)
        {
            using (Stream source = File.OpenRead(sourceFileName))
            {
                Read(source);
            }
        }

        public virtual void Load(byte[] sourceByteArray)
        {
            using (Stream source = new MemoryStream(sourceByteArray))
            {
                Read(source);
            }
        }

        public virtual void Save(string destinationFileName)
        {
            using (Stream destination = File.Create(destinationFileName))
            {
                Write(destination);
            }
        }

        public virtual byte[] Save()
        {
            using (MemoryStream destination = new MemoryStream())
            {
                Write(destination);
                return destination.ToArray();
            }
        }

        public virtual long CalculateLength()
        {
            return -1;
        }
    }
}
