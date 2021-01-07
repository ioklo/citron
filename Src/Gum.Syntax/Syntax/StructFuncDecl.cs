using System.Collections.Generic;

namespace Gum.Syntax
{
    public class StructFuncDecl : FuncDecl
    {
        public AccessModifier AccessModifier { get; }
        public bool IsStatic { get; }
        public StructFuncDecl(
            AccessModifier accessModifier,
            bool bStatic,
            bool bSequence,
            TypeExp retType,
            string name,
            IEnumerable<string> typeParams,
            FuncParamInfo paramInfo,
            BlockStmt body)
            : base(bSequence, retType, name, typeParams, paramInfo, body)
        {
            AccessModifier = accessModifier;
            IsStatic = bStatic;
        }
    }
}
