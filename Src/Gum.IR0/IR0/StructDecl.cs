using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;
using Pretune;

namespace Citron.IR0
{
    public partial record StructDecl(
        AccessModifier AccessModifier,
        string Name,
        ImmutableArray<string> TypeParams,
        ImmutableArray<Path> BaseTypes,
        ImmutableArray<StructConstructorDecl> ConstructorDecls,
        ImmutableArray<FuncDecl> MemberFuncDecls,
        ImmutableArray<StructMemberVarDecl> MemberVarDecls
    ) : TypeDecl;
    
}
