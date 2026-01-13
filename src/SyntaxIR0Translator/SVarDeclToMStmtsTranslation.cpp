#include "SVarDeclToMStmtsTranslation.h"

#include <span>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "Logging/Diag.h"

#include "RSymbol/RTypes.h"
#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "ScopeContext.h"
#include "SExpToMExpTranslation.h"
#include "SExpToMLocTranslation.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

class VarDeclElemTranslator
{
    vector<MStmt*>* outStmts;
    span<SVarDeclElement> elems;
    TranslationContexts& contexts;

public:
    VarDeclElemTranslator(vector<MStmt*>* outStmts, std::span<SVarDeclElement>&& elems, TranslationContexts& contexts)
        : outStmts{outStmts}, elems{move(elems)}, contexts{contexts}
    {
    }

private:

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

        if (dynamic_cast<RType_NullableValue*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr>()};

        if (dynamic_cast<RType_NullableRef*>(initExpType))
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
            if (!dynamic_cast<RType_NullableRef*>(initExpType) || !dynamic_cast<RType_NullableValue*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable>()};
            return {};

        default:
            unreachable();
        }
    }
    
public:
    using ResultType = expected<void, DiagPtr>;

    ResultType Visit(SVarDeclType_Var* sVarDeclType)
    {
        for (auto& elem : elems)
        {
            if (contexts.scopeContext->DoesLocalNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            if (!elem.initExp)
                return unexpected{MakePtr<Error_VarDecl_LocalVarDeclNeedInitializer>()};

            // var꼴로 나오는 경우 hintType은 없다
            auto e_nInitExp = TranslateSExpToMExp(elem.initExp, /*hintType*/nullptr, contexts);
            RETURN_ON_ERROR(e_nInitExp);
            
            auto* rInitExpType = (*e_nInitExp)->GetType();
            auto e_result = CheckVarConsistency(sVarDeclType->kind, rInitExpType);
            RETURN_ON_ERROR(e_result);

            contexts.scopeContext->AddLocalVarInfo(rInitExpType, RName_Normal{elem.varName});
            outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rInitExpType, RName_Normal{elem.varName}, *e_nInitExp));
        }

        return {};
    }

    ResultType Visit(SVarDeclType_VarRef* sVarDeclType)
    {   
        for (auto& elem : elems)
        {
            if (contexts.scopeContext->DoesLocalNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            assert(elem.initExp); // 이제 초기화 식이 반드시 있어야 한다. 초기화를 안할거면 명시적으로 uninit을 써주는걸로

            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto e_mLoc = TranslateSExpToMLoc(elem.initExp, /*hintType*/nullptr, /*bWrapExpAsLoc*/false, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_mLoc);

            // mLoc의 타입을 그대로 쓴다
            auto* rDeclType = (*e_mLoc)->GetType();

            contexts.scopeContext->AddLocalRefInfo(rDeclType, RName_Normal{elem.varName});
            outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_LocalRefDecl>(rDeclType, RName_Normal{elem.varName}, *e_mLoc));
        }

        return {};
    }

    ResultType Visit(SVarDeclType_Ref* sVarDeclType)
    {
        auto e_rDeclType = contexts.scopeContext->TranslateSTypeExpToRType(sVarDeclType->typeExp);
        RETURN_ON_ERROR(e_rDeclType);

        for (auto& elem : elems)
        {
            if (contexts.scopeContext->DoesLocalNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            assert(elem.initExp); // 이제 초기화 식이 반드시 있어야 한다. 초기화를 안할거면 명시적으로 uninit을 써주는걸로

            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto e_mLoc = TranslateSExpToMLoc(elem.initExp, *e_rDeclType, /*bWrapExpAsLoc*/false, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_mLoc);

            // 둘이 타입이 mismatch되면 에러를 낸다
            if ((*e_mLoc)->GetType() != *e_rDeclType)
                return unexpected{MakePtr<Error_VarDecl_MismatchBetweenRefDeclTypeAndRefInitType>()};

            contexts.scopeContext->AddLocalRefInfo(*e_rDeclType, RName_Normal{elem.varName});
            outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_LocalRefDecl>(*e_rDeclType, RName_Normal{elem.varName}, *e_mLoc));
        }

        return {};
    }

    ResultType Visit(SVarDeclType_Normal* sVarDeclType)
    {   
        auto e_rDeclType = contexts.scopeContext->TranslateSTypeExpToRType(sVarDeclType->typeExp);
        RETURN_ON_ERROR(e_rDeclType);
        auto* rDeclType = *e_rDeclType;

        for (auto& elem : elems)
        {
            if (contexts.scopeContext->DoesLocalNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            MExp* nInitExp = nullptr;
            if (elem.initExp)
            {
                auto e_nExp = TranslateSExpToMExp(elem.initExp, rDeclType, contexts);
                RETURN_ON_ERROR(e_nExp);

                e_nExp = CastMExp(*e_nExp, rDeclType, contexts);
                if (!e_nExp) return unexpected{MakePtr<Error_VarDecl_InitExpTypeMismatch>()};

                nInitExp = *e_nExp;
            }

            contexts.scopeContext->AddLocalVarInfo(rDeclType, RName_Normal{elem.varName});
            outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rDeclType, RName_Normal{elem.varName}, nInitExp));
        }

        return {};
    }
};

} // namespace

expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>* outStmts, SVarDecl* varDecl, TranslationContexts& contexts)
{
    VarDeclElemTranslator translator{outStmts, varDecl->elements, contexts};
    auto e_result = Accept(translator, varDecl->type);
    RETURN_ON_ERROR(e_result);

    return {};
}

} // namespace Citron