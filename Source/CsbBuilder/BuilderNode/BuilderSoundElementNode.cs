using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace CsbBuilder.BuilderNode
{
    public class BuilderSoundElementNode : BuilderBaseNode
    {
        [ReadOnly(true)]
        [Category("General")]
        [Description("The name of the audio in Project/Audio directory, which is going to play before the loop audio starts to play. Can be left empty, so that there will be no audio to play.")]
        public string Intro { get; set; }

        [ReadOnly(true)]
        [Category("General")]
        [Description("The name of the audio in Project/Audio directory, which is going to be looped. Can be left empty, so that there will be no audio to loop in-game.")]
        public string Loop { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Channel Count")]
        [Description("The channel count of BOTH Intro and Loop audio files. If they do not match, this will most likely result issues in-game, because the game ignores the channel count information in the audio file itself, but uses this info.")]
        public byte ChannelCount { get; set; }

        [Category("General"), DisplayName("Streamed")]
        [Description("Determines whether the audio files specified here are going to be streamed from a .CPK file, which is outside the .CSB file. Otherwise, it will be played from the memory. That's the best to be 'true', if the specified audio files are large.")]
        public bool Streaming { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sample Rate")]
        [Description("The sample rate of BOTH Intro and Loop audio files. If they do not match, this will most likely result issues in-game, because the game ignores the sample rate information in the audio file itself, but uses this info.")]
        public uint SampleRate { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Sample Count")]
        [Description("The sample count of Intro and Loop files, added together.")]
        public uint SampleCount { get; set; }
    }
}
