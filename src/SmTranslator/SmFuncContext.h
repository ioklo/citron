#pragma once

#include <vector>
#include <memory>
#include <optional>
#include <expected>

#include "Infra/SmallMap.h"
#include "Logging/Diag.h"

#include "MIR/MArgument.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncReturn.h"
#include "BodyRes.h"

namespace Citron {

class RType;
class RDecl;
class RTypeDecl;
class RTypeArguments;

class RLambdaDecl;
class RLambdaVarDecl;

using RFactoryPtr = std::shared_ptr<class RFactory>;

struct MLoc_This;

struct RLambdaVarAndArg
{
    RLambdaVarDecl* var;
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
class SmFuncContext
{
    // 람다 관련, funcDecl을 clone시키지 않으려고 funcDecl에 넣을 lambdaDecls들을 따로 보관하다가 마지막에 집어넣는다 (검색도 여기를 통해서 하기로 한다)
    // 이 함수가 람다일때 캡쳐할 멤버 변수에 대한 것
    std::vector<RLambdaVarAndArg> lambdaVarAndInitArgs;

    // 이 함수가 갖고 있는 자식 lambda에 대한 것. lambda syntax를 처리한 후에 lambda에 해당하는 FuncContext를 통해 만들어 진다
    std::vector<RLambdaDecl*> lambdaDecls;
    RFactoryPtr rFactory;

private: // transaction
    struct TransactionInfo
    {
        size_t prevLambdaVarAndInitArgsCount; // 이전 상태의 lambdaVarAndInitArgs 크기
        size_t prevLambdaDeclsCount;
    };
    std::vector<TransactionInfo> transactionInfos;
    SmallMap<std::string, size_t> namedLabels;
    size_t labelCount;

public:
    SmFuncContext();
    RLambdaVarDecl* StageLambdaVar(RType* type, TakeRef<RName> name, MArgument&& arg);

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();

    size_t AddNewLabelId(std::optional<std::string>& o_label);
    std::optional<size_t> GetLabelId(const std::string& label);

    virtual void BeginTransaction_FuncContext() = 0;
    virtual void CommitTransaction_FuncContext() = 0;
    virtual void RollbackTransaction_FuncContext() = 0;

    virtual bool CanAccess(RDecl* target) = 0;
    virtual std::optional<RTypeRes> ResolveTypeIdentifier(InRef<RName> name) = 0;
    virtual std::expected<std::optional<BodyRes>, DiagPtr> ResolveIdentifier(InRef<RName> name) = 0;

    // decl/body space의 return type을 리턴한다
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual void SetOpenFuncReturn(RType* retType) = 0;
    virtual RTypeArguments* MakeOpenTypeArgs() = 0;

    virtual bool IsSeqFunc() = 0;
    virtual MLoc_This* MakeThisLoc() = 0;
};


} // namespace Citron