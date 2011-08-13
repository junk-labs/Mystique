using System;
using System.Collections.Generic;

namespace Inscribe.Subsystems.KeyAssign
{
    public class AssignDescription
    {
        public string Name { get; set; }

        public IEnumerable<Tuple<AssignRegion, IEnumerable<AssignItem>>> AssignDatas { get; set; }
    }
}
