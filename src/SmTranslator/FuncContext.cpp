#include "FuncContext.h"

#include <variant>
#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Variants.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTypes.h"

#include "RSymbol/RLambdaDecl.h"
#include "RSymbol/RLambdaVarDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RFuncDeclOuter.h"

#include "MIR/MExp.h"
#include "MIR/MArgument.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "ImExp.h"

using namespace std;

namespace Citron {

FuncContext::FuncContext()
    : labelCount{0}
{
}

RLambdaVarDecl* FuncContext::StageLambdaVar(RType* type, TakeRef<RName> name, MArgument&& arg)
{
    auto* lambdaVar = rFactory->MakeDecl<RLambdaVarDecl>(type, move(name));
    lambdaVarAndInitArgs.emplace_back(lambdaVar, move(arg));
    return lambdaVar;
}

void FuncContext::BeginTransaction()
{
    transactionInfos.emplace_back(lambdaVarAndInitArgs.size(), lambdaDecls.size());
    BeginTransaction_FuncContext();
}

void FuncContext::CommitTransaction()
{
    // prev 마킹 방식은 그대로 두면 된다
    transactionInfos.pop_back();

    CommitTransaction_FuncContext();
}

void FuncContext::RollbackTransaction()
{
    auto& transactionInfo = transactionInfos.back();
    lambdaVarAndInitArgs.resize(transactionInfo.prevLambdaVarAndInitArgsCount);
    lambdaDecls.resize(transactionInfo.prevLambdaDeclsCount);
    transactionInfos.pop_back();

    RollbackTransaction_FuncContext();
}

size_t FuncContext::AddNewLabelId(std::optional<std::string>& o_label)
{
    size_t newId = labelCount++;

    if (o_label)
        namedLabels.Add(*o_label, newId);

    return newId;
}

std::optional<size_t> FuncContext::GetLabelId(const std::string& label)
{
    if (auto* id = namedLabels.Find(label))
        return *id;
    else
        return std::nullopt;
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


} // namespace Citron
