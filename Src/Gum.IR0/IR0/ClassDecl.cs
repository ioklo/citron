using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Infra;

namespace Citron.IR0
{
    public partial record ClassDecl(
        AccessModifier AccessModifier,
        string Name,
        ImmutableArray<string> TypeParams,
        Path.Nested? BaseClass,
        ImmutableArray<Path.Nested> Interfaces,        
        ImmutableArray<ClassConstructorDecl> ConstructorDecls,
        ImmutableArray<FuncDecl> MemberFuncDecls,
        ImmutableArray<ClassMemberVarDecl> MemberVarDecls
    ) : TypeDecl
    {   
    }
}
