#include "FuncContext.h"

#include <variant>
#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Variants.h"
#include "Infra/Exceptions.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypes.h"

#include "NSymbol/NLambdaDecl.h"
#include "NSymbol/NFactory.h"

#include "MIR/MExp.h"
#include "MIR/MArgument.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "ImExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

FuncContext::FuncContext() = default;

NLambdaVarDecl* FuncContext::StageLambdaVar(RType* type, const RName& name, MArgument_Normal&& arg)
{
    auto* lambdaVar = nFactory->MakeNDecl<NLambdaVarDecl>(type, name);
    lambdaVarAndInitArgs.emplace_back(lambdaVar, move(arg));
    return lambdaVar;
}

FuncContext_Lambda::FuncContext_Lambda(const ScopeContextPtr& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : outer(outer), bSeqFunc(bSeqFunc), funcReturn(move(funcReturn)), funcParams(move(funcParams)), bLastParamVariadic(bLastParamVariadic)
{
}

bool FuncContext_Lambda::CanAccess(RDecl* target)
{
    return outer->funcContext->CanAccess(target);
}

optional<RMember> FuncContext_Lambda::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto oMember = outer->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
    if (!oMember) return nullopt;
    
    // 상위 스코프에서 얻어오는 
    // 로컬과 람다 멤버, this만 감싸는 대상이다
    if (auto* localVar = get_if<RMember_LocalVar>(&*oMember))
    {
        RName localVarName = RName_Normal(localVar->name);

        auto* localVarLoc = mFactory->MakeMLoc<MLoc_LocalVar>(localVarName, localVar->type);
        auto* initExp = mFactory->MakeMExp<MExp_Load>(localVarLoc);
        auto initArg = MArgument_Normal(initExp);

        auto* lambdaVar = StageLambdaVar(localVar->type, localVarName, move(initArg));

        auto* openTypeArgs = MakeOpenTypeArgs();
        return RMember_LambdaVar(openTypeArgs, lambdaVar);
    }

    if (auto* lambdaVar = get_if<RMember_LambdaVar>(&*oMember))
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

        auto openTypeArgs = MakeOpenTypeArgs();
        auto* lambdaVarDecl = mFactory->MakeMLoc<MLoc_LambdaVar>(lambdaVar->decl, openTypeArgs);
        auto* loadExp = mFactory->MakeMExp<MExp_Load>(lambdaVarDecl);
        MArgument_Normal initArg{loadExp};

        auto* newLambdaVar = StageLambdaVar(lambdaVar->decl->GetUnboundDeclType(), lambdaVar->decl->GetName(), move(initArg));
        return RMember_LambdaVar{openTypeArgs, newLambdaVar};
    }

    if (auto* thisVar = get_if<RMember_ThisVar>(&*oMember))
    {
        // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
        if (auto structType = dynamic_cast<RType_Struct*>(thisVar->type))
            throw NotImplementedException{};

        auto* thisLoc = mFactory->MakeMLoc<MLoc_This>(thisVar->type);
        auto* initExp = mFactory->MakeMExp<MExp_Load>(thisLoc);
        auto initArg = MArgument_Normal{initExp};

        auto* lambdaVar = StageLambdaVar(thisVar->type, RNames::_this, move(initArg));
        auto* openTypeArgs = MakeOpenTypeArgs();

        return RMember_LambdaVar(openTypeArgs, lambdaVar);
    }

    // 나머지는 그대로 리턴
    return oMember;
}

RFuncReturn FuncContext_Lambda::GetUnboundFuncReturn()
{
    return funcReturn;
}

void FuncContext_Lambda::SetOpenFuncReturn(RType* retType)
{
    assert(holds_alternative<RFuncReturn_NotSet>(funcReturn));
    funcReturn = RFuncReturn_Set{retType};
}

RTypeArguments* FuncContext_Lambda::MakeOpenTypeArgs()
{
    return outer->MakeOpenTypeArgs();
}

bool FuncContext_Lambda::IsSeqFunc()
{
    return bSeqFunc;
}

bool FuncContext_FuncDecl::CanAccess(RDecl* target)
{
    return funcDecl->GetNDecl()->GetRDecl()->CanAccess(target);
}

optional<RMember> FuncContext_FuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return funcDecl->GetNDecl()->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, *rFactory);
}

RFuncReturn FuncContext_FuncDecl::GetUnboundFuncReturn()
{
    return funcDecl->GetUnboundFuncReturn();
}

void FuncContext_FuncDecl::SetOpenFuncReturn(RType* retType)
{
    throw RuntimeFatalException{};
}


RTypeArguments* FuncContext_FuncDecl::MakeOpenTypeArgs()
{
    return funcDecl->GetNDecl()->GetRDecl()->MakeOpenTypeArgs(*rFactory);
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
