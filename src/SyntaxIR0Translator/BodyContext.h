#pragma once

#include <memory>
#include <IR0/RNames.h>
#include <IR0/RFuncReturn.h>
#include <IR0/RArgument.h>

namespace Citron {

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

class BodyContext
{
public:
    ModuleDeclsPtr moduleDecls;
    RFuncDeclPtr funcDecl; // decl space
    bool bSeqFunc;
    RFuncReturn funcReturn;
    ScopeContextPtr outerScopeContext; // 람다라면, 람다를 선언한 Scope

    // 람다 관련, funcDecl을 clone시키지 않으려고 funcDecl에 넣을 lambdaDecls들을 따로 보관하다가 마지막에 집어넣는다 (검색도 여기를 통해서 하기로 한다)
    // 이 함수가 람다일때 캡쳐할 멤버 변수에 대한 것
    std::vector<RLambdaDeclMemberVarAndArg> lambdaMemberVarAndInitArgs;
    // 이 함수가 갖고 있는 자식 lambda에 대한 것
    std::vector<RLambdaDecl> lambdaDecls;

public:
    BodyContext(const ModuleDeclsPtr& moduleDecls, const RFuncDeclPtr& funcDecl, bool bSeqFunc, const RFuncReturn& funcReturn, const ScopeContextPtr& outerScopeContext, const RTypeFactoryPtr& factory);
    BodyContextPtr Clone(CloneContext& context);
    void Update(const BodyContextPtr& src, UpdateContext& context);
    bool CanAccess(RDecl* target);

    RFuncDecl* GetOutermostFuncDecl();

    RFuncReturn GetFuncReturn();
    void SetFuncReturn(RTypePtr&& retType);
};

} // namespace SyntaxIR0Translator 

} // namespace Citron