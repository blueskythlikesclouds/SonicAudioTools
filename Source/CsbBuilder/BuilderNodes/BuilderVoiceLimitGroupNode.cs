using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CsbBuilder.BuilderNodes
{
    public class BuilderVoiceLimitGroupNode : BuilderBaseNode
    {
        [Category("General"), DisplayName("Max Amount of Instances")]
        public uint MaxAmountOfInstances { get; set; }
    }
}
