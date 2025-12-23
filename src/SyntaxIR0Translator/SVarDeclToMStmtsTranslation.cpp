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

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "SExpToMExpTranslation.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {

class VarDeclElemTranslator
{
    vector<MStmt*>* outStmts;
    span<SVarDeclElement> elems;
    TranslationContext& context;

public:
    VarDeclElemTranslator(vector<MStmt*>* outStmts, std::span<SVarDeclElement>&& elems, TranslationContext& context)
        : outStmts{outStmts}, elems{move(elems)}, context{context}
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
            if (context.GetScopeContext().DoesLocalVarNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            if (!elem.initExp)
                return unexpected{MakePtr<Error_VarDecl_LocalVarDeclNeedInitializer>()};

            // var꼴로 나오는 경우 hintType은 없다
            auto eNInitExp = TranslateSExpToMExp(elem.initExp, /*hintType*/ nullptr, context);
            RETURN_ON_ERROR(eNInitExp);
            
            auto* rInitExpType = context.GetType(*eNInitExp);
            auto eResult = CheckVarConsistency(sVarDeclType->kind, rInitExpType);
            RETURN_ON_ERROR(eResult);

            context.GetScopeContext().AddLocalVarInfo(rInitExpType, RName_Normal{elem.varName});
            outStmts->push_back(context.MakeNStmt<MStmt_LocalVarDecl>(rInitExpType, elem.varName, *eNInitExp));
        }

        return {};
    }

    ResultType Visit(SVarDeclType_VarRef* sVarDeclType)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SVarDeclType_Ref* sVarDeclType)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SVarDeclType_Normal* sVarDeclType)
    {   
        auto eRDeclType = context.TranslateSTypeExpToRType(sVarDeclType->typeExp);
        RETURN_ON_ERROR(eRDeclType);
        auto* rDeclType = *eRDeclType;

        for (auto& elem : elems)
        {
            if (context.GetScopeContext().DoesLocalVarNameExistInScope(RName_Normal{elem.varName}))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            MExp* nInitExp = nullptr;
            if (elem.initExp)
            {
                auto eNExp = TranslateSExpToMExp(elem.initExp, rDeclType, context);
                if (!eNExp) return unexpected{move(eNExp).error()};

                eNExp = CastMExp(*eNExp, rDeclType, context);
                if (!eNExp) return unexpected{MakePtr<Error_VarDecl_InitExpTypeMismatch>()};

                nInitExp = *eNExp;
            }

            context.GetScopeContext().AddLocalVarInfo(rDeclType, RName_Normal{elem.varName});
            outStmts->push_back(context.MakeNStmt<MStmt_LocalVarDecl>(rDeclType, elem.varName, nInitExp));
        }

        return {};
    }
};

} // namespace

expected<void, DiagPtr> TranslateSVarDeclToMStmts(std::vector<MStmt*>* outStmts, SVarDecl* varDecl, TranslationContext& context)
{
    VarDeclElemTranslator translator{outStmts, varDecl->elements, context};
    auto eResult = Accept(translator, varDecl->type);
    RETURN_ON_ERROR(eResult);

    // DeclTypeInfo declTypeInfo = context.GetDeclTypeInfo(varDecl->type);
    for (auto& elem : varDecl->elements)
    {
        if (context.GetScopeContext().DoesLocalVarNameExistInScope(RName_Normal{elem.varName}))
            return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

        
    }

    return {};
}

} // namespace Citron