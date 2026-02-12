#include "SVarDeclToMStmtsTranslation.h"

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
#include "SExpToMExpTranslation.h"
#include "SExpToMLocTranslation.h"
#include "SExpToMOperandTranslation.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

class VarDeclElemTranslator
{
    using ResultType = expected<void, DiagPtr>;

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

    void AddLocalVar(MStmt* mStmt, RType* rType, const RName& name)
    {
        contexts.scopeContext->AddLocalVarInfo(rType, name);
        outStmts->push_back(mStmt);
    }

    void AddLocalRef(MStmt* mStmt, RType* rType, const RName& name)
    {
        contexts.scopeContext->AddLocalRefInfo(rType, name);
        outStmts->push_back(mStmt);
    }

    // 각 케이스별로 함수로 만들어 본다
    ResultType Handle_Var_Loc(MLoc* mLoc)
    {
        // TODO: location이면 lvalue로, primitive면 그냥 bitwise copy, struct라면 copy ctor호출
        auto* rType = mLoc->GetType();

        if (auto* rPrimType = dynamic_cast<RType_Primitive*>(rType))
        {
            // 복사: var x = 2; var y = x; // 여기서 var y = x




        }
        else if (auto* rStructType = dynamic_cast<RType_Struct*>(rType))
        {
            // struct일때는 copy ctor호출
            throw NotImplementedException{};
        }
        else
        {
            throw NotImplementedException{};
        }
    }

    bool AreTypeArgsOfMExp_NewStructAndRTypeSame(MExp_NewStruct* newStructExp, RType* rType)
    {
        // MExp_NewStruct의 typeArgs와 rType의 typeArgs가 일치해야 한다
        auto* rStructType = dynamic_cast<RType_Struct*>(rType);
        if (!rStructType) return false;

        if (newStructExp->typeArgs != rStructType->typeArgs) return false;

        return true;
    }

    ResultType Handle_Var_Exp(const RName& varName, MExp* mExp)
    {
        auto* rType = mExp->GetType();

        // NewStruct일때만 따로 처리. StructInit
        // var x = S(1, 2, 3);
        if (auto* newStructExp = dynamic_cast<MExp_NewStruct*>(mExp))
        {
            // MExp_NewStruct의 typeArgs와 rType의 typeArgs가 일치해야 한다
            assert(AreTypeArgsOfMExp_NewStructAndRTypeSame(newStructExp, rType));
            
            auto* stmt = contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rType, varName, MStmt_LocalVarDeclInit_StructInit{newStructExp->ctor});
            AddLocalVar(stmt, rType, varName);
        }
        else
        {
            auto* stmt = contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rType, varName, MStmt_LocalVarDeclInit_Exp{mExp});
            AddLocalVar(stmt, rType, varName);
        }

        return {};
    }
    
public:
    // var x = ...
    ResultType Visit(SVarDeclType_Var* sVarDeclType)
    {
        for (auto& elem : elems)
        {
            RName_Normal varName = {elem.varName};

            if (contexts.scopeContext->DoesLocalNameExistInScope(varName))
                return unexpected{MakePtr<Error_VarDecl_LocalVarNameShouldBeUniqueWithinScope>()};

            return visit([this, sVarDeclType, &varName](auto& sInit) -> ResultType {
                using T = remove_cvref_t<decltype(sInit)>;

                // var x = <expr>
                if constexpr (same_as<T, SVarDeclElementInit_Exp>)
                {
                    // 오히려, sInit.exp를 MExp로 일단 바꾸고, MExp_NewStruct 라면, 변환을 하는게 맞는거 같다
                    auto e_mOperand = TranslateSExpToMOperand(sInit.exp, /*hintType*/nullptr, contexts);
                    RETURN_ON_ERROR(e_mOperand);

                    return visit([this, &varName](auto& mOperand) -> ResultType {
                        using U = remove_cvref_t<decltype(mOperand)>;

                        // var x = l;
                        if constexpr (same_as<U, MOperand_Loc>)
                        {
                            return Handle_Var_Loc();
                        }
                        // var x = F();
                        // var x = S(...);
                        else if constexpr (same_as<U, MOperand_Exp>)
                        {
                            return Handle_Var_Exp(varName, mOperand.exp);
                        }
                        else static_assert(false);
                    }, *e_mOperand);



                    // try
                    if (auto* sCallInitExp = dynamic_cast<SExp_Call*>(sInit.exp))
                    {   
                        // callable인 경우, 
                        auto e_callable = TranslateSExpToImExp(sCallInitExp->callable, /*hintType*/nullptr, contexts);

                        
                        
                    }

                    // var꼴로 나오는 경우 hintType은 없다
                    auto e_nInitExp = TranslateSExpToMExp(sInit.exp, /*hintType*/nullptr, contexts);
                    RETURN_ON_ERROR(e_nInitExp);

                    auto* rInitExpType = (*e_nInitExp)->GetType();
                    auto e_result = CheckVarConsistency(sVarDeclType->kind, rInitExpType);
                    RETURN_ON_ERROR(e_result);

                    auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_LocalVarDecl>(rInitExpType, varName, *e_nInitExp);
                    AddLocalVar(mStmt, rInitExpType, varName);
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
        }

        return {};
    }

    // var& x = ...
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
            auto* mStmt = contexts.mFactory->MakeMStmt<MStmt_LocalRefDecl>(rDeclType, RName_Normal{elem.varName}, *e_mLoc);

            return LocalRef(mStmt, rDeclType, RName_Normal{elem.varName});
        }

        return {};
    }

    // T& x = ...
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

            return LocalRef
            contexts.scopeContext->AddLocalRefInfo(*e_rDeclType, RName_Normal{elem.varName});
            outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_LocalRefDecl>(*e_rDeclType, RName_Normal{elem.varName}, *e_mLoc));
        }

        return {};
    }

    // T x = ...
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