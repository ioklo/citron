using System;
using Citron.Symbol;
using Citron.Collections;
using Citron.IR0;

namespace Citron.Analysis;

using Result = TranslationResult<IR0ExpResult>;
using ITV = ITypeVisitor<TranslationResult<IR0ExpResult>>;
using ISQRV = ISymbolQueryResultVisitor<TranslationResult<IR0ExpResult>>;
using SQR = SymbolQueryResult;

// 소스코드에서 마치 x.'Func(a, b)' 한거 같은 효과? 그럼 그냥 CallBinder를 쓰지?
// TODO: [12] Extension 함수 검색
struct InstanceFuncBinder : ITV, ISQRV
{
    IDeclSymbolNode curNode;
    Loc instance;
    Name name;
    ImmutableArray<IType> explicitTypeArgs;
    ImmutableArray<Argument> args;

    public static Result Bind(IDeclSymbolNode curNode, Loc instance, IType instanceType, Name name, ImmutableArray<IType> explicitTypeArgs, ImmutableArray<Argument> args)
    {
        var member = instanceType.QueryMember(name, explicitTypeArgs.Length);
        if (member == null) return Error();

        var binder = new InstanceFuncBinder { curNode = curNode, instance = instance, explicitTypeArgs = explicitTypeArgs, args = args };
        return member.Accept<InstanceFuncBinder, Result>(ref binder);
        
        return instanceType.Accept<InstanceFuncBinder, Result>(ref binder);
    }

    static Result Error() { return TranslationResult.Error<IR0ExpResult>(); }
    static Result Valid(IR0ExpResult exp) { return TranslationResult.Valid(exp); }

    Result HandleError() { throw new NotImplementedException(); }

    // TODO: [12] 아래 모든 타입들은 extension이 있으면 사용가능하게 된다
    Result ITV.VisitBoxPtr(BoxPtrType type) => HandleError();
    Result ITV.VisitEnum(EnumType type) => HandleError();
    Result ITV.VisitEnumElem(EnumElemType type) => HandleError();
    Result ITV.VisitLambda(LambdaType type) => HandleError();
    Result ITV.VisitLocalPtr(LocalPtrType type) => HandleError();
    Result ITV.VisitTuple(TupleType type) => HandleError();
    Result ITV.VisitVoid(VoidType type) => HandleError();
    Result ITV.VisitFunc(FuncType type) => HandleError();
    Result ITV.VisitNullable(NullableType type) => HandleError();

    Result ITV.VisitTypeVar(TypeVarType type) => throw new NotImplementedException();

    Result ITV.VisitClass(ClassType type)
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

    Result ITV.VisitInterface(InterfaceType type)
    {
        // TODO: [13] interface 구현
        throw new NotImplementedException();
    }
        
    Result ITV.VisitStruct(StructType type)
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

    Result ISQRV.VisitMultipleCandidatesError(SQR.MultipleCandidatesError result) => HandleError();
    Result ISQRV.VisitNamespace(SQR.Namespace result) => HandleError();
    Result ISQRV.VisitGlobalFuncs(SQR.GlobalFuncs result) => HandleError();
    Result ISQRV.VisitClass(SQR.Class result) => HandleError();

    Result ISQRV.VisitClassMemberFuncs(SQR.ClassMemberFuncs result)
    {
        
    }

    Result ISQRV.VisitClassMemberVar(SQR.ClassMemberVar result) => HandleError();

    Result ISQRV.VisitStruct(SQR.Struct result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitStructMemberFuncs(SQR.StructMemberFuncs result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitStructMemberVar(SQR.StructMemberVar result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitEnum(SQR.Enum result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitEnumElem(SQR.EnumElem result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitEnumElemMemberVar(SQR.EnumElemMemberVar result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitLambdaMemberVar(SQR.LambdaMemberVar result)
    {
        throw new NotImplementedException();
    }

    Result ISQRV.VisitTupleMemberVar(SQR.TupleMemberVar result)
    {
        throw new NotImplementedException();
    }
}