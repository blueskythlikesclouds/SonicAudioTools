using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SonicAudioLib.IO;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CsbBuilder.Audio
{
    public struct AdxHeader
    {
        public ushort Identifier;
        public ushort DataPosition;
        public byte EncodeType;
        public byte BlockLength;
        public byte SampleBitdepth;
        public byte ChannelCount;
        public uint SampleRate;
        public uint SampleCount;
        public ushort CutoffFrequency;
        public ushort Version;
        public short[][] SampleHistories;
    }

    public class AdxFileReader : WaveStream
    {
        private class SampleHistory
        {
            public short Sample1 = 0;
            public short Sample2 = 0;
        }

        private Stream source;

        private AdxHeader header;
        private WaveFormat waveFormat;

        private short coef1;
        private short coef2;

        private SampleHistory[] histories;

        private int sampleCount;
        private int readSamples;

        private byte[] previousSamples;

        private double volume = 1;
        private double pitch = 0;
        private long delayTime = 0;
        private DateTime startTime;

        public override WaveFormat WaveFormat
        {
            get
            {
                return waveFormat;
            }
        }

        public override long Length
        {
            get
            {
                return sampleCount * 2;
            }
        }

        public override long Position
        {
            get
            {
                return readSamples * 2;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsFinished
        {
            get
            {
                return readSamples >= sampleCount;
            }
        }

        public bool IsLoopEnabled { get; set; }

        public double Volume
        {
            set
            {
                volume = value;
            }
        }

        public double Pitch
        {
            set
            {
                pitch = value;
            }
        }

        public int DelayTime
        {
            set
            {
                startTime = DateTime.Now;
                delayTime = value * 10000;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsFinished && !IsLoopEnabled)
            {
                return 0;
            }

            if (delayTime > 0)
            {
                if ((DateTime.Now - startTime).Ticks < delayTime)
                {
                    return count;
                }

                delayTime = 0;
            }

            int length = count;

            while ((length % (header.ChannelCount * 64)) != 0)
            {
                length++;
            }

            byte[] samples = new byte[length];

            int currentLength = 0;

            while (currentLength < length)
            {
                int sampleLength = GetNextSamples(samples, currentLength);

                if (sampleLength < 0)
                {
                    count = count > currentLength ? currentLength : count;
                    break;
                }

                currentLength = sampleLength;
            }

            if (previousSamples != null)
            {
                samples = previousSamples.Concat(samples).ToArray();
                length = samples.Length;
            }

            if (length > count)
            {
                previousSamples = samples.Skip(count).ToArray();
            }

            else if (length < count)
            {
                previousSamples = null;
                count = length;
            }

            else
            {
                previousSamples = null;
            }

            Array.Copy(samples, 0, buffer, offset, count);

            return count;
        }

        private int GetNextSamples(byte[] destination, int startIndex)
        {
            short[][] channelSamples = new short[header.ChannelCount][];

            for (int i = 0; i < header.ChannelCount; i++)
            {
                if (!DecodeBlock(i, out short[] samples))
                {
                    if (IsLoopEnabled)
                    {
                        Reset();
                        DecodeBlock(i, out samples);
                    }

                    else
                    {
                        readSamples = sampleCount;
                        return -1;
                    }
                }

                channelSamples[i] = samples;
            }

            int position = startIndex;

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < header.ChannelCount; j++)
                {
                    short sample = (short)(channelSamples[j][i] * volume);

                    destination[position++] = (byte)sample;
                    destination[position++] = (byte)(sample >> 8);
                }
            }

            return position;
        }

        private bool DecodeBlock(int c, out short[] samples)
        {
            int scale = EndianStream.ReadUInt16BE(source) + 1;

            // There seems to be a null sample block at the end of every adx file.
            // It always added a half second delay between intro and loop, so 
            // I wanted to get rid of it.
            if (scale > short.MaxValue + 1)
            {
                samples = null;
                return false;
            }

            samples = new short[32];

            int sampleByte = 0;

            SampleHistory history = histories[c];
            for (int i = 0; i < 32; i++)
            {
                if ((i % 2) == 0)
                {
                    sampleByte = source.ReadByte();
                }

                int sample = ((i & 1) != 0 ?
                    (sampleByte & 7) - (sampleByte & 8) :
                    ((sampleByte & 0x70) - (sampleByte & 0x80)) >> 4) * scale +
                    ((coef1 * history.Sample1 + coef2 * history.Sample2) >> 12);

                sample = sample > short.MaxValue ? short.MaxValue : sample < short.MinValue ? short.MinValue : sample;

                samples[i] = (short)sample;

                history.Sample2 = history.Sample1;
                history.Sample1 = (short)sample;

                readSamples++;
            }

            return true;
        }

        public void Reset()
        {
            source.Seek(header.DataPosition + 4, SeekOrigin.Begin);
            readSamples = 0;
        }

        public void ReplaceHistories(AdxFileReader reader)
        {
            histories = reader.histories;
        }

        public static AdxHeader LoadHeader(string sourceFileName)
        {
            using (Stream source = File.OpenRead(sourceFileName))
            {
                return ReadHeader(source);
            }
        }

        public static AdxHeader ReadHeader(Stream source)
        {
            AdxHeader header = new AdxHeader();
            header.Identifier = EndianStream.ReadUInt16BE(source);
            header.DataPosition = EndianStream.ReadUInt16BE(source);
            header.EncodeType = EndianStream.ReadByte(source);
            header.BlockLength = EndianStream.ReadByte(source);
            header.SampleBitdepth = EndianStream.ReadByte(source);
            header.ChannelCount = EndianStream.ReadByte(source);
            header.SampleRate = EndianStream.ReadUInt32BE(source);
            header.SampleCount = EndianStream.ReadUInt32BE(source);
            header.CutoffFrequency = EndianStream.ReadUInt16BE(source);
            header.Version = EndianStream.ReadUInt16BE(source);
            return header;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Close();
            }

            base.Dispose(disposing);
        }

        public AdxFileReader(string fileName) : this(File.OpenRead(fileName))
        {
        }

        public AdxFileReader(Stream source)
        {
            this.source = source;

            header = AdxFileReader.ReadHeader(this.source);
            source.Seek(header.DataPosition + 4, SeekOrigin.Begin);

            // Calculate coefficients
            double a = Math.Sqrt(2.0);
            double b = a - Math.Cos(header.CutoffFrequency * Math.PI * 2.0 / header.SampleRate);
            double c = (b - Math.Sqrt((b - (a - 1.0)) * (a - 1.0 + b))) / (a - 1.0);

            coef1 = (short)(8192.0 * c);
            coef2 = (short)(c * c * -4096.0);

            histories = new SampleHistory[header.ChannelCount];
            for (int i = 0; i < histories.Length; i++)
            {
                histories[i] = new SampleHistory();
            }

            sampleCount = (int)(header.SampleCount * header.ChannelCount);
            waveFormat = new WaveFormat((int)header.SampleRate, 16, header.ChannelCount);
        }
    }

    public class ExtendedAdxFileReader : WaveStream
    {
        private List<AdxFileReader> readers = new List<AdxFileReader>();
        private int currentIndex = 0;

        public override long Length
        {
            get
            {
                long totalLength = 0;

                foreach (AdxFileReader reader in readers)
                {
                    totalLength += reader.Length;
                }

                return totalLength;
            }
        }

        public override long Position
        {
            get
            {
                long position = 0;

                foreach (AdxFileReader reader in readers)
                {
                    if (reader == readers[currentIndex])
                    {
                        position += reader.Position;
                        break;
                    }

                    position += reader.Length;
                }

                return position;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override WaveFormat WaveFormat
        {
            get
            {
                return readers[currentIndex].WaveFormat;
            }
        }

        public double Volume
        {
            set
            {
                readers.ForEach(reader => reader.Volume = value);
            }
        }

        public double Pitch
        {
            set
            {
                readers.ForEach(reader => reader.Pitch = value);
            }
        }

        public int DelayTime
        {
            set
            {
                readers.First().DelayTime = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = readers[currentIndex].Read(buffer, 0, count);

            if ((num < count) && !readers[currentIndex].IsLoopEnabled)
            {
                currentIndex++;

                readers[currentIndex].ReplaceHistories(readers[currentIndex - 1]);

                int num2 = readers[currentIndex].Read(buffer, num, count - num);
                return num + num2;
            }

            else if (readers[currentIndex].IsFinished && !readers[currentIndex].IsLoopEnabled)
            {
                currentIndex++;

                readers[currentIndex].ReplaceHistories(readers[currentIndex - 1]);

                num = readers[currentIndex].Read(buffer, 0, count);
            }

            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                readers.ForEach(reader => reader.Dispose());
            }

            base.Dispose(disposing);
        }

        public ExtendedAdxFileReader(params string[] fileNames) : this(fileNames.Select(fileName => File.OpenRead(fileName)).ToArray())
        {
        }

        public ExtendedAdxFileReader(params Stream[] sources)
        {
            foreach (Stream source in sources)
            {
                readers.Add(new AdxFileReader(source));
            }

            // The last one is the one to loop
            readers.Last().IsLoopEnabled = true;
        }
    }
}
