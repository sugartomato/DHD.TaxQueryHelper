using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHD.TaxQueryHelper
{
    internal class PathBookmark
    {
        public String Name { get; set; } = String.Empty;
        public String Path { get; set; } = String.Empty;

        public String DisplayName { get
            {
                return $"{Name} | {Path}";
            }
        }

    }
}
