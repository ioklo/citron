#include "BodyResToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "IrExp.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"
#include "FuncContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {

struct DeclResAndMemberTypeArgsToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;

    template<typename TIrExp, typename... TArgs> requires derived_from<TIrExp, IrExp>
    IrExp* MakeIrExp(TArgs&&... args)
    {
        return contexts.srtFactory->MakeIrExp<TIrExp>(forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    MLoc* MakeMLoc(TArgs&&... args)
    {
        return contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
    }
    
    ResultType operator()(auto& declRes) { return Visit(declRes); }

    ResultType Visit(auto& declRes)
    {
        return Error<Error_SharedTranslation_CantMakeSharedFromBase>();
    }

    ResultType Visit(RDeclRes_Namespace& declRes) 
    { 
        assert(memberTypeArgs->GetCount() == 0);
        return MakeIrExp<IrExp_Namespace>(declRes.decl);
    }

    // ResultType Visit(RDeclRes_GlobalFuncs& declRes); 함수류는 IrExp에서 관심없다

    ResultType Visit(RDeclRes_Class& declRes)
    {
        auto* typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerTypeArgs, memberTypeArgs);
        return MakeIrExp<IrExp_Class>(declRes.decl, typeArgs);
    }

    // ResultType Visit(RDeclRes_ClassFuncs& declRes);
    ResultType Visit(RDeclRes_ClassVar& declRes) 
    {   
        assert(memberTypeArgs->GetCount() == 0);

        if (declRes.decl->IsStatic()) // &C.x
        {
            auto* loc = MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, declRes.decl, declRes.typeArgs);
            return MakeIrExp<IrExp_Static>(loc);
        }
        else // &this.x
        {   
            return MakeIrExp<IrExp_ClassVar>(
                contexts.funcContext->MakeThisLoc(), declRes.decl, declRes.typeArgs);
        }
    }

    ResultType Visit(RDeclRes_Struct& declRes)
    { 
        auto* typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerTypeArgs, memberTypeArgs);
        return MakeIrExp<IrExp_Struct>(declRes.decl, typeArgs);
    }

    // ResultType Visit(RDeclRes_StructFuncs& declRes);

    ResultType Visit(RDeclRes_StructVar& declRes) 
    {
        assert(memberTypeArgs->GetCount() == 0);

        if (declRes.decl->IsStatic())
        {
            auto* loc = MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, declRes.decl, declRes.typeArgs);
            return MakeIrExp<IrExp_Static>(loc);
        }
        else
        {
            // TODO: [10] shared함수이면 this를 shared로 판단해야 한다
            // 지금은 this의 타입이 S&이다.

            // IrExp_StructVar는 base가 IrExp인 경우(sharedExp로 보일수 있는 가능성)에만 만드는것이다.

            auto* loc = MakeMLoc<MLoc_StructVar>(contexts.funcContext->MakeThisLoc(), declRes.decl, declRes.typeArgs);
            return MakeIrExp<IrExp_Loc>(loc);
        }
    }
    // ResultType Visit(RDeclRes_Enum& declRes);
    // ResultType Visit(RDeclRes_EnumElem& declRes);
    // ResultType Visit(RDeclRes_EnumElemVar& declRes);
    // ResultType Visit(RDeclRes_LambdaVar& declRes);
    // ResultType Visit(RDeclRes_TupleVar& declRes);
    // ResultType Visit(RDeclRes_TypeVar& declRes);
    ResultType Visit(RDeclRes_FuncParam& declRes) 
    {
        MLoc* loc;
        if (declRes.funcParam.IsRef())
            loc = MakeMLoc<MLoc_LocalRef>(declRes.funcParam.name, declRes.funcParam.type);
        else
            loc = MakeMLoc<MLoc_LocalVar>(declRes.funcParam.name, declRes.funcParam.type);

        // IrExp자체는 Loc에 관심이 없다. (SharedDeref 빼고)
        return MakeIrExp<IrExp_Loc>(loc);
    }
};

struct BodyResAndMemberTypeArgsToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    TranslationContexts& contexts;
    
private:
    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    ResultType Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.srtFactory->MakeIrExp<IrExp_Loc>(loc);
    }
    
public:
    ResultType operator()(auto& bodyRes) { return Visit(bodyRes); }

    ResultType Visit(BodyRes_RDeclRes& bodyRes) 
    { 
        return visit(DeclResAndMemberTypeArgsToIrExpTranslator{memberTypeArgs, contexts}, bodyRes.declRes);
    }

    ResultType Visit(BodyRes_LocalVar& bodyRes) 
    {
        return Loc<MLoc_LocalVar>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(BodyRes_LocalRef& bodyRes) 
    { 
        return Loc<MLoc_LocalRef>(bodyRes.name, bodyRes.type);
    }

    // 어떤 경로로 NeedCapture가 나오는가
    ResultType Visit(BodyRes_NeedCapture& bodyRes) 
    {
        // TODO: [42] BodyRes.NeedCapture구현
        throw NotImplementedException{};
    }

    ResultType Visit(BodyRes_ThisVar& bodyRes) 
    {
        return Loc<MLoc_This>(bodyRes.type);
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateBaseResAndMemberTypeArgsToIrExp(BodyRes& bodyRes, RTypeArguments* memberTypeArgs, TranslationContexts& contexts)
{
    return visit(BodyResAndMemberTypeArgsToIrExpTranslator{memberTypeArgs, contexts}, bodyRes);
}

} // namespace Citron::SyntaxIR0Translation

