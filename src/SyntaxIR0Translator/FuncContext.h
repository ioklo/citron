#pragma once

#include <memory>
#include <IR0/RNames.h>
#include <IR0/RFuncReturn.h>
#include <IR0/NArgument.h>

namespace Citron {

class NLambdaDecl;
class NLambdaMemberVarDecl;
class NFuncDecl;
class NFuncDeclOuter;

class RDecl;
struct RFuncParameter;
class RFuncDeclOuter;

using RTypePtr = std::shared_ptr<class RType>;
using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

namespace SyntaxIR0Translator {

class CloneContext;
class UpdateContext;

using FuncContextPtr = std::shared_ptr<class FuncContext>;
using ModuleDeclsPtr = std::shared_ptr<class ModuleDecls>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;
using ImExpPtr = std::shared_ptr<class ImExp>;

struct NLambdaDeclMemberVarAndArg
{
    std::shared_ptr<NLambdaMemberVarDecl> memberVarDecl;
    NArgument arg;
};

struct FuncContextOuter_ScopeContext 
{ 
    ScopeContextPtr scopeContext; 
    std::weak_ptr<FuncContext> funcContext;

    bool CanAccess(RDecl* target);
    NFuncDecl* GetOutermostFuncDecl();
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

struct FuncContextOuter_NFuncDeclOuter 
{ 
    std::shared_ptr<NFuncDeclOuter> decl; 

    bool CanAccess(RDecl* target);
    NFuncDecl* GetOutermostFuncDecl();
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);
};

using FuncContextOuter = std::variant<FuncContextOuter_ScopeContext, FuncContextOuter_NFuncDeclOuter>;

class FuncContext
{
public:
    ModuleDeclsPtr moduleDecls;

    // bodyContext에서 funcDecl을 미리 생성시키지 말고(일반 함수는 이미 생성되었겠지만 lambda의 경우는 생성되지 않았다), 마지막에 모은 정보로 만들도록 하자
    // outer를 갖고 있는 방향으로 가는 것이 낫겠다
    // 람다라면 outer는 scopeContext이고, funcDecl이었다면 RDecl일것이다
    FuncContextOuter outer;

    bool bSeqFunc;
    RFuncReturn funcReturn;
    std::vector<RFuncParameter> funcParams;
    bool bLastParamVariadic;
    RTypeArgumentsPtr openTypeArgs;

    // 람다 관련, funcDecl을 clone시키지 않으려고 funcDecl에 넣을 lambdaDecls들을 따로 보관하다가 마지막에 집어넣는다 (검색도 여기를 통해서 하기로 한다)
    // 이 함수가 람다일때 캡쳐할 멤버 변수에 대한 것
    std::vector<NLambdaDeclMemberVarAndArg> lambdaMemberVarAndInitArgs;
    // 이 함수가 갖고 있는 자식 lambda에 대한 것
    std::vector<std::shared_ptr<NLambdaDecl>> lambdaDecls;

public:
    FuncContext(const ModuleDeclsPtr& moduleDecls, FuncContextOuter&& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    FuncContextPtr MakeLambdaBodyContext(const std::shared_ptr<FuncContext>& sharedThis, const ScopeContextPtr& curScopeContext, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    // FuncContextPtr Clone(CloneContext& context);
    // void Update(const FuncContextPtr& src, UpdateContext& context);
    bool CanAccess(RDecl* target);

    NFuncDecl* GetOutermostFuncDecl();
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory);

    RFuncReturn GetFuncReturn();
    void SetFuncReturn(RTypePtr&& retType);
};

} // namespace SyntaxIR0Translator 

} // namespace Citron