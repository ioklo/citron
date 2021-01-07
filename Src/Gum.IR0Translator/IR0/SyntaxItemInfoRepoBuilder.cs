using Gum.CompileTime;
using System.Collections.Generic;
using S = Gum.Syntax;

namespace Gum.IR0
{
    class SyntaxItemInfoRepoBuilder
    {
        Dictionary<S.FuncDecl, FuncInfo> funcInfosByDecl;
        Dictionary<S.TypeDecl, TypeInfo> typeInfosByDecl;

        public SyntaxItemInfoRepoBuilder()
        {
            funcInfosByDecl = new Dictionary<S.FuncDecl, FuncInfo>();
            typeInfosByDecl = new Dictionary<S.TypeDecl, TypeInfo>();
        }

        public void AddTypeInfo(S.TypeDecl typeDecl, TypeInfo typeInfo)
        {
            typeInfosByDecl[typeDecl] = typeInfo;
        }

        public void AddFuncInfo(S.FuncDecl funcDecl, FuncInfo funcInfo)
        {
            funcInfosByDecl[funcDecl] = funcInfo;
        }

        public SyntaxItemInfoRepository Build()
        {
            return new SyntaxItemInfoRepository(funcInfosByDecl, typeInfosByDecl);
        }
    }
}