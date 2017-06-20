using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CsbBuilder.BuilderNodes
{
    public class BuilderCueNode : BuilderBaseNode
    {
        [Category("General"), DisplayName("Identifier")]
        [Description("The identifier of this Cue. Must be unique for this Cue Sheet.")]
        public uint Identifier { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Synth Reference Path")]
        [Description("The full path of the Synth (can be Track or Sound) that this Cue is referenced to. When this Cue is called to play in game, the referenced Synth will be played.")]
        public string SynthReference { get; set; }

        [Category("General"), DisplayName("User Comment")]
        [Description("User comment of this Cue.")]
        public string UserComment { get; set; }

        [Category("General")]
        [Description("Currently, none of the flags are known. However, value '1' seems to mute the Cue in-game.")]
        public byte Flags { get; set; }
    }
}
