#include "SVarDeclToMStmts.h"

#include <span>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"

#include "RSymbol/RTypes.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "SExpTranslations.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

struct VarDeclElemTranslator
{
    using ResultType = expected<void, DiagPtr>;

    vector<MStmt*>& outStmts;
    span<SVarDeclElement> elems;
    TranslationContexts& contexts;

    expected<void, DiagPtr> CheckVarConsistencyPlainVar(RType* initExpType)
    {
        // 1단계 까지만 체크하고 나머지는 넘어간다
        if (auto* interfaceType = dynamic_cast<RType_Interface*>(initExpType))
        {
            if (interfaceType->bLocal)
                return unexpected{MakePtr<Error_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface>()};

            return {};
        }

        if (dynamic_cast<RType_Shared*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingSharedVarInsteadOfVarWhenInitExpIsShared>()};

        if (dynamic_cast<RType_Box*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingBoxVarInsteadOfVarWhenInitExpIsBox>()};

        if (dynamic_cast<RType_Ptr*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingPtrVarInsteadOfVarWhenInitExpIsPtr>()};

        if (dynamic_cast<RType_Nullable*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr>()};

        if (dynamic_cast<RType_NullableInplace*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr>()};

        return {};
    }

    expected<void, DiagPtr> CheckVarConsistency(SVarDeclType_VarKind kind, RType* initExpType)
    {
        // var 꼴별로 에러 체크
        switch (kind)
        {
        // local, box, shared, ptr, nullable 인지 체크한다 
        case SVarDeclType_VarKind::Normal:
            return CheckVarConsistencyPlainVar(initExpType);
            
        case SVarDeclType_VarKind::Local:
            if (!dynamic_cast<RType_Interface*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface>()};
            return {};

        case SVarDeclType_VarKind::Shared:
            if (!dynamic_cast<RType_Shared*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingSharedVarAsDeclTypeButInitExpIsNotShared>()};
            return {};

        case SVarDeclType_VarKind::Box:
            if (!dynamic_cast<RType_Box*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingBoxVarAsDeclTypeButInitExpIsNotBox>()};
            return {};

        case SVarDeclType_VarKind::Ptr:
            if (!dynamic_cast<RType_Ptr*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingPtrVarAsDeclTypeButInitExpIsNotPtr>()};
            return {};

        case SVarDeclType_VarKind::Nullable:
            if (!dynamic_cast<RType_NullableInplace*>(initExpType) || !dynamic_cast<RType_Nullable*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable>()};
            return {};

        default:
            unreachable();
        }
    }

