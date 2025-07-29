#include "SVarDeclToNStmtsTranslation.h"

#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Unreachable.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "IR0/RTypes.h"
#include "IR0/NStmt.h"

#include "DeclTypeInfo.h"
#include "TranslationContext.h"
#include "SExpToNExpTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class VarDeclElemTranslator
{
    vector<NStmtPtr>* outStmts;

    SVarDeclElement& elem;
    DeclTypeInfo& declTypeInfo;
    
    TranslationContext& context;

public:
    VarDeclElemTranslator(vector<NStmtPtr>* outStmts, SVarDeclElement& elem, DeclTypeInfo& declTypeInfo, TranslationContext& context)
        : outStmts(outStmts), elem(elem), declTypeInfo(declTypeInfo), context(context)
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
        if (!elem.initExp)
            return unexpected{MakePtr<Error_VarDecl_LocalVarDeclNeedInitializer>()};

        // var꼴로 나오는 경우 hintType은 없다
        auto eNInitExp = TranslateSExpToNExp(*elem.initExp, /*hintType*/ nullptr, context);
        if (!eNInitExp) return unexpected{move(eNInitExp).error()};
        auto rInitExpType = context.GetType(**eNInitExp);

        auto eResult = CheckVarConsistency(rInitExpType.get());
        if (!eResult) return unexpected{move(eResult).error()};

        context.AddLocalVarInfo(rInitExpType, RName_Normal(elem.varName));
        outStmts->push_back(MakePtr<NStmt_LocalVarDecl>(rInitExpType, elem.varName, move(*eNInitExp)));

        return {};
    }

    expected<void, DiagPtr> HandleExplicitDeclType()
    {
        assert(declTypeInfo.kind == DeclTypeInfoKind::Normal);
        auto& declType = declTypeInfo.type;

        NExpPtr nInitExp;
        if (elem.initExp)
        {
            auto eNExp = TranslateSExpToNExp(*elem.initExp, declType, context);
            if (!eNExp) return unexpected{move(eNExp).error()};

            eNExp = CastNExp(move(*eNExp), declType, context);
            if (!eNExp) return unexpected{MakePtr<Error_VarDecl_InitExpTypeMismatch>()};

            nInitExp = *eNExp;
        }

        context.AddLocalVarInfo(declType, RName_Normal(elem.varName));
        outStmts->push_back(MakePtr<NStmt_LocalVarDecl>(declType, elem.varName, move(nInitExp)));

        return {};
    }

public:
    expected<void, DiagPtr> Translate()
    {
        if (context.DoesLocalVarNameExistInScope(elem.varName))
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

expected<void, DiagPtr> TranslateSVarDeclToNStmts(std::vector<NStmtPtr>* outStmts, SVarDecl& varDecl, TranslationContext& context)
{
    DeclTypeInfo declTypeInfo = context.GetDeclTypeInfo(*varDecl.type);

    for (auto& elem : varDecl.elements)
    {
        VarDeclElemTranslator translator{outStmts, elem, declTypeInfo, context};

        auto eResult = translator.Translate();
        if (!eResult) return unexpected{move(eResult).error()};
    }

    return {};
}

} // namespace Citron::SyntaxIR0Translator