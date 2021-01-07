using Gum.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using S = Gum.Syntax;

namespace Gum.IR0
{
    public class SyntaxItemInfoRepository
    {
        ImmutableDictionary<S.FuncDecl, FuncInfo> funcInfosByDecl;
        ImmutableDictionary<S.TypeDecl, TypeInfo> typeInfosByDecl;

        public SyntaxItemInfoRepository(IDictionary<S.FuncDecl, FuncInfo> funcInfosByDecl, IDictionary<S.TypeDecl, TypeInfo> typeInfosByDecl)
        {
            this.funcInfosByDecl = funcInfosByDecl.ToImmutableDictionary();
            this.typeInfosByDecl = typeInfosByDecl.ToImmutableDictionary();
        }

        public FuncInfo GetFuncInfoByDecl(S.FuncDecl funcDecl)
        {
            return funcInfosByDecl[funcDecl];
        }

        public TypeInfo GetTypeInfoByDecl(S.TypeDecl typeDecl)
        {
            return typeInfosByDecl[typeDecl];
        }
    }
}