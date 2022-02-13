using Citron.Infra;
using Citron.Collections;
using Pretune;

namespace Citron.IR0
{   
    public record ClassMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names);
    
    
    [AutoConstructor]
    public partial struct ConstructorBaseCallInfo
    {
        public ParamHash ParamHash { get; }
        public ImmutableArray<Argument> Args { get; }
    }

    // public S(int a, int b) { }
    public record ClassConstructorDecl(
        AccessModifier AccessModifier,
        ImmutableArray<CallableMemberDecl> CallableMemberDecls,
        ImmutableArray<Param> Parameters,
        ConstructorBaseCallInfo? BaseCallInfo,
        Stmt Body
    );
}
