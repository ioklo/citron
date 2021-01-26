namespace Gum.CompileTime
{
    // Module
    //   - Namespace
    //       - MemberType
    //           - Member Type
    //           - Member Func
    //           - Member Var
    //       - MemberFunc
    //       - MemberVar

    // 모듈이 갖고 있는 TypeInfo, FuncInfo, VarInfo를 가리킨다
    public abstract class ItemInfo
    {
        public abstract Name Name { get; }
    }
}