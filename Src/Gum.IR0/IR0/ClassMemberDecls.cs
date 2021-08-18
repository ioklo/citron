using Gum.Infra;
using Gum.Collections;
using Pretune;

namespace Gum.IR0
{   
    public record ClassMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names);

    // TODO: AccessModifier
    public record ClassMemberFuncDecl(FuncDecl FuncDecl);
    
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
