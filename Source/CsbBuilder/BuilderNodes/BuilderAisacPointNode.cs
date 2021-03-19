using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CsbBuilder.BuilderNodes
{
    public class BuilderAisacPointNode : BuilderBaseNode
    {
        [Category("Vector"), DisplayName("X")]
        [Description("Normalized X coordinate of this point.")]
        public ushort X { get; set; }

        [Category("Vector"), DisplayName("Y")]
        [Description("Normalized Y coordinate of this point.")]
        public ushort Y { get; set; }
    }
}
