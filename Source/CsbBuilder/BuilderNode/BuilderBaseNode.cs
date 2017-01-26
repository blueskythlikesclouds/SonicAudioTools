using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;

namespace CsbBuilder.BuilderNode
{
    public abstract class BuilderBaseNode : ICloneable
    {
        [Category("General"), ReadOnly(true)]
        [Description("The name of this node. Shift JIS encoding is used for this in the Cue Sheet Binary, so try to avoid using special characters that this codec does not support.")]
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
