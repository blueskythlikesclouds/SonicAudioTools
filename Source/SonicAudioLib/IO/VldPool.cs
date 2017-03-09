using System.Collections;
using System.IO;

using SonicAudioLib.Module;

namespace SonicAudioLib.IO
{
    public class VldPool
    {
        private ArrayList items = new ArrayList();

        private long startPosition = 0;
        private uint align = 1;
        private long length = 0;

        public long Position
        {
            get
            {
                return startPosition;
            }
        }

        public long Length
        {
            get
            {
                return length;
            }
        }

        public long Align
        {
            get
            {
                return align;
            }
        }

        public long Put(byte[] data)
        {
            if (data == null || data.Length <= 0)
            {
                return 0;
            }

            length = Helpers.Align(length, align);

            long position = length;
            length += data.Length;
            items.Add(data);

            return position;
        }

        public long Put(Stream stream)
        {
            if (stream == null || stream.Length <= 0)
            {
                return 0;
            }

            length = Helpers.Align(length, align);

            long position = length;
            length += stream.Length;
            items.Add(stream);

            return position;
        }

        public long Put(FileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo.Length <= 0)
            {
                return 0;
            }

            length = Helpers.Align(length, align);

            long position = length;
            length += fileInfo.Length;
            items.Add(fileInfo);

            return position;
        }

        public long Put(ModuleBase module)
        {
            if (module == null)
            {
                return 0;
            }

            length = Helpers.Align(length, align);

            long position = length;
            length += module.CalculateLength();
            items.Add(module);

            return position;
        }

        public void Write(Stream destination)
        {
            startPosition = destination.Position;

            foreach (object item in items)
            {
                EndianStream.Pad(destination, align);

                if (item is byte[] bytes)
                {
                    destination.Write(bytes, 0, bytes.Length);
                }

                else if (item is Stream stream)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(destination);
                }

                else if (item is FileInfo fileInfo)
                {
                    using (Stream source = fileInfo.OpenRead())
                    {
                        source.CopyTo(destination);
                    }
                }

                else if (item is ModuleBase module)
                {
                    module.Write(destination);
                }
            }
        }

        public void Clear()
        {
            items.Clear();
        }

        public VldPool(uint align, long baseLength)
        {
            this.align = align;
            length = baseLength;
        }
        
        public VldPool(uint align)
        {
            this.align = align;
        }

        public VldPool()
        {
        }
    }
}
