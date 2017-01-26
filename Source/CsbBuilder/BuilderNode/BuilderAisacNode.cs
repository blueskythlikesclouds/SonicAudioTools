using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CsbBuilder.BuilderNode
{
    public class BuilderAisacNode : BuilderBaseNode
    {
        [Category("General"), DisplayName("Aisac Name")]
        [Description("The name of this Aisac. (Shouldn't be seen as the node name.)")]
        public string AisacName { get; set; }

        [Category("General")]
        [Description("The type of this Aisac. Currently, none of the types are known.")]
        public byte Type { get; set; }

        [Category("Graph")]
        [Description("The Graph's of this Aisac.")]
        public List<BuilderAisacGraphNode> Graphs { get; set; }

        [Category("Graph"), DisplayName("Random Range (Unknown)")]
        public byte RandomRange { get; set; }

        public BuilderAisacNode()
        {
            Graphs = new List<BuilderAisacGraphNode>();
        }
    }
}
