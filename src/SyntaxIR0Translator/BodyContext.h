#pragma once

#include <memory>
#include <IR0/RNames.h>
#include <IR0/RFuncReturn.h>
#include <IR0/RArgument.h>

namespace Citron {

class RLambdaDecl;
class RLambdaMemberVarDecl;
class RDecl;

struct RFuncParameter;
class RFuncDeclOuter;

using RTypePtr = std::shared_ptr<class RType>;
using RFuncDeclPtr = std::shared_ptr<class RFuncDecl>;

namespace SyntaxIR0Translator {

class CloneContext;
class UpdateContext;


using BodyContextPtr = std::shared_ptr<class BodyContext>;
using ModuleDeclsPtr = std::shared_ptr<class ModuleDecls>;
using ScopeContextPtr = std::shared_ptr<class ScopeContext>;

struct RLambdaDeclMemberVarAndArg
{
    std::shared_ptr<RLambdaMemberVarDecl> memberVarDecl;
    RArgument arg;
};

struct BodyContextOuter_ScopeContext { ScopeContextPtr scopeContext; };
struct BodyContextOuter_RFuncDeclOuter { std::shared_ptr<RFuncDeclOuter> decl; };

using BodyContextOuter = std::variant<BodyContextOuter_ScopeContext, BodyContextOuter_RFuncDeclOuter>;

class BodyContext
{
public:
    ModuleDeclsPtr moduleDecls;

    // bodyContext에서 funcDecl을 미리 생성시키지 말고(일반 함수는 이미 생성되었겠지만 lambda의 경우는 생성되지 않았다), 마지막에 모은 정보로 만들도록 하자
    // outer를 갖고 있는 방향으로 가는 것이 낫겠다
    // 람다라면 outer는 scopeContext이고, funcDecl이었다면 RDecl일것이다
    BodyContextOuter outer;

    bool bSeqFunc;
    RFuncReturn funcReturn;
    std::vector<RFuncParameter> funcParams;
    bool bLastParamVariadic;

    // 람다 관련, funcDecl을 clone시키지 않으려고 funcDecl에 넣을 lambdaDecls들을 따로 보관하다가 마지막에 집어넣는다 (검색도 여기를 통해서 하기로 한다)
    // 이 함수가 람다일때 캡쳐할 멤버 변수에 대한 것
    std::vector<RLambdaDeclMemberVarAndArg> lambdaMemberVarAndInitArgs;
    // 이 함수가 갖고 있는 자식 lambda에 대한 것
    std::vector<std::shared_ptr<RLambdaDecl>> lambdaDecls;

public:
    BodyContext(const ModuleDeclsPtr& moduleDecls, BodyContextOuter&& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    BodyContextPtr MakeLambdaBodyContext(const ScopeContextPtr& curScopeContext, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic);

    BodyContextPtr Clone(CloneContext& context);
    void Update(const BodyContextPtr& src, UpdateContext& context);
    bool CanAccess(RDecl* target);

    RFuncDecl* GetOutermostFuncDecl();

    RFuncReturn GetFuncReturn();
    void SetFuncReturn(RTypePtr&& retType);
};

} // namespace SyntaxIR0Translator 

} // namespace Citron