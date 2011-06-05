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
                String.Join(", ", Inscribe.Filter.Manager.FilterRegistrant.RegisteredFilters.Select(t => t.ToString())));
            try
            {
                var structure = Inscribe.Filter.QuerySystem.QueryCompiler.ToFilter("(verified | (((protected & verified!))))");
                System.Diagnostics.Debug.WriteLine(structure);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }
}
