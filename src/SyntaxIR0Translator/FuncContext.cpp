 #include "pch.h"
#include "FuncContext.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Variants.h>
#include <Infra/Exceptions.h>

#include <Syntax/Syntax.h>

#include <IR0/NLambdaMemberVarDecl.h>
#include <IR0/NArgument.h>
#include <IR0/NFuncDecl.h>
#include <IR0/NFuncDeclOuter.h>
#include <IR0/NDecl.h>
#include <IR0/RTypeArguments.h>
#include <IR0/NExp.h>

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "ImExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

FuncContext::FuncContext() = default;

shared_ptr<NLambdaMemberVarDecl> FuncContext::StageLambdaMemberVar(const RTypePtr& type, const RName& name, NArgument_Normal&& arg)
{
    auto lambdaMemberVar = MakePtr<NLambdaMemberVarDecl>(type, name);
    lambdaMemberVarAndInitArgs.emplace_back(lambdaMemberVar, std::move(arg));
    return lambdaMemberVar;
}

FuncContext_Lambda::FuncContext_Lambda(const ScopeContextPtr& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : outer(outer), bSeqFunc(bSeqFunc), funcReturn(std::move(funcReturn)), funcParams(std::move(funcParams)), bLastParamVariadic(bLastParamVariadic)
{
}

bool FuncContext_Lambda::CanAccess(RDecl* target)
{
    return outer->funcContext->CanAccess(target);
}

optional<RMember> FuncContext_Lambda::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    auto oMember = outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
    if (!oMember) return nullopt;
    
    // 상위 스코프에서 얻어오는 
    // 로컬과 람다 멤버, this만 감싸는 대상이다
    if (auto* localVar = get_if<RMember_LocalVar>(&*oMember))
    {
        RName localVarName = RName_Normal(localVar->name);

        auto initExp = MakePtr<NExp_Load>(MakePtr<NLoc_LocalVar>(localVarName, localVar->type));
        auto initArg = NArgument_Normal(std::move(initExp));

        auto lambdaMemberVar = StageLambdaMemberVar(localVar->type, localVarName, std::move(initArg));

        auto openTypeArgs = MakeOpenTypeArgs(factory);
        return RMember_LambdaMemberVar(std::move(openTypeArgs), std::move(lambdaMemberVar));
    }

    if (auto* lambdaMemberVar = get_if<RMember_LambdaMemberVar>(&*oMember))
    {
        // class C<T> { void F<S> {
        //     List<T> x;      // 5) scopeContext.ResolveIdentifier(x, 0) => RMember
        //     var f = () => { // 4) funcContext.ResolveIdentifier(x, 0)
        //         // 3) scopeContext.ResolveIdentifier(x, 0)
        //     
        //         var g = () => { // 2) funcContext.ResolveIdentifier(x, 0)
        //
        //             // 1) 여기에서 scopeContext.ResolveIdentifier(x, 0) 호출
        //             x; 
        //     
        //         }
        //     }
        // } }

        auto openTypeArgs = MakeOpenTypeArgs(factory);
        auto initArg = NArgument_Normal(MakePtr<NExp_Load>(MakePtr<NLoc_LambdaMemberVar>(lambdaMemberVar->decl, openTypeArgs)));

        auto newLambdaMemberVar = StageLambdaMemberVar(lambdaMemberVar->decl->GetUnboundDeclType(), lambdaMemberVar->decl->name, std::move(initArg));
        return RMember_LambdaMemberVar(std::move(openTypeArgs), std::move(newLambdaMemberVar));
    }

    if (auto* thisVar = get_if<RMember_ThisVar>(&*oMember))
    {
        // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
        if (auto structType = dynamic_cast<RType_Struct*>(thisVar->type.get()))
            throw NotImplementedException();

        auto initExp = MakePtr<NExp_Load>(MakePtr<NLoc_This>(thisVar->type));
        auto initArg = NArgument_Normal(std::move(initExp));

        auto lambdaMemberVar = StageLambdaMemberVar(thisVar->type, RNames::_this, std::move(initArg));
        auto openTypeArgs = MakeOpenTypeArgs(factory);

        return RMember_LambdaMemberVar(std::move(openTypeArgs), std::move(lambdaMemberVar));
    }

    // 나머지는 그대로 리턴
    return oMember;
}

RFuncReturn FuncContext_Lambda::GetUnboundFuncReturn()
{
    return funcReturn;
}

void FuncContext_Lambda::SetOpenFuncReturn(RTypePtr&& retType)
{
    assert(holds_alternative<RFuncReturn_NotSet>(funcReturn));
    funcReturn = std::move(RFuncReturn_Set(retType));
}

RTypeArgumentsPtr FuncContext_Lambda::MakeOpenTypeArgs(RTypeFactory& factory)
{
    return outer->MakeOpenTypeArgs(factory);
}

bool FuncContext_Lambda::IsSeqFunc()
{
    return bSeqFunc;
}

bool FuncContext_FuncDecl::CanAccess(RDecl* target)
{
    return funcDecl->GetNDecl()->GetRDecl()->CanAccess(target);
}

optional<RMember> FuncContext_FuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    return funcDecl->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

RFuncReturn FuncContext_FuncDecl::GetUnboundFuncReturn()
{
    return funcDecl->GetUnboundFuncReturn();
}

void FuncContext_FuncDecl::SetOpenFuncReturn(RTypePtr&& retType)
{
    throw RuntimeFatalException();
}


RTypeArgumentsPtr FuncContext_FuncDecl::MakeOpenTypeArgs(RTypeFactory& factory)
{
    return funcDecl->GetNDecl()->GetRDecl()->MakeOpenTypeArgs(factory);
}

bool FuncContext_FuncDecl::IsSeqFunc()
{
    return funcDecl->IsSeqFunc();
}

//public void CommitLambdasToDeclSymbolTree()
//{
//    foreach(var lambda in lambdaDs)
//        funcDeclSymbol.AddLambda(lambda);

//    lambdaDs = default;
//}

//public ImmutableArray<R.Argument> MakeLambdaArgs()
//{
//    return lambdaMemberVarInitArgs;
//}

//// 리턴값 관련 
//public bool IsSetReturn()
//{
//    return bSetReturn;
//}

//// constructor라면 null
//public FuncReturn ? GetReturn()
//{
//    Debug.Assert(bSetReturn);
//    return funcReturn;
//}

//public void SetReturn(IType retType)
//{
//    bSetReturn = true;
//    funcReturn = new FuncReturn(retType);
//}


} // namespace Citron::SyntaxIR0Translator
