using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CsbBuilder.BuilderNodes
{
    public class BuilderAisacGraphNode : BuilderBaseNode
    {
        [Category("General")]
        [Description("Type of this graph. Values are unknown.")]
        public byte Type { get; set; }

        [Category("Vector"), DisplayName("Maximum X")]
        [Description("End X position of this graph.")]
        public float MaximumX { get; set; }

        [Category("Vector"), DisplayName("Minimum X")]
        [Description("Begin X position of this graph.")]
        public float MinimumX { get; set; }

        [Category("Vector"), DisplayName("Maximum Y")]
        [Description("End Y position of this graph.")]
        public float MaximumY { get; set; }

        [Category("Vector"), DisplayName("Minimum Y")]
        [Description("End X position of this graph.")]
        public float MinimumY { get; set; }

        [Category("Vector"), DisplayName("Points")]
        public List<BuilderAisacPointNode> Points { get; set; }

        public BuilderAisacGraphNode()
        {
            Points = new List<BuilderAisacPointNode>();
        }
    }
}
