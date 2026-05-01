#include "ImExpTranslations.h"
#include <variant>
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "ImExp.h"
#include "FuncContext.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

MLoc_ClassVar* TranslateImExp_ClassVarToMLoc_ClassVar(ImExp_ClassVar* imExp, TranslationContexts& contexts)
{
    return visit([imExp, &contexts](auto& instanceKind) -> MLoc_ClassVar* {
        using T = remove_cvref_t<decltype(instanceKind)>;

        if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(instanceKind.mInstLoc, imExp->decl, imExp->typeArgs);
        else if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(/*instance*/nullptr, imExp->decl, imExp->typeArgs);
        else if constexpr (same_as<T, ImExpInstanceKind_Implicit>)
        {
            MLoc* mInstanceLoc = imExp->decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
            return contexts.mFactory->MakeMLoc<MLoc_ClassVar>(mInstanceLoc, imExp->decl, imExp->typeArgs);
        }
        else static_assert(false);
    }, imExp->instanceKind);
}

MLoc_StructVar* TranslateImExp_StructVarToMLoc_StructVar(ImExp_StructVar* imExp, TranslationContexts& contexts)
{
    return visit([imExp, &contexts](auto& instanceKind) -> MLoc_StructVar* {
        using T = remove_cvref_t<decltype(instanceKind)>;

        if constexpr (same_as<T, ImExpInstanceKind_ExplicitInstance>)
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(instanceKind.mInstLoc, imExp->decl, imExp->typeArgs);
        else if constexpr (same_as<T, ImExpInstanceKind_ExplicitStatic>)
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(/*instance*/nullptr, imExp->decl, imExp->typeArgs);
        else if constexpr (same_as<T, ImExpInstanceKind_Implicit>)
        {
            MLoc* mInstanceLoc = imExp->decl->IsStatic() ? nullptr : contexts.funcContext->MakeThisLoc();
            return contexts.mFactory->MakeMLoc<MLoc_StructVar>(mInstanceLoc, imExp->decl, imExp->typeArgs);
        }
        else static_assert(false);
    }, imExp->instanceKind);
}



} // namespace Citron