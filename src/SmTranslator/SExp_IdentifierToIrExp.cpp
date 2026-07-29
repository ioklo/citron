#include "SExp_IdentifierToIrExp.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "IrExp.h"
#include "SmTranslationContexts.h"
#include "SmFactory.h"
#include "SmFuncContext.h"
#include "Misc.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

namespace {

struct DeclResTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;

    template<typename TIrExp, typename... TArgs> requires derived_from<TIrExp, IrExp>
    IrExp* MakeIrExp(TArgs&&... args)
    {
        return contexts.smFactory->MakeIrExp<TIrExp>(forward<TArgs>(args)...);
    }

    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    MLoc* MakeMLoc(TArgs&&... args)
    {
        return contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
    }
    
    ResultType operator()(auto&& declRes) { return Visit(std::forward<decltype(declRes)>(declRes)); }

    ResultType Visit(auto&& declRes)
    {
        return Error<Error_SharedTranslation_CantMakeSharedFromBase>();
    }

    ResultType Visit(SmDeclRes_Namespaces&& declRes) 
    { 
        assert(memberTypeArgs->GetCount() == 0);
        return MakeIrExp<IrExp_Namespaces>(std::move(declRes.namespaces));
    }

    // ResultType Visit(RDeclRes_GlobalFuncs& declRes); 함수류는 IrExp에서 관심없다

    ResultType Visit(SmDeclRes_Class&& declRes)
    {
        auto* typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeIrExp<IrExp_Class>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    // ResultType Visit(SmDeclRes_ClassFuncs& declRes);
    ResultType Visit(SmDeclRes_ClassVar&& declRes) 
    {   
        assert(memberTypeArgs->GetCount() == 0);

        if (declRes.appliedDecl.decl->IsStatic()) // &C.x
        {
            auto* loc = MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs);
            return MakeIrExp<IrExp_Static>(loc);
        }
        else // &this.x
        {   
            return MakeIrExp<IrExp_ClassVar>(
                contexts.funcContext->MakeThisLoc(), declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs);
        }
    }

    ResultType Visit(SmDeclRes_Struct&& declRes)
    { 
        auto* typeArgs = contexts.rFactory->MergeTypeArguments(declRes.outerAppliedDecl.outerTypeArgs, memberTypeArgs);
        return MakeIrExp<IrExp_Struct>(declRes.outerAppliedDecl.decl, typeArgs);
    }

    // ResultType Visit(SmDeclRes_StructFuncs& declRes);

    ResultType Visit(SmDeclRes_StructVar&& declRes) 
    {
        assert(memberTypeArgs->GetCount() == 0);

        if (declRes.appliedDecl.decl->IsStatic())
        {
            auto* loc = MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs);
            return MakeIrExp<IrExp_Static>(loc);
        }
        else
        {
            // TODO: [10] shared함수이면 this를 shared로 판단해야 한다
            // 지금은 this의 타입이 S&이다.

            // IrExp_StructVar는 base가 IrExp인 경우(sharedExp로 보일수 있는 가능성)에만 만드는것이다.

            auto* loc = MakeMLoc<MLoc_StructVar>(contexts.funcContext->MakeThisLoc(), declRes.appliedDecl.decl, declRes.appliedDecl.typeArgs);
            return MakeIrExp<IrExp_Loc>(loc);
        }
    }
    // ResultType Visit(RDeclRes_Enum&& declRes);
    // ResultType Visit(RDeclRes_EnumElem&& declRes);
    // ResultType Visit(RDeclRes_EnumElemVar&& declRes);
    // ResultType Visit(RDeclRes_Lambda&& declRes);
    // ResultType Visit(RDeclRes_LambdaVar&& declRes);
    // ResultType Visit(RDeclRes_Interface&& declRes);
    // ResultType Visit(RDeclRes_TupleVar&& declRes);
    // ResultType Visit(RDeclRes_TypeVar&& declRes);
};

struct BodyResTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    RTypeArguments* memberTypeArgs;
    SmTranslationContexts& contexts;
    
private:
    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    ResultType Loc(TArgs&&... args)
    {
        auto* loc = contexts.mFactory->MakeMLoc<TMLoc>(forward<TArgs>(args)...);
        return contexts.smFactory->MakeIrExp<IrExp_Loc>(loc);
    }
    
public:
    ResultType operator()(auto&& bodyRes) { return Visit(std::forward<decltype(bodyRes)>(bodyRes)); }

    ResultType Visit(SmBodyRes_DeclRes&& bodyRes) 
    { 
        return std::move(bodyRes.declRes).Visit(DeclResTranslator{memberTypeArgs, contexts});
    }

    ResultType Visit(SmBodyRes_LocalVar&& bodyRes) 
    {
        return Loc<MLoc_LocalVar>(bodyRes.name, bodyRes.type);
    }

    ResultType Visit(SmBodyRes_LocalRef&& bodyRes) 
    { 
        return Loc<MLoc_LocalRef>(bodyRes.name, bodyRes.type);
    }

    // 어떤 경로로 NeedCapture가 나오는가
    ResultType Visit(SmBodyRes_NeedCapture&& bodyRes) 
    {
        // TODO: [42] SmBodyRes.NeedCapture구현
        throw NotImplementedException{};
    }

    ResultType Visit(SmBodyRes_ThisVar&& bodyRes) 
    {
        return Loc<MLoc_This>(bodyRes.type);
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExp_IdentifierToIrExp(SExp_Identifier* sExp, SmTranslationContexts& contexts)
{
    // identifier는 name<typeArgs>로 이뤄져 있다
    auto e_memberTypeArgs = MakeRTypeArgs(sExp->typeArgs, SmTypeResolveScope_FuncContext{contexts.funcContext.get()}, contexts.rFactory.get());
    RETURN_ON_ERROR(e_memberTypeArgs);

    auto* memberTypeArgs = *e_memberTypeArgs;
    auto e_bodyRes = ResolveIdentifier(RName::Normal(sExp->value), contexts);
    RETURN_ON_ERROR(e_bodyRes);

    return std::move(*e_bodyRes).Visit(BodyResTranslator{memberTypeArgs, contexts});
}

} // namespace Citron::SyntaxIR0Translation

