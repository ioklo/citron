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

#include "NSymbol/NLambdaDecl.h"
#include "NSymbol/NFactory.h"

#include "NSymbol/NStructDecl.h"
#include "NSymbol/NStructCtorDecl.h"
#include "NSymbol/NStructDtorDecl.h"
#include "NSymbol/NStructFuncDecl.h"
#include "NSymbol/NNamespaceDecl.h"
#include "NSymbol/NGlobalFuncDecl.h"
#include "NSymbol/NClassDecl.h"
#include "NSymbol/NClassCtorDecl.h"
#include "NSymbol/NClassFuncDecl.h"
#include "NSymbol/NFuncDeclOuter.h"

#include "MIR/MExp.h"
#include "MIR/MArgument.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "ImExp.h"

using namespace std;

namespace Citron {

FuncContext::FuncContext() = default;

NLambdaVarDecl* FuncContext::StageLambdaVar(RType* type, const RName& name, MArgument&& arg)
{
    auto* lambdaVar = nFactory->MakeNDecl<NLambdaVarDecl>(type, name);
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
