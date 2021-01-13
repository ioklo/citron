using System;
using System.Collections.Generic;
using S = Gum.Syntax;

namespace Gum.IR0
{
    // phase3 순회를 위한 자료구조
    class Phase3Root
    {
        public IEnumerable<S.Stmt> GetTopLevelStmts() { throw new NotImplementedException(); }
        public IEnumerable<Phase3Func> GetAllFuncs() { throw new NotImplementedException(); }
    }
}