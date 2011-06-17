using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mystique.Core
{
    internal static class Initializer
    {
        internal static void Init()
        {
            System.Diagnostics.Debug.WriteLine(
                String.Join(", ", Inscribe.Filter.Core.FilterRegistrant.RegisteredFilters.Select(t => t.ToString())));
            try
            {
                var structure = Inscribe.Filter.Core.QueryCompiler.ToFilter("(mtree:1003131)");
                System.Diagnostics.Debug.WriteLine(structure.ToQuery());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }
}
