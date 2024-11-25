#pragma once

#include <memory>
#include <optional>

#include <IR0/RNames.h>
#include <IR0/RFuncReturn.h>
#include <IR0/RMember.h>

#include <IR0/NArgument.h>

namespace Citron {

class RDecl;
struct RFuncParameter;
class RFuncDeclOuter;
class RTypeFactory;

using RTypePtr = std::shared_ptr<class RType>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

class NLambdaDecl;
class NLambdaVarDecl;
class NFuncDeclOuter;
using NFuncDeclPtr = std::shared_ptr<class NFuncDecl>;

namespace SyntaxIR0Translator {

class CloneContext;
class UpdateContext;

using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ModuleDeclsPtr = std::shared_ptr<class ModuleDecls>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using ImExpPtr = std::shared_ptr<class ImExp>;

struct NLambdaVarAndArg
{
    std::shared_ptr<NLambdaVarDecl> var;
    NArgument arg;
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
    std::vector<std::shared_ptr<NLambdaDecl>> lambdaDecls;

public:
    FuncContext();
    std::shared_ptr<NLambdaVarDecl> StageLambdaVar(const RTypePtr& type, const RName& name, NArgument_Normal&& arg);

    virtual bool CanAccess(RDecl* target) = 0;
    virtual std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) = 0;

    // decl/body space의 return type을 리턴한다
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual void SetOpenFuncReturn(RTypePtr&& retType) = 0;
    virtual RTypeArgumentsPtr MakeOpenTypeArgs(RTypeFactory& factory) = 0;

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

public:
    FuncContext_Lambda(const ScopeContextPtr& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    bool CanAccess(RDecl* target) override;
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RTypePtr&& retType) override;

    RTypeArgumentsPtr MakeOpenTypeArgs(RTypeFactory& factory) override;

    bool IsSeqFunc() override;
};

// FuncDecl인 경우
class FuncContext_FuncDecl : public FuncContext
{
    NFuncDeclPtr funcDecl;

public:
    bool CanAccess(RDecl* target) override;
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    RFuncReturn GetUnboundFuncReturn() override;
    void SetOpenFuncReturn(RTypePtr&& retType) override;

    RTypeArgumentsPtr MakeOpenTypeArgs(RTypeFactory& factory) override;

    bool IsSeqFunc() override;
};

} // namespace SyntaxIR0Translator 

} // namespace Citron