using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonicAudioLib.IO
{
    public class Methods
    {
        public static long Align(long value, long alignment)
        {
            while ((value % alignment) != 0)
            {
                value++;
            }

            return value;
        }
    }
}
