using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SonicAudioLib.IO;
using NAudio.Wave;

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
        public byte Version;
        public byte Flags;
        public bool LoopEnabled;
        public uint LoopBeginSampleIndex;
        public uint LoopBeginByteIndex;
        public uint LoopEndSampleIndex;
        public uint LoopEndByteIndex;
    }

    public static class AdxConverter
    {
        public static void ConvertToWav(string sourceFileName)
        {
            ConvertToWav(sourceFileName, Path.ChangeExtension(sourceFileName, "wav"));
        }

        public static void ConvertToWav(string sourceFileName, string destinationFileName)
        {
            BufferedWaveProvider provider = Decode(sourceFileName, 1.0, 0.0);

            using (WaveFileWriter writer = new WaveFileWriter(destinationFileName, provider.WaveFormat))
            {
                int num;

                byte[] buffer = new byte[32767];
                while ((num = provider.Read(buffer, 0, buffer.Length)) != 0)
                {
                    writer.Write(buffer, 0, num);
                }
            }

            provider.ClearBuffer();
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
            header.Version = EndianStream.ReadByte(source);
            header.Flags = EndianStream.ReadByte(source);
            source.Seek(4, SeekOrigin.Current);
            header.LoopEnabled = EndianStream.ReadUInt32BE(source) > 0;
            header.LoopBeginSampleIndex = EndianStream.ReadUInt32BE(source);
            header.LoopBeginByteIndex = EndianStream.ReadUInt32BE(source);
            header.LoopEndSampleIndex = EndianStream.ReadUInt32BE(source);
            header.LoopEndByteIndex = EndianStream.ReadUInt32BE(source);
            return header;
        }

        public static BufferedWaveProvider Decode(string sourceFileName, double volume, double pitch)
        {
            using (Stream source = File.OpenRead(sourceFileName))
            {
                return Decode(source, volume, pitch);
            }
        }

        private static void CalculateCoefficients(double cutoffFrequency, double sampleRate, out short coef1, out short coef2)
        {
            double a = Math.Sqrt(2.0);
            double b = a - Math.Cos(cutoffFrequency * 6.2831855 / sampleRate);
            double c = (b - Math.Sqrt((b - (a - 1.0)) * (a - 1.0 + b))) / (a - 1.0);

            coef1 = (short)(8192.0 * c);
            coef2 = (short)(c * c * -4096.0);
        }

        // https://wiki.multimedia.cx/index.php/CRI_ADX_ADPCM
        public static BufferedWaveProvider Decode(Stream source, double volume, double pitch)
        {
            AdxHeader header = ReadHeader(source);

            WaveFormat waveFormat = new WaveFormat((int)header.SampleRate, 16, header.ChannelCount);
            BufferedWaveProvider provider = new BufferedWaveProvider(waveFormat);
            provider.BufferLength = (int)(header.SampleCount * header.ChannelCount * 2);

            provider.ReadFully = false;
            provider.DiscardOnBufferOverflow = true;

            short firstHistory1 = 0;
            short firstHistory2 = 0;
            short secondHistory1 = 0;
            short secondHistory2 = 0;

            short coef1 = 0;
            short coef2 = 0;

            CalculateCoefficients(header.CutoffFrequency, header.SampleRate, out coef1, out coef2);

            source.Seek(header.DataPosition + 4, SeekOrigin.Begin);

            if (header.ChannelCount == 1)
            {
                for (int i = 0; i < header.SampleCount / 32; i++)
                {
                    byte[] block = EndianStream.ReadBytes(source, header.BlockLength);
                    foreach (short sampleShort in DecodeBlock(block, ref firstHistory1, ref firstHistory2, coef1, coef2))
                    {
                        double sample = (double)sampleShort * volume;

                        if (sample > short.MaxValue)
                        {
                            sample = short.MaxValue;
                        }

                        if (sample < short.MinValue)
                        {
                            sample = short.MinValue;
                        }

                        provider.AddSamples(BitConverter.GetBytes((short)sample), 0, 2);
                    }
                }
            }

            else if (header.ChannelCount == 2)
            {
                for (int i = 0; i < header.SampleCount / 32; i++)
                {
                    byte[] blockLeft = EndianStream.ReadBytes(source, header.BlockLength);
                    byte[] blockRight = EndianStream.ReadBytes(source, header.BlockLength);

                    short[] samplesLeft = DecodeBlock(blockLeft, ref firstHistory1, ref firstHistory2, coef1, coef2);
                    short[] samplesRight = DecodeBlock(blockRight, ref secondHistory1, ref secondHistory2, coef1, coef2);

                    for (int j = 0; j < 32; j++)
                    {
                        double newSampleLeft = samplesLeft[j] * volume;
                        double newSampleRight = samplesRight[j] * volume;

                        if (newSampleLeft > short.MaxValue)
                        {
                            newSampleLeft = short.MaxValue;
                        }

                        if (newSampleLeft < short.MinValue)
                        {
                            newSampleLeft = short.MinValue;
                        }

                        if (newSampleRight > short.MaxValue)
                        {
                            newSampleRight = short.MaxValue;
                        }

                        if (newSampleRight < short.MinValue)
                        {
                            newSampleRight = short.MinValue;
                        }

                        samplesLeft[j] = (short)newSampleLeft;
                        samplesRight[j] = (short)newSampleRight;

                        byte[] sampleLeft = BitConverter.GetBytes(samplesLeft[j]);
                        byte[] sampleRight = BitConverter.GetBytes(samplesRight[j]);

                        provider.AddSamples(new byte[] { sampleLeft[0], sampleLeft[1], sampleRight[0], sampleRight[1] }, 0, 4);
                    }
                }
            }

            return provider;
        }

        public static short[] DecodeBlock(byte[] block, ref short history1, ref short history2, short coef1, short coef2)
        {
            int scale = (block[0] << 8 | block[1]) + 1;
                
            short[] samples = new short[32];

            for (int i = 0; i < 32; i++)
            {
                int sampleByte = block[2 + i / 2];
                int sampleNibble = ((i & 1) != 0 ? (sampleByte & 7) - (sampleByte & 8) : ((sampleByte & 0x70) - (sampleByte & 0x80)) >> 4);
                int sampleDelta = sampleNibble * scale;
                int predictedSample12 = coef1 * history1 + coef2 * history2;
                int predictedSample = predictedSample12 >> 12;

                int sampleRaw = predictedSample + sampleDelta;

                if (sampleRaw > short.MaxValue)
                {
                    sampleRaw = short.MaxValue;
                }

                else if (sampleRaw < short.MinValue)
                {
                    sampleRaw = short.MinValue;
                }

                samples[i] = (short)sampleRaw;

                history2 = history1;
                history1 = (short)sampleRaw;
            }

            return samples;
        }
    }
}
