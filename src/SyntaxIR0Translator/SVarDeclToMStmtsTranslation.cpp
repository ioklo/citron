#include "SVarDeclToMStmtsTranslation.h"

#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Unreachable.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "RSymbol/RTypes.h"
#include "MIR/MStmt.h"

#include "DeclTypeInfo.h"
#include "TranslationContext.h"
#include "ScopeContext.h"
#include "SExpToMExpTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class VarDeclElemTranslator
{
    vector<MStmt*>* outStmts;

    SVarDeclElement* elem;
    DeclTypeInfo& declTypeInfo;
    
    TranslationContext& context;

public:
    VarDeclElemTranslator(vector<MStmt*>* outStmts, SVarDeclElement* elem, DeclTypeInfo& declTypeInfo, TranslationContext& context)
        : outStmts{outStmts}, elem{elem}, declTypeInfo{declTypeInfo}, context{context}
    {
    }

private:

    expected<void, DiagPtr> CheckVarConsistencyPlainVar(RType* initExpType)
    {
        if (auto* interfaceType = dynamic_cast<RType_Interface*>(initExpType))
        {
            if (interfaceType->bLocal)
                return unexpected{MakePtr<Error_VarDecl_UsingLocalVarInsteadOfVarWhenInitExpIsLocalInterface>()};

            return {};
        }

        if (dynamic_cast<RType_BoxPtr*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingBoxPtrVarInsteadOfVarWhenInitExpIsBoxPtr>()};

        if (dynamic_cast<RType_LocalPtr*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingLocalPtrVarInsteadOfVarWhenInitExpIsLocalPtr>()};

        if (dynamic_cast<RType_NullableValue*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr>()};

        if (dynamic_cast<RType_NullableRef*>(initExpType))
            return unexpected{MakePtr<Error_VarDecl_UsingNullableVarInsteadOfVarWhenInitExpIsNullablePtr>()};

        return {};
    }

    expected<void, DiagPtr> CheckVarConsistency(RType* initExpType)
    {
        // var 꼴별로 에러 체크
        switch (declTypeInfo.kind)
        {
        case DeclTypeInfoKind::Normal:
            assert(false);
            return unexpected{MakePtr<Error_NotRechable>()};

            // local, boxptr, localptr, nullable 인지 체크한다 
        case DeclTypeInfoKind::PlainVar:
            return CheckVarConsistencyPlainVar(initExpType);
            
        case DeclTypeInfoKind::LocalInterfaceVar:
            if (!dynamic_cast<RType_Interface*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingLocalVarAsDeclTypeButInitExpIsNotLocalInterface>()};
            return {};

        case DeclTypeInfoKind::BoxPtrVar:
            if (!dynamic_cast<RType_BoxPtr*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingBoxPtrVarAsDeclTypeButInitExpIsNotBoxPtr>()};
            return {};

        case DeclTypeInfoKind::LocalPtrVar:
            if (!dynamic_cast<RType_LocalPtr*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingLocalPtrVarAsDeclTypeButInitExpIsNotLocalPtr>()};
            return {};

        case DeclTypeInfoKind::NullableVar:
            if (!dynamic_cast<RType_NullableRef*>(initExpType) || !dynamic_cast<RType_NullableValue*>(initExpType))
                return unexpected{MakePtr<Error_VarDecl_UsingNullableVarAsDeclTypeButInitExpIsNotNullable>()};
            return {};

        default:
            return {};
        }

        unreachable();
    }

    expected<void, DiagPtr> HandleVarDeclType()
    {
        if (!elem->initExp)
            return unexpected{MakePtr<Error_VarDecl_LocalVarDeclNeedInitializer>()};

        // var꼴로 나오는 경우 hintType은 없다
        auto eNInitExp = TranslateSExpToMExp(elem->initExp, /*hintType*/ nullptr, context);
        if (!eNInitExp) return unexpected{move(eNInitExp).error()};
        auto rInitExpType = context.GetType(*eNInitExp);

        auto eResult = CheckVarConsistency(rInitExpType);
        if (!eResult) return unexpected{move(eResult).error()};

        context.GetScopeContext().AddLocalVarInfo(rInitExpType, elem->varName);
        outStmts->push_back(context.MakeNStmt<MStmt_LocalVarDecl>(rInitExpType, elem->varName, *eNInitExp));

        return {};
    }

    expected<void, DiagPtr> HandleExplicitDeclType()
    {
        assert(declTypeInfo.kind == DeclTypeInfoKind::Normal);
        auto& declType = declTypeInfo.type;

        MExp* nInitExp;
        if (elem->initExp)
        {
            auto eNExp = TranslateSExpToMExp(elem->initExp, declType, context);
            if (!eNExp) return unexpected{move(eNExp).error()};

            eNExp = CastMExp(*eNExp, declType, context);
            if (!eNExp) return unexpected{MakePtr<Error_VarDecl_InitExpTypeMismatch>()};

            nInitExp = *eNExp;
        }

        context.GetScopeContext().AddLocalVarInfo(declType, elem->varName);
        outStmts->push_back(context.MakeNStmt<MStmt_LocalVarDecl>(declType, elem->varName, nInitExp));

        return {};
    }

public:
    expected<void, DiagPtr> Translate()
    {
        if (context.GetScopeContext().DoesLocalVarNameExistInScope(elem->varName))
            return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

        if (declTypeInfo.kind != DeclTypeInfoKind::Normal)
        {
            return HandleVarDeclType();
        }
        else
        {
            return HandleExplicitDeclType();
        }
    }
};

} // namespace

expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>* outStmts, SVarDecl* varDecl, TranslationContext& context)
{
    DeclTypeInfo declTypeInfo = context.GetDeclTypeInfo(varDecl->type);

    for (auto& elem : varDecl->elements)
    {
        VarDeclElemTranslator translator{outStmts, &elem, declTypeInfo, context};
        auto eResult = translator.Translate();
        if (!eResult) return unexpected{move(eResult).error()};
    }

    return {};
}

} // namespace Citron::SyntaxIR0Translator