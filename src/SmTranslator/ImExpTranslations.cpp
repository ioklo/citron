#include "ImExpTranslations.h"
#include <variant>
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "ImExp.h"
#include "SmFuncContext.h"
#include "SmTranslationContexts.h"

using namespace std;

namespace Citron {

MLoc_ClassVar* TranslateImExp_ClassVarToMLoc_ClassVar(ImExp_ClassVar* imExp, SmTranslationContexts& contexts)
{
    return visit([imExp, &contexts](auto& instanceKind) -> MLoc_ClassVar* {
        using T = remove_cvref_t<decltype(instanceKind)>;

        if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(instanceKind.mInstLoc, imExp->appliedDecl);
        else if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, imExp->appliedDecl);
        else if constexpr (same_as<T, ImExpInstanceKind_Implicit>)
        {
            MLoc* mInstanceLoc = imExp->appliedDecl.decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(mInstanceLoc, imExp->appliedDecl);
        }
        else static_assert(false);
    }, imExp->instanceKind);
}

MLoc_StructVar* TranslateImExp_StructVarToMLoc_StructVar(ImExp_StructVar* imExp, SmTranslationContexts& contexts)
{
    return visit([imExp, &contexts](auto& instanceKind) -> MLoc_StructVar* {
        using T = remove_cvref_t<decltype(instanceKind)>;

        if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(instanceKind.mInstLoc, imExp->appliedDecl);
        else if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, imExp->appliedDecl);
        else if constexpr (same_as<T, ImExpInstanceKind_Implicit>)
        {
            MLoc* mInstanceLoc = imExp->appliedDecl.decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(mInstanceLoc, imExp->appliedDecl);
        }
        else static_assert(false);
    }, imExp->instanceKind);
}



} // namespace Citron