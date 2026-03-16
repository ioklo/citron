#include "RFuncAndRArgsToMExpTranslation.h"

#include <vector>
#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Diag.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RTypes.h"
#include "MIR/MExp.h"
#include "MIR/MInitExp.h"
#include "MIR/MStmt.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

class RFuncAndRArgsToMStmtTranslator
{
public:
    using ResultType = expected<MStmt*, DiagPtr>;
    
    RTypeArguments* typeArgs;
    MLoc* instance;
    vector<MArgument> args;
    TranslationContexts& contexts;

    template<typename TRFuncDecl> requires std::derived_from<TRFuncDecl, RFuncDecl>
    ResultType Call(TRFuncDecl* func, MCallable&& callable)
    {
        auto* retType = func->GetReturnType(typeArgs);

        switch (retType->GetCopyStrategy())
        {
        case RCopyStrategy::Void:
            return contexts.mFactory->MakeMStmt<MStmt_Call>(move(callable), move(args), /*o_catch*/nullopt);

        case RCopyStrategy::Bitwise:
        {
            auto* exp = contexts.mFactory->MakeMExp<MExp_Call>(move(callable), move(args), /*o_catch*/nullopt);
            return contexts.mFactory->MakeMStmt<MStmt_Exp>(MCreate_BC{exp});
        }

        case RCopyStrategy::NonBitwise:
        {
            auto* initExp = contexts.mFactory->MakeMInitExp<MInitExp_Call>(move(callable), move(args), /*o_catch*/nullopt);
            return contexts.mFactory->MakeMStmt<MStmt_Exp>(MCreate_NBC{initExp});
        }

        }

        unreachable();
    }

    ResultType Visit(RGlobalFuncDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RClassCtorDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RClassFuncDecl* func) 
    {
        return Call(func, MCallable_ClassFunc{func, typeArgs, instance});
    }

    ResultType Visit(RStructCtorDecl* func) 
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RStructDtorDecl* func)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(RStructFuncDecl* func) 
    {   
        return Call(func, MCallable_StructFunc{func, typeArgs, instance});
    }

    ResultType Visit(RLambdaDecl* func) 
    {
        throw NotImplementedException{};
    }
};

} // namespace Citron

expected<MStmt*, DiagPtr> TranslateRFuncAndNArgsToMStmt(RFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args, TranslationContexts& contexts)
{
    RFuncAndRArgsToMStmtTranslator binder{typeArgs, instance, move(args), contexts};
    return Accept(binder, decl);
}

}
