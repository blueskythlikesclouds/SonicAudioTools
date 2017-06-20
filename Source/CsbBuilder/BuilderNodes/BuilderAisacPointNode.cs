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
        [Description("The X dimension of this Point, relative to the Graph it's in.")]
        public ushort X { get; set; }

        [Category("Vector"), DisplayName("Y")]
        [Description("The Y dimension of this Point, relative to the Graph it's in.")]
        public ushort Y { get; set; }
    }
}
