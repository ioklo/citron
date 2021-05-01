using System;
using System.Collections.Generic;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    class TypeContext
    {
        Dictionary<(int Depth, int Index), R.Path> dict;

        public R.Path Apply(R.Path path)
        {
            throw new NotImplementedException();
        }
    }
}