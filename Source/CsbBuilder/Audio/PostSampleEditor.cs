using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsbBuilder.Audio
{
    public static class PostSampleEditor
    {
        public static void ApplyVolume(byte[] buffer, int offset, int count, double volume)
        {
            for (int i = offset; i < count; i += 2)
            {
                ApplyVolume(buffer, i, volume);
            }
        }

        public static void ApplyVolume(byte[] buffer, int offset, double volume)
        {
            int sample = (int)((short)(buffer[offset] | buffer[offset + 1] << 8) * volume);

            short sample16 =
                sample > short.MaxValue ? short.MaxValue :
                sample < short.MinValue ? short.MinValue :
                (short)sample;

            buffer[offset] = (byte)sample16;
            buffer[offset + 1] = (byte)(sample16 >> 8);
        }
    }
}
