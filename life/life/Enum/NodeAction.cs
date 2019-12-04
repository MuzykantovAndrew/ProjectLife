using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace life.Enum
{
    public enum NodeAction
    {
        Waiting,    // For idle nodes and inbetween processing steps
        Die,        // Node will die
        StayAlive,  // Node will stay alive
        Populate    // Node will become alive
    }
}
