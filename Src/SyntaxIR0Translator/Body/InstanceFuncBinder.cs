using System;
using Citron.Symbol;
using Citron.Collections;
using Citron.IR0;

namespace Citron.Analysis;

// 소스코드에서 마치 x.'Func(a, b)' 한거 같은 효과? 그럼 그냥 CallBinder를 쓰지?
// TODO: [12] Extension 함수 검색
struct InstanceFuncBinder
    : ITypeVisitor<TranslationResult<IR0ExpResult>>
    , ISymbolQueryResultVisitor<TranslationResult<IR0ExpResult>>
{
    IDeclSymbolNode curNode;
    Loc instance;
    Name name;
    ImmutableArray<IType> explicitTypeArgs;
    ImmutableArray<Argument> args;

    public static TranslationResult<IR0ExpResult> Bind(IDeclSymbolNode curNode, Loc instance, IType instanceType, Name name, ImmutableArray<IType> explicitTypeArgs, ImmutableArray<Argument> args)
    {
        var member = instanceType.QueryMember(name, explicitTypeArgs.Length);
        if (member == null) return Error();

        var binder = new InstanceFuncBinder { curNode = curNode, instance = instance, explicitTypeArgs = explicitTypeArgs, args = args };
        return member.Accept<InstanceFuncBinder, TranslationResult<IR0ExpResult>>(ref binder);
        
        return instanceType.Accept<InstanceFuncBinder, TranslationResult<IR0ExpResult>>(ref binder);
    }

    static TranslationResult<IR0ExpResult> Error() { return TranslationResult.Error<IR0ExpResult>(); }
    static TranslationResult<IR0ExpResult> Valid(IR0ExpResult exp) { return TranslationResult.Valid(exp); }

    TranslationResult<IR0ExpResult> HandleError() { throw new NotImplementedException(); }

    // TODO: [12] 아래 모든 타입들은 extension이 있으면 사용가능하게 된다
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitBoxPtr(BoxPtrType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitEnum(EnumType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitEnumElem(EnumElemType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitLambda(LambdaType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitLocalPtr(LocalPtrType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitTuple(TupleType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitVoid(VoidType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitFunc(FuncType type) => HandleError();
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitNullable(NullableType type) => HandleError();

    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitTypeVar(TypeVarType type) => throw new NotImplementedException();

    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitClass(ClassType type)
    {
        // x.GetEnumerator() 호출
        var classMemberFunc = type.GetMemberFunc(name, typeArgs, paramIds);
        if (classMemberFunc == null)
        {   
            // TODO: [13] interface 구현
            // TODO: [14] 임의의 타입에서 멤버함수를 호출하는 코드 에러 처리
            throw new NotImplementedException();
        }

        if (!curNode.CanAccess(classMemberFunc.GetDeclSymbolNode()))
        {
            // TODO: [14] 임의의 타입에서 멤버함수를 호출하는 코드 에러 처리
            throw new NotImplementedException();
        }

        return Valid(new IR0ExpResult(new CallClassMemberFuncExp(classMemberFunc, instance, args), classMemberFunc.GetReturn().Type));
    }

    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitInterface(InterfaceType type)
    {
        // TODO: [13] interface 구현
        throw new NotImplementedException();
    }
        
    TranslationResult<IR0ExpResult> ITypeVisitor<TranslationResult<IR0ExpResult>>.VisitStruct(StructType type)
    {
        var structMemberFunc = type.GetMemberFunc(name, typeArgs: default, paramIds: default);
        if (structMemberFunc == null)
        {
            // TODO: [12] Extension 함수 검색
            // TODO: [14] 임의의 타입에서 멤버함수를 호출하는 코드 에러 처리
            throw new NotImplementedException(); 
        }

        if (!curNode.CanAccess(structMemberFunc.GetDeclSymbolNode()))
        {
            // TODO: [14] 임의의 타입에서 멤버함수를 호출하는 코드 에러 처리
            throw new NotImplementedException();
        }

        return Valid(new IR0ExpResult(new CallStructMemberFuncExp(structMemberFunc, instance, args), structMemberFunc.GetReturn().Type));
    }
        
}