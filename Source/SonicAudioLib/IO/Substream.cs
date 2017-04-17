using System;
using System.IO;
using System.Linq;

namespace SonicAudioLib.IO
{
    /// <summary>
    /// Represents a <see cref="Stream"/> based substream for viewing a portion of a Stream.
    /// </summary>
    public class Substream : Stream
    {
        private Stream baseStream;
        private long streamPosition;
        private long streamLength;

        /// <summary>
        /// Determines whether the base Stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        /// <summary>
        /// Determines whether the base Stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return baseStream.CanSeek;
            }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the length of the substream.
        /// </summary>
        public override long Length
        {
            get
            {
                return streamLength;
            }
        }

        /// <summary>
        /// Gets or sets the position of the substream.
        /// </summary>
        public override long Position
        {
            get
            {
                return baseStream.Position - streamPosition;
            }

            set
            {
                baseStream.Position = value + streamPosition;
            }
        }

        /// <summary>
        /// Gets or sets the position of the base Stream.
        /// </summary>
        public long AbsolutePosition
        {
            get
            {
                return baseStream.Position;
            }

            set
            {
                baseStream.Position = value;
            }
        }

        /// <summary>
        /// Closes the substream.
        /// </summary>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (baseStream.Position >= streamPosition + streamLength)
            {
                count = 0;
            }
            else if (baseStream.Position + count > streamPosition + streamLength)
            {
                count = (int)(streamPosition + streamLength - baseStream.Position);
            }

            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                offset += streamPosition;
            }

            else if (origin == SeekOrigin.End)
            {
                offset = streamPosition + streamLength - offset;
                origin = SeekOrigin.Begin;
            }

            return baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Seeks to the start of the substream.
        /// </summary>
        public void SeekToStart()
        {
            baseStream.Position = streamPosition;
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/>.
        /// </summary>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets an array of the data that the substream covers.
        /// </summary>
        public byte[] ToArray()
        {
            using (MemoryStream destination = new MemoryStream())
            {
                CopyTo(destination);
                return destination.ToArray();
            }
        }

        /// <summary>
        /// Creates a substream by the specified base Stream at the specified offset.
        /// </summary>
        public Substream(Stream baseStream, long streamPosition) : this(baseStream, streamPosition, baseStream.Length - streamPosition)
        {
        }

        /// <summary>
        /// Creates a substream by the specified base Stream at the specified offset and with the specified length.
        /// </summary>
        public Substream(Stream baseStream, long streamPosition, long streamLength)
        {
            this.baseStream = baseStream;
            this.streamPosition = streamPosition;
            this.streamLength = streamLength;

            SeekToStart();
        }
    }
}
