using Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citron.Analysis
{
    // private 
    static class Extensions
    {
        public static int GetTotalTypeArgCount(this SymbolPath? path)
        {
            if (path == null)
                return 0;

            if (path.Outer == null)
                return path.TypeArgs.Length;

            return GetTotalTypeArgCount(path.Outer) + path.TypeArgs.Length;
        }

        public static int GetTotalTypeArgCount(this ModuleSymbolId id)
        {
            return id.Path.GetTotalTypeArgCount();
        }
    }
}
