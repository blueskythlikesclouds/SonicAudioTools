using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CsbBuilder.BuilderNodes
{
    public class BuilderSoundElementNode : BuilderBaseNode
    {
        [ReadOnly(true)]
        [Category("General")]
        [Description("File path of audio to play before loop. This can be left empty if loop is set.")]
        public string Intro { get; set; }

        [ReadOnly(true)]
        [Category("General")]
        [Description("File path of audio to play after intro. This can be left empty if intro is set.")]
        public string Loop { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Channel Count")]
        [Description("Channel count of audio files. This information is used instead of the metadata in audio files.")]
        public byte ChannelCount { get; set; }

        [Category("General"), DisplayName("Streamed")]
        [Description("Determines whether audio files are going to be streamed from a .cpk file.")]
        public bool Streaming { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sample Rate")]
        [Description("Sample rate of audio files. This information is used instead of the metadata in audio files.")]
        public uint SampleRate { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sample Count")]
        [Description("Sample count of audio files added together.")]
        public uint SampleCount { get; set; }
    }
}
