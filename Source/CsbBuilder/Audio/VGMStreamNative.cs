using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CsbBuilder.Audio
{
    /// <summary>
    /// Represents a static class for calling native VGMSTREAM methods.
    /// </summary>
    public static class VGMStreamNative
    {
        /// <summary>
        /// Path of the VGMSTREAM DLL file.
        /// </summary>
        public const string DllName = "vgmstream.dll";

        /// <summary>
        /// Size of VGMSTREAM structure.
        /// </summary>
        public const int SizeOfVgmStream = 152;

        /// <summary>
        /// Size of VGMSTREAMCHANNEL structure.
        /// </summary>
        public const int SizeOfVgmStreamChannel = 552;

        #region VGMStream Exports
        /// <summary>
        /// Initializes a VGMSTREAM from source file name by doing format detection and returns a usable pointer
        /// to it, or NULL on failure.
        /// </summary>
        /// <param name="sourceFileName">Path to source file name.</param>
        /// <returns>Pointer to VGMSTREAM or NULL on failure.</returns>
        [DllImport(DllName, EntryPoint = "init_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Initialize(string sourceFileName);

        /// <summary>
        /// Initializes a VGMSTREAM from stream file by doing format detection and returns a usable pointer
        /// to it, or NULL on failure.
        /// </summary>
        /// <param name="streamFile">Pointer to stream file.</param>
        /// <returns>Pointer to VGMSTREAM or NULL on failure.</returns>
        [DllImport(DllName, EntryPoint = "init_from_STREAMFILE", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitializeFromStreamFile(IntPtr streamFile);

        /// <summary>
        /// Resets a VGMSTREAM to start of stream.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        [DllImport(DllName, EntryPoint = "reset_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Reset(IntPtr vgmstream);

        /// <summary>
        /// Closes an open VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        [DllImport(DllName, EntryPoint = "close_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Close(IntPtr vgmstream);

        /// <summary>
        /// Calculates the number of samples to be played based on looping parameters.
        /// </summary>
        [DllImport(DllName, EntryPoint = "get_vgmstream_play_samples", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPlaySamples(double loopTimes, double fadeSeconds, double fadeDelaySeconds, IntPtr vgmstream);

        /// <summary>
        /// Renders VGMSTREAM to sample buffer.
        /// </summary>
        /// <param name="buffer">Destination sample buffer.</param>
        /// <param name="sampleCount">Amount of samples to render.</param>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        [DllImport(DllName, EntryPoint = "render_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Render(short[] buffer, int sampleCount, IntPtr vgmstream);

        /// <summary>
        /// Renders VGMSTREAM to byte buffer.
        /// </summary>
        /// <param name="buffer">Destination byte buffer.</param>
        /// <param name="sampleCount">Amount of samples to render.</param>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        [DllImport(DllName, EntryPoint = "render_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Render8(byte[] buffer, int sampleCount, IntPtr vgmstream);

        /// <summary>
        /// Writes a description of the stream into array pointed by description,
        /// which must be length bytes long. Will always be null-terminated if length > 0
        /// </summary>
        [DllImport(DllName, EntryPoint = "describe_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Describe(IntPtr vgmstream, IntPtr description, int length);

        /// <summary>
        /// Returns the average bitrate in bps of all unique files contained within
        /// this stream. Compares files by absolute paths.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        [DllImport(DllName, EntryPoint = "get_vgmstream_average_bitrate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetAverageBitrate(IntPtr vgmstream);

        /// <summary>
        /// Allocates a VGMSTREAM.
        /// </summary>
        [DllImport(DllName, EntryPoint = "allocate_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Allocate(int channelCount, [MarshalAs(UnmanagedType.I4)]bool looped);

        /// <summary>
        /// smallest self-contained group of samples is a frame
        /// </summary>
        [DllImport(DllName, EntryPoint = "get_vgmstream_samples_per_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSamplesPerFrame(IntPtr vgmstream);

        /// <summary>
        /// Gets the number of bytes per frame.
        /// </summary>
        [DllImport(DllName, EntryPoint = "get_vgmstream_frame_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFrameSize(IntPtr vgmstream);

        /// <summary>
        /// in NDS IMA the frame size is the block size, so the last one is short
        /// </summary>
        [DllImport(DllName, EntryPoint = "get_vgmstream_samples_per_shortframe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSamplesPerShortframe(IntPtr vgmstream);

        [DllImport(DllName, EntryPoint = "get_vgmstream_shortframe_size", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetShortframeSize(IntPtr vgmstream);

        /// <summary>
        /// Assumes that we have written samplesWrittem into the buffer already, and we have samplesToDo consecutive
        /// samples ahead of us. Decode those samples into the buffer.
        /// </summary>
        [DllImport(DllName, EntryPoint = "decode_vgmstream", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Decode(IntPtr vgmstream, int samplesWritten, int samplesToDo, short[] buffer);

        /// <summary>
        /// Assumes additionally that we have samplesToDo consecutive samples in "data",
        /// and this this is for channel number "channel".
        /// </summary>
        [DllImport(DllName, EntryPoint = "decode_vgmstream_mem", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DecodeMem(IntPtr vgmstream, int samplesWritten, int samplesToDo, short[] buffer, byte[] data, int channel);

        /// <summary>
        /// Calculates number of consecutive samples to do (taking into account stopping for loop start and end.)
        /// </summary>
        [DllImport(DllName, EntryPoint = "vgmstream_samples_to_do", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SamplesToDo(int samplesThisBlock, int samplesPerFrame, IntPtr vgmstream);

        /// <summary>
        /// Detects start and save values, also detects end and restore values. Only works on exact sample values.
        /// </summary>
        [DllImport(DllName, EntryPoint = "vgmstream_do_loop", CallingConvention = CallingConvention.Cdecl)]
        public static extern int DoLoop(IntPtr vgmstream);

        /// <summary>
        /// Opens a stream for reading at offset (standarized taking into account layouts, channels and so on.)
        /// returns 0 on failure
        /// </summary>
        [DllImport(DllName, EntryPoint = "vgmstream_open_stream", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenStream(IntPtr vgmstream, IntPtr streamFile, long position);
        #endregion

        #region Format Exports
        /// <summary>
        /// Gets a pointer to the array of supported formats.
        /// </summary>
        [DllImport(DllName, EntryPoint = "vgmstream_get_formats", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetFormatsPtr();

        /// <summary>
        /// Gets the length of the format array.
        /// </summary>
        /// <returns></returns>
        [DllImport(DllName, EntryPoint = "vgmstream_get_formats_length", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFormatsLength();

        [DllImport(DllName, EntryPoint = "get_vgmstream_coding_description", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetCodingDescription(int codingEnumCode);

        [DllImport(DllName, EntryPoint = "get_vgmstream_layout_description", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetLayoutDescription(int layoutEnumCode);

        [DllImport(DllName, EntryPoint = "get_vgmstream_meta_description", CallingConvention = CallingConvention.Cdecl)]
        public static extern string GetMetaDescription(int metaEnumCode);
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets the sample count of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetSampleCount(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream);
        }

        /// <summary>
        /// Gets the sample rate of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetSampleRate(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 4);
        }

        /// <summary>
        /// Gets the channel count of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetChannelCount(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 8);
        }

        /// <summary>
        /// Gets the absolute sample count of a VGMSTREAM. 
        /// (sample count * channel count)
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetAbsoluteSampleCount(IntPtr vgmstream)
        {
            return GetSampleCount(vgmstream) * GetChannelCount(vgmstream);
        }

        /// <summary>
        /// Gets the loop flag of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static bool GetLoopFlag(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 28) != 0;
        }

        /// <summary>
        /// Sets the loop flag of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static void SetLoopFlag(IntPtr vgmstream, bool value)
        {
            if (value && !GetLoopFlag(vgmstream))
            {
                Marshal.WriteIntPtr(vgmstream, 48, Marshal.AllocHGlobal(GetChannelCount(vgmstream) * SizeOfVgmStreamChannel));
            }

            Marshal.WriteInt32(vgmstream, 28, value ? 1 : 0);
        }

        /// <summary>
        /// Gets the loop start sample of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetLoopStartSample(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 32);
        }

        /// <summary>
        /// Sets the loop start sample of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static void SetLoopStartSample(IntPtr vgmstream, int value)
        {
            Marshal.WriteInt32(vgmstream, 32, value);
        }

        /// <summary>
        /// Gets the loop end sample of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetLoopEndSample(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 36);
        }

        /// <summary>
        /// Sets the loop end sample of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static void SetLoopEndSample(IntPtr vgmstream, int value)
        {
            Marshal.WriteInt32(vgmstream, 36, value);
        }

        /// <summary>
        /// Gets the current sample of a VGMSTREAM.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static int GetCurrentSample(IntPtr vgmstream)
        {
            return Marshal.ReadInt32(vgmstream, 52);
        }

        /// <summary>
        /// Gets an array of supported formats.
        /// </summary>
        /// <param name="vgmstream">Pointer to VGMSTREAM.</param>
        public static string[] GetFormats()
        {
            string[] formats = new string[GetFormatsLength()];

            IntPtr ptr = GetFormatsPtr();
            for (int i = 0; i < formats.Length; i++)
            {
                IntPtr stringPtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                formats[i] = Marshal.PtrToStringAnsi(stringPtr);
            }

            return formats;
        }

        #endregion
    }
}
