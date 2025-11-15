#pragma once

#include <vector>
#include <memory>
#include <optional>

#include "MIR/MArgument.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RMember.h"

namespace Citron {

class RType;
class RDecl;
class RFactory;
using RFactoryPtr = std::shared_ptr<RFactory>;
class RTypeArguments;
struct RFuncParameter;

class NLambdaDecl;
class NLambdaVarDecl;
class NFuncDecl;
class NFactory;
using NFactoryPtr = std::shared_ptr<NFactory>;

class MFactory;
using MFactoryPtr = std::shared_ptr<MFactory>;

namespace SyntaxIR0Translator {

class ScopeContext;
using ScopeContextPtr = std::shared_ptr<ScopeContext>;

struct NLambdaVarAndArg
{
    NLambdaVarDecl* var;
    MArgument arg;
};

// outer가 1) scopeContext인 경우 (람다)
//         2) FuncDeclOuter 경우  (함수)
// 본체가  1) 람다 인경우 (람다)
//         2) FuncDecl인 경우 (함수)

// _Lambda
// _FuncDecl

// funcContext에서 funcDecl을 미리 생성시키지 말고(일반 함수는 이미 생성되었겠지만 lambda의 경우는 생성되지 않았다), 마지막에 모은 정보로 만들도록 하자
// outer를 갖고 있는 방향으로 가는 것이 낫겠다
// 람다라면 outer는 scopeContext이고, funcDecl이었다면 RDecl일것이다
class FuncContext
{
    // 람다 관련, funcDecl을 clone시키지 않으려고 funcDecl에 넣을 lambdaDecls들을 따로 보관하다가 마지막에 집어넣는다 (검색도 여기를 통해서 하기로 한다)
    // 이 함수가 람다일때 캡쳐할 멤버 변수에 대한 것
    std::vector<NLambdaVarAndArg> lambdaVarAndInitArgs;

    // 이 함수가 갖고 있는 자식 lambda에 대한 것. lambda syntax를 처리한 후에 lambda에 해당하는 FuncContext를 통해 만들어 진다
    std::vector<NLambdaDecl*> lambdaDecls;

    NFactoryPtr nFactory;

public:
    FuncContext();
    NLambdaVarDecl* StageLambdaVar(RType* type, const RName& name, MArgument_Normal&& arg);

    virtual bool CanAccess(RDecl* target) = 0;
    virtual std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) = 0;

    // decl/body space의 return type을 리턴한다
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual void SetOpenFuncReturn(RType* retType) = 0;
    virtual RTypeArguments* MakeOpenTypeArgs() = 0;

    virtual bool IsSeqFunc() = 0;

    // virtual FuncContextPtr Clone(CloneContext& context) = 0;
    // virtual void Update(const FuncContextPtr& src, UpdateContext& context) = 0;
};

// 람다인 경우
class FuncContext_Lambda : public FuncContext
{
    ScopeContextPtr outer;
    bool bSeqFunc; // reserved
    RFuncReturn funcReturn;
    std::vector<RFuncParameter> funcParams;
    bool bLastParamVariadic;

    MFactoryPtr mFactory;

public:
    FuncContext_Lambda(const ScopeContextPtr& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    bool CanAccess(RDecl* target) override;
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
};

// FuncDecl인 경우
class FuncContext_FuncDecl : public FuncContext
{
    NFuncDecl* nFuncDecl;
    RFactoryPtr rFactory;

public:
    FuncContext_FuncDecl(NFuncDecl* funcDecl, const RFactoryPtr& rFactory);

    bool CanAccess(RDecl* target) override;
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RType* retType) override;

    RTypeArguments* MakeOpenTypeArgs() override;

    bool IsSeqFunc() override;
};

} // namespace SyntaxIR0Translator 
} // namespace Citron