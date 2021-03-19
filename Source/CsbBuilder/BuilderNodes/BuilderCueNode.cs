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
        [Category("General"), DisplayName("ID")]
        [Description("ID of this cue.")]
        public uint Id { get; set; }

        [ReadOnly(true)]
        [Category("General"), DisplayName("Synth Reference Path")]
        [Description("Full path of synth reference.")]
        public string SynthReference { get; set; }

        [Category("General"), DisplayName("User Comment")]
        public string UserComment { get; set; }

        [Category("General")]
        [Description("Flags of this cue. Values are unknown.")]
        public byte Flags { get; set; }
    }
}