    void AddLocalVar(RType* rType, const RName& name, MStmt_LocalVarDeclInit&& init)
    {
        auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rType, name, move(init));
        contexts.scopeContext->AddLocalVarInfo(rType, name);
        outStmts.push_back(mStmt);
    }

    void AddLocalRef(RType* rType, const RName& name, MTopLevel_Loc&& mTopLevelLoc)
    {   
        auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_LocalRefDecl>(rType, name, move(mTopLevelLoc));

        contexts.scopeContext->AddLocalRefInfo(rType, name);
        outStmts.push_back(mStmt);
    }

    // var x = ...
    ResultType Visit(SVarDeclType_Var* sVarDeclType)
    {
        for (auto& elem : elems)
        {
            RName_Normal varName = {elem.varName};

            if (contexts.scopeContext->DoesLocalNameExistInScope(varName))
                return Error<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>();

            auto e_result = visit([this, sVarDeclType, &varName](auto& sInit) -> ResultType {
                using T = remove_cvref_t<decltype(sInit)>;

                // var x = <expr>
                if constexpr (same_as<T, SVarDeclElementInit_Exp>)
                {
                    auto e_mCreate = TranslateSExpToMCreate(sInit.exp, /*hintType*/nullptr, contexts);
                    RETURN_ON_ERROR(e_mCreate);

                    auto* initType = GetType(*e_mCreate, &*contexts.rFactory);
                    auto e_result = CheckVarConsistency(sVarDeclType->kind, initType);
                    RETURN_ON_ERROR(e_result);
                    
                    AddLocalVar(initType, varName, MStmt_LocalVarDeclInit_Create{MTopLevel_Create{move(*e_mCreate)}});
                    return {};
                }
                // var x = move expr;
                else if constexpr (same_as<T, SVarDeclElementInit_Move>)
                {
                    // TODO: [30] move구현
                    throw NotImplementedException{};
                }
                // var x = uninit; 에러
                else if constexpr (same_as<T, SVarDeclElementInit_Uninit>)
                {
                    return unexpected{MakePtr<Error_VarDecl_CantInferenceWithoutInitExpression>()};
                }
                else static_assert(false);
            }, elem.init);

            RETURN_ON_ERROR(e_result);
        }

        return {};
    }

    // var& x = ...
    ResultType Visit(SVarDeclType_VarRef* sVarDeclType)
    {   
        for (auto& elem : elems)
        {   
            RName_Normal varName{elem.varName};

            if (contexts.scopeContext->DoesLocalNameExistInScope(varName))
                return Error<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>();

            auto e_result = visit([this, sVarDeclType, &varName](auto& sInit) -> ResultType {
                using T = remove_cvref_t<decltype(sInit)>;

                // var& x = <expr>
                if constexpr (same_as<T, SVarDeclElementInit_Exp>)
                {
                    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;
                    auto e_mLoc = TranslateSExpToMLoc(sInit.exp, /*hintType*/nullptr, /*bMaterializeExp*/false, &notLocationDiag, contexts);
                    RETURN_ON_ERROR(e_mLoc);

                    auto* locType = GetType(*e_mLoc, &*contexts.rFactory);
                    // var&는 단일로만 존재한다. var consistency 테스트를 하지 않는다 
                    AddLocalRef(locType, varName, MTopLevel_Loc{*e_mLoc});
                    return {};
                }
                // var& x = move expr;
                else if constexpr (same_as<T, SVarDeclElementInit_Move>)
                {
                    return Error<Error_VarDecl_RefDeclCantUsingMove>();
                }
                // var& x = uninit; 에러
                else if constexpr (same_as<T, SVarDeclElementInit_Uninit>)
                {
                    return Error<Error_VarDecl_RefDeclNeedLocationInitializer>();
                }
                else static_assert(false);
            }, elem.init);
            RETURN_ON_ERROR(e_result);
        }

        return {};
    }

    // T& x = ...
    ResultType Visit(SVarDeclType_Ref* sVarDeclType)
    {
        auto e_declType = contexts.scopeContext->TranslateSTypeExpToRType(sVarDeclType->typeExp);
        RETURN_ON_ERROR(e_declType);

        auto* declType = *e_declType;
        for (auto& elem : elems)
        {
            RName_Normal varName{elem.varName};

            if (contexts.scopeContext->DoesLocalNameExistInScope(varName))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            auto e_result = visit([this, declType, &varName](auto& sInit) -> ResultType {
                using T = remove_cvref_t<decltype(sInit)>;

                // T& x = <expr>
                if constexpr (same_as<T, SVarDeclElementInit_Exp>)
                {
                    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;
                    auto e_mLoc = TranslateSExpToMLoc(sInit.exp, /*hintType*/declType, /*bMaterializeExp*/false, &notLocationDiag, contexts);
                    RETURN_ON_ERROR(e_mLoc);

                    auto* locType = GetType(*e_mLoc, &*contexts.rFactory);
                    if (locType != declType)
                        return Error<Error_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType>();
                    
                    AddLocalRef(declType, varName, MTopLevel_Loc{*e_mLoc});
                    return {};
                }
                // T& x = move expr;
                else if constexpr (same_as<T, SVarDeclElementInit_Move>)
                {
                    return Error<Error_VarDecl_RefDeclCantUsingMove>();
                }
                // T& x = uninit; 에러
                else if constexpr (same_as<T, SVarDeclElementInit_Uninit>)
                {
                    return Error<Error_VarDecl_RefDeclNeedLocationInitializer>();
                }
                else static_assert(false);
            }, elem.init);
            RETURN_ON_ERROR(e_result);
        }

        return {};
    }

    // T x = ...
    ResultType Visit(SVarDeclType_Normal* sVarDeclType)
    {   
        auto e_declType = contexts.scopeContext->TranslateSTypeExpToRType(sVarDeclType->typeExp);
        RETURN_ON_ERROR(e_declType);
        auto* declType = *e_declType;

        for (auto& elem : elems)
        {
            RName_Normal varName{elem.varName};

            if (contexts.scopeContext->DoesLocalNameExistInScope(varName))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            auto e_result = visit([this, declType, &varName](auto& sInit) -> ResultType {
                using T = remove_cvref_t<decltype(sInit)>;

                // T x = <expr>
                if constexpr (same_as<T, SVarDeclElementInit_Exp>)
                {
                    auto e_mCreate = TranslateSExpToMCreate(sInit.exp, /*hintType*/declType, contexts);
                    RETURN_ON_ERROR(e_mCreate);

                    auto* initType = GetType(*e_mCreate, &*contexts.rFactory);
                    if (initType != declType)
                        return Error<Error_VarDecl_InitExpTypeMismatch>();

                    AddLocalVar(initType, varName, MStmt_LocalVarDeclInit_Create{MTopLevel_Create{move(*e_mCreate)}});
                    return {};
                }
                // var x = move expr;
                else if constexpr (same_as<T, SVarDeclElementInit_Move>)
                {
                    // TODO: [30] move구현
                    throw NotImplementedException{};
                }
                // var x = uninit; 에러
                else if constexpr (same_as<T, SVarDeclElementInit_Uninit>)
                {
                    // TODO: [59] uninitialized 분석
                    auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(declType, varName, MStmt_LocalVarDeclInit_Uninit{});
                    contexts.scopeContext->AddLocalVarInfo(declType, varName);
                    outStmts.push_back(mStmt);
                    return {};
                }
                else static_assert(false);
            }, elem.init);

            RETURN_ON_ERROR(e_result);
        }

        return {};
    }
};

} // namespace

expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>& outStmts, SVarDecl* varDecl, TranslationContexts& contexts)
{
    VarDeclElemTranslator translator{outStmts, varDecl->elements, contexts};
    return Accept(translator, varDecl->type);
}

} // namespace Citron