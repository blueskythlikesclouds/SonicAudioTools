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
        [Description("The type of this Graph. Currently, none of the types are known.")]
        public byte Type { get; set; }

        [Category("Vector"), DisplayName("Maximum X")]
        [Description("The maximum range that the X values in this Graph can reach.")]
        public float MaximumX { get; set; }

        [Category("Vector"), DisplayName("Minimum X")]
        [Description("The minimum range that the X values in this Graph can reach.")]
        public float MinimumX { get; set; }

        [Category("Vector"), DisplayName("Maximum Y")]
        [Description("The maximum range that the Y values in this Graph can reach.")]
        public float MaximumY { get; set; }

        [Category("Vector"), DisplayName("Minimum Y")]
        [Description("The minimum range that the Y values in this Graph can reach.")]
        public float MinimumY { get; set; }

        [Category("Vector"), DisplayName("Points")]
        [Description("The points of this Graph.")]
        public List<BuilderAisacPointNode> Points { get; set; }

        public BuilderAisacGraphNode()
        {
            Points = new List<BuilderAisacPointNode>();
        }
    }
}
