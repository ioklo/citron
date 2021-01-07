using System.Collections.Generic;

namespace Gum.Syntax
{
    public class GlobalFuncDecl : FuncDecl
    {
        public GlobalFuncDecl(
            bool bSequence,
            TypeExp retType, string name, IEnumerable<string> typeParams,
            FuncParamInfo paramInfo, BlockStmt body)
            : base(bSequence, retType, name, typeParams, paramInfo, body)
        {
        }
    }
}