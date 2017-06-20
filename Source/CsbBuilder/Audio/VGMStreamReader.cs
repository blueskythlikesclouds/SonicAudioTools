using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CsbBuilder.Audio
{
    /// <summary>
    /// Represents a NAudio WaveStream for VGMSTREAM playback.
    /// </summary>
    public class VGMStreamReader : WaveStream
    {
        private IntPtr vgmstream;
        private WaveFormat waveFormat;
        private int sampleCount;
        private bool loopFlag;

        private byte[] cache;

        /// <summary>
        /// Gets the wave format of the VGMSTREAM.
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get
            {
                return waveFormat;
            }
        }

        /// <summary>
        /// Gets the length of this VGMSTREAM in bytes.
        /// </summary>
        public override long Length
        {
            get
            {
                return sampleCount * waveFormat.Channels * 2;
            }
        }

        /// <summary>
        /// Gets or sets the position of this VGMSTREAM in bytes.
        /// </summary>
        public override long Position
        {
            get
            {
                return VGMStreamNative.GetCurrentSample(vgmstream) * waveFormat.Channels * 2;
            }

            set
            {
                CurrentSample = (int)(value / waveFormat.Channels / 2);
            }
        }

        /// <summary>
        /// Gets or sets the current sample of this VGMSTREAM.
        /// </summary>
        public int CurrentSample
        {
            get
            {
                return VGMStreamNative.GetCurrentSample(vgmstream);
            }

            set
            {
                int currentSample = VGMStreamNative.GetCurrentSample(vgmstream);

                if (value == currentSample || value > sampleCount)
                {
                    return;
                }

                if (value > currentSample)
                {
                    value = value - currentSample;
                }

                else if (value < currentSample)
                {
                    VGMStreamNative.Reset(vgmstream);
                }

                int cacheSampleLength = cache.Length / waveFormat.Channels / 2;
                int length = 0;

                while (length < value)
                {
                    if (length + cacheSampleLength >= value)
                    {
                        cacheSampleLength = value - length;
                    }

                    VGMStreamNative.Render8(cache, cacheSampleLength, vgmstream);
                    length += cacheSampleLength;
                }
            }
        }

        /// <summary>
        /// Determines whether the loop is enabled for the VGMSTREAM.
        /// </summary>
        public bool LoopFlag
        {
            get
            {
                return loopFlag;
            }
        }

        /// <summary>
        /// Gets the loop start sample of the VGMSTREAM.
        /// </summary>
        public long LoopStartSample
        {
            get
            {
                return VGMStreamNative.GetLoopStartSample(vgmstream);
            }
        }

        /// <summary>
        /// Gets the loop start position of the VGMSTREAM in bytes.
        /// </summary>
        public long LoopStartPosition
        {
            get
            {
                return LoopStartSample * waveFormat.Channels * 2;
            }
        }

        /// <summary>
        /// Gets the loop end sample of the VGMSTREAM.
        /// </summary>
        public long LoopEndSample
        {
            get
            {
                return VGMStreamNative.GetLoopEndSample(vgmstream);
            }
        }

        /// <summary>
        /// Gets the loop end position of the VGMSTREAM in bytes.
        /// </summary>
        public long LoopEndPosition
        {
            get
            {
                return LoopEndSample * waveFormat.Channels * 2;
            }
        }

        /// <summary>
        /// Forces the VGMSTREAM to loop from start to end.
        /// This method destroys the previous loop points.
        /// </summary>
        public void ForceLoop()
        {
            VGMStreamNative.SetLoopFlag(vgmstream, true);
            VGMStreamNative.SetLoopStartSample(vgmstream, 0);
            VGMStreamNative.SetLoopEndSample(vgmstream, sampleCount);

            loopFlag = true;
        }

        /// <summary>
        /// Disables the loop for this VGMSTREAM.
        /// </summary>
        public void DisableLoop()
        {
            loopFlag = false;
            VGMStreamNative.SetLoopFlag(vgmstream, false);
        }

        /// <summary>
        /// Resets the VGMSTREAM to beginning.
        /// </summary>
        public void Reset()
        {
            VGMStreamNative.Reset(vgmstream);
        }

        /// <summary>
        /// Renders the VGMSTREAM to byte buffer.
        /// </summary>
        /// <param name="buffer">Destination byte buffer.</param>
        /// <param name="offset">Offset within buffer to write to.</param>
        /// <param name="count">Count of bytes to render.</param>
        /// <returns>Number of bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (cache == null || cache.Length < buffer.Length)
            {
                cache = new byte[buffer.Length];
            }

            int currentSample = VGMStreamNative.GetCurrentSample(vgmstream);
            int sampleCount_vgmstream = count / waveFormat.Channels / 2;

            if (currentSample >= sampleCount && !loopFlag)
            {
                return 0;
            }

            if (!loopFlag && currentSample + sampleCount_vgmstream > sampleCount)
            {
                sampleCount_vgmstream = sampleCount - currentSample;
            }

            count = sampleCount_vgmstream * waveFormat.Channels * 2;

            VGMStreamNative.Render8(cache, sampleCount_vgmstream, vgmstream);
            Array.Copy(cache, 0, buffer, offset, count);

            return count;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                VGMStreamNative.Close(vgmstream);
            }

            base.Dispose(disposing);
        }

        private void FromPtr(IntPtr vgmstream)
        {
            if (vgmstream == IntPtr.Zero)
            {
                throw new NullReferenceException("VGMSTREAM pointer is set to null!");
            }

            waveFormat = new WaveFormat(VGMStreamNative.GetSampleRate(vgmstream), 16, VGMStreamNative.GetChannelCount(vgmstream));
            sampleCount = VGMStreamNative.GetSampleCount(vgmstream);
            loopFlag = VGMStreamNative.GetLoopFlag(vgmstream);

            cache = new byte[4096];
        }

        /// <summary>
        /// Constructs from source file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name to load.</param>
        public VGMStreamReader(string sourceFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                throw new FileNotFoundException($"VGMStream could not find file: {sourceFileName}");
            }

            vgmstream = VGMStreamNative.Initialize(sourceFileName);
            if (vgmstream == IntPtr.Zero)
            {
                throw new NullReferenceException($"VGMStream could not initialize file: {sourceFileName}");
            }

            FromPtr(vgmstream);
        }

        /// <summary>
        /// Constructs from pointer.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public VGMStreamReader(IntPtr vgmstream)
        {
            FromPtr(vgmstream);
        }
    }
}
