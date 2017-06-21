using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace CsbBuilder.Audio
{
    public class ExtendedWaveStream : WaveStream
    {
        private int currentStreamIndex = 0;
        private readonly List<WaveStream> streams = new List<WaveStream>();

        private double volume = 1;
        private double pitch = 0;
        private long delayTime = 0;
        private DateTime startTime;

        public override WaveFormat WaveFormat
        {
            get
            {
                WaveFormat waveFormat = streams[currentStreamIndex].WaveFormat;

                return new WaveFormat(waveFormat.SampleRate + (int)(waveFormat.SampleRate * pitch), waveFormat.Channels);
            }
        }

        public override long Length
        {
            get
            {
                return streams.Sum(reader => reader.Length);
            }
        }

        public override long Position
        {
            get
            {
                return streams.Take(currentStreamIndex).Sum(reader => reader.Length) + streams[currentStreamIndex].Position;
            }

            set
            {
                long position = 0;

                for (int i = 0; i < streams.Count; i++)
                {
                    if (position + streams[i].Length > value)
                    {
                        currentStreamIndex = i;
                        streams[i].Position = value - position;
                        break;
                    }

                    position += streams[i].Length;
                }
            }
        }

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

        public bool ForceLoop { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (delayTime > 0)
            {
                if ((DateTime.Now - startTime).Ticks < delayTime)
                {
                    return count;
                }

                delayTime = 0;
            }

            int num = streams[currentStreamIndex].Read(buffer, 0, count);

            if (num < count && currentStreamIndex != streams.Count - 1)
            {
                currentStreamIndex++;
                num += streams[currentStreamIndex].Read(buffer, num, count - num);
            }

            else if (ForceLoop && num < count && currentStreamIndex == streams.Count - 1)
            {
                streams[currentStreamIndex].Position = 0;
                num += streams[currentStreamIndex].Read(buffer, num, count - num);
            }

            if (volume != 1)
            {
                PostSampleEditor.ApplyVolume(buffer, offset, num, volume);
            }

            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var reader in streams)
                {
                    reader.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        public ExtendedWaveStream(params WaveStream[] waveStreams)
        {
            if (waveStreams.Length == 0)
            {
                throw new ArgumentException("You must at least specify one source!", nameof(waveStreams));
            }

            streams.AddRange(waveStreams);
        }
    }
}
