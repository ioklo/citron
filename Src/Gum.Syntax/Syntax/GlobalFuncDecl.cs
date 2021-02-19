using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.Syntax
{
    public class GlobalFuncDecl : FuncDecl
    {
        public GlobalFuncDecl(
            bool bSequence,
            TypeExp retType, string name, ImmutableArray<string> typeParams,
            FuncParamInfo paramInfo, BlockStmt body)
            : base(bSequence, retType, name, typeParams, paramInfo, body)
        {
        }
    }
}