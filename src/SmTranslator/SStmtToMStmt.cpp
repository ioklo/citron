#include "SStmtToMStmt.h"

#include <optional>
#include <variant>
#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"
#include "Logging/Diag.h"

#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "MIR/MStmt.h"
#include "MIR/MInitExp.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MCreate.h"
#include "MIR/MFactory.h"
#include "ReExp.h"
#include "SVarDeclToMStmts.h"

#include "SmScopeContext.h"
#include "SmFuncContext.h"
#include "DesignatedDiagnostic.h"
#include "Misc.h"
#include "RFuncAndRArgsToMExpTranslation.h"
#include "SmTranslationContexts.h"
#include "SExpTranslations.h"
#include "SExpToReExp.h"

using namespace std;

namespace Citron {

class RLambdaDecl;

namespace {

struct RLambdaDeclAndArgs
{
    RLambdaDecl* decl;
    std::vector<MArgument> args;   // ctor args
};

expected<void, DiagPtr> TranslateSStmtToMStmts(std::vector<MStmt*>& outStmts, SStmt* sStmt, SmTranslationContexts& contexts);
expected<MStmt_Scope*, DiagPtr> TranslateScopedSEmbeddableStmtToMStmt_Scope(SEmbeddableStmt* embedStmt, SmTranslationContexts& contexts);
expected<MStmt_Scope*, DiagPtr> TranslateLoopSEmbeddableStmtToMStmt_Scope(std::optional<std::string>& o_label, SEmbeddableStmt* sEmbedStmt, SmTranslationContexts& contexts);
expected<void, DiagPtr> TranslateSEmbeddableStmtToMStmts(std::vector<MStmt*>& outStmts, SEmbeddableStmt* embedStmt, SmTranslationContexts& contexts);
expected<void, DiagPtr> TranslateSForStmtInitializerToMStmts(SForStmtInitializer* forInit, vector<MStmt*>& outStmts, SmTranslationContexts& contexts);
expected<MStmt*, DiagPtr> TranslateSExpToMStmt(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, SmTranslationContexts& contexts);
expected<RLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToRLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, SmTranslationContexts& contexts);

bool IsTopLevelExp(MExp* exp)
{
    if (auto* stmtExp = dynamic_cast<MExp_Stmt*>(exp))
        return IsTopLevelExp(stmtExp->finalExp);

    if (auto* callIntrinsicExp = dynamic_cast<MExp_CallIntrinsic*>(exp))
    {
        return callIntrinsicExp->kind == MExp_CallIntrinsicKind::PrefixInc_Int_IntRef
            || callIntrinsicExp->kind == MExp_CallIntrinsicKind::PrefixDec_Int_IntRef
            || callIntrinsicExp->kind == MExp_CallIntrinsicKind::PostfixInc_Int_IntRef
            || callIntrinsicExp->kind == MExp_CallIntrinsicKind::PostfixDec_Int_IntRef;
    }

    return dynamic_cast<MExp_Store*>(exp) != nullptr
        || dynamic_cast<MExp_Call*>(exp) != nullptr;
}

bool IsTopLevelInitExp(MInitExp* initExp)
{
    if (auto* stmtInitExp = dynamic_cast<MInitExp_Stmt*>(initExp))
        return IsTopLevelInitExp(stmtInitExp->finalExp);

    return dynamic_cast<MInitExp_Call*>(initExp) != nullptr;
}

struct SStmtToMStmtsTranslator
{
    using ResultType = expected<void, DiagPtr>;

    vector<MStmt*>& outStmts;
    SmTranslationContexts& contexts;

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MStmt>
    ResultType Value(TArgs&&... args)
    {
        outStmts.push_back(contexts.mFactory->MakeMStmt<TValue>(forward<TArgs>(args)...));
        return {};
    }

    ResultType Value(MStmt* stmt)
    {
        outStmts.push_back(stmt);
        return {};
    }

    ResultType Values(vector<MStmt*>&& stmts)
    {
        outStmts.insert(outStmts.end(), make_move_iterator(stmts.begin()), make_move_iterator(stmts.end()));
        return {};
    }

    ResultType Visit(SStmt_Command* stmt) 
    {
        vector<MRead_Loc> builder;

        auto* stringType = contexts.rFactory->MakeStringType();
        for(auto* cmd : stmt->commands)
        {
            auto e_mCmdRead = TranslateSExpToMRead(cmd, /*hintType*/stringType, contexts);
            RETURN_ON_ERROR(e_mCmdRead);

            // cmd가 SExp_String을 translation하면 무조건 string type이 나오게 된다
            assert(GetType(*e_mCmdRead, &*contexts.rFactory) == stringType);

            builder.push_back(move(get<MRead_Loc>(*e_mCmdRead)));
        }

        return Value<MStmt_Command>(MTopLevel_Command{move(builder)});
    }

    ResultType Visit(SStmt_VarDecl* stmt) 
    {
        // int a;
        // auto x = 
        return TranslateSVarDeclToMStmts(outStmts, &stmt->varDecl, contexts);
    }

    ResultType Visit(SStmt_If* stmt) 
    {
        // 순회
        auto e_mCond = TranslateSExpToMRead(stmt->cond, /*hintType*/contexts.rFactory->MakeBoolType(), contexts);
        RETURN_ON_ERROR(e_mCond);

        // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
        // e_mCond = CastMExp(*e_mCond, contexts.rFactory->MakeBoolType(), contexts);
        // RETURN_ON_ERROR(e_mCond);

        auto* boolType = contexts.rFactory->MakeBoolType();

        auto* condType = GetType(*e_mCond, &*contexts.rFactory);
        if (condType != boolType)
            return Error<Error_IfStmt_ConditionShouldBeBool>();

        auto e_trueBody = TranslateScopedSEmbeddableStmtToMStmt_Scope(stmt->body, contexts);
        RETURN_ON_ERROR(e_trueBody);
        
        if (stmt->elseBody != nullptr)
        {   
            auto e_falseBody = TranslateScopedSEmbeddableStmtToMStmt_Scope(stmt->elseBody, contexts);
            RETURN_ON_ERROR(e_falseBody);
            return Value<MStmt_If>(MTopLevel_Read{move(*e_mCond)}, *e_trueBody, *e_falseBody);
        }
        else
        {
            return Value<MStmt_If>(MTopLevel_Read{move(*e_mCond)}, *e_trueBody, /*falseBody*/nullptr);
        }
    }
    
    expected<MStmt_For*, DiagPtr> MakeInnerFor(SStmt_For* stmt, SmTranslationContexts& forOuterContexts)
    {
        optional<MTopLevel_Read> mCond;
        if (stmt->cond)
        {
            auto boolType = contexts.rFactory->MakeBoolType();
            auto e_mCond = TranslateSExpToMRead(stmt->cond, /*hintType*/boolType, forOuterContexts);
            RETURN_ON_ERROR(e_mCond);

            if (GetType(*e_mCond, &*contexts.rFactory) != boolType)
                return Error<Error_ForStmt_ConditionShouldBeBool>();

            mCond = MTopLevel_Read{move(*e_mCond)};

            // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
            // e_rawCond = CastMExp(*e_rawCond, boolType, contexts);
            // RETURN_ON_ERROR(e_rawCond);
            // condExp = *e_rawCond;
        }

        MStmt* contStmt = nullptr;
        if (stmt->cont)
        {
            // for(;;i++)
            DesignatedDiagnostic<Error_ForStmt_ContinueExpShouldBeAssignOrCall> designatedDiag;
            auto e_contResult = TranslateSExpToMStmt(stmt->cont, /*hintType*/nullptr, &designatedDiag, forOuterContexts);
            RETURN_ON_ERROR(e_contResult);

            contStmt = *e_contResult;
        }

        auto e_body = TranslateLoopSEmbeddableStmtToMStmt_Scope(stmt->o_label, stmt->body, forOuterContexts);
        RETURN_ON_ERROR(e_body);

        return forOuterContexts.mFactory->MakeMStmt<MStmt_For>(move(mCond), contStmt, *e_body);
    }
    
    ResultType Visit(SStmt_For* stmt) 
    {
        // label: for(int i = 0; i < 20; i++) { }
        // label은 for에 대한 label이라기 보다, for 본문에 대한 label이다.
        // scope는 다음과 같다 
        // {
        //     int i;
        //     { // <- label scope (
        //         i < 20;
        //         body    
        //         i++;
        //     }
        // }

        // for(
        //     int i = 0; <- forStmtContexts 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }
        if (stmt->initializer)
        {
            vector<MStmt*> outerStmts;
            auto forOuterContexts = MakeTranslationContexts_DefaultScope(contexts);
            
            auto e_initResult = TranslateSForStmtInitializerToMStmts(stmt->initializer, outerStmts, forOuterContexts);
            RETURN_ON_ERROR(e_initResult);

            auto e_innerFor = MakeInnerFor(stmt, forOuterContexts);
            RETURN_ON_ERROR(e_innerFor);
            
            outerStmts.push_back(*e_innerFor);
            return Value<MStmt_Scope>(MScopeKind_Default{}, move(outerStmts));
        }
        else
        {
            auto e_innerFor = MakeInnerFor(stmt, contexts);
            RETURN_ON_ERROR(e_innerFor);

            return Value(*e_innerFor);
        }
    }

    ResultType Visit(SStmt_While* stmt) 
    {
        optional<MRead> mCond;
        if (stmt->cond)
        {
            auto boolType = contexts.rFactory->MakeBoolType();
            auto e_mCond = TranslateSExpToMRead(stmt->cond, /*hintType*/boolType, contexts);
            RETURN_ON_ERROR(e_mCond);

            if (GetType(*e_mCond, &*contexts.rFactory) != boolType)
                return Error<Error_WhileStmt_ConditionShouldBeBool>();

            mCond = move(*e_mCond);

            // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
            // e_rawCond = CastMExp(*e_rawCond, boolType, contexts);
            // RETURN_ON_ERROR(e_rawCond);
            // condExp = *e_rawCond;
        }

        auto e_body = TranslateLoopSEmbeddableStmtToMStmt_Scope(stmt->o_label, stmt->body, contexts);
        RETURN_ON_ERROR(e_body);

        return Value<MStmt_While>(move(mCond), move(*e_body));
    }

    ResultType Visit(SStmt_Switch* stmt)
    {
        // TODO: [60] switch 구현
        // MakeTranslationContexts_SwitchScope
        throw NotImplementedException{};
    }

    ResultType Visit(SStmt_Continue* stmt)
    {
        if (stmt->o_label)
        {
            auto o_labelId = contexts.funcContext->GetLabelId(*stmt->o_label);
            if (!o_labelId) return Error<Error_ContinueStmt_LabelNotFound>();

            auto o_scopeKind = contexts.scopeContext->GetReachableScopeKind(*o_labelId);
            if (!o_scopeKind) return Error<Error_ContinueStmt_LabelNotReachable>();

            if (!holds_alternative<MScopeKind_Loop>(*o_scopeKind))
                return Error<Error_ContinueStmt_LabelNotCompatible>();

            return Value<MStmt_Continue>(*o_labelId);
        }
        else
        {
            auto o_labelId = contexts.scopeContext->GetCurContinueLabelId();
            if (!o_labelId)
                return Error<Error_ContinueStmt_ShouldUsedInLoop>();

            return Value<MStmt_Continue>(*o_labelId);
        }
    }

    ResultType Visit(SStmt_Break* stmt)
    {
        if (stmt->o_label)
        {
            auto o_labelId = contexts.funcContext->GetLabelId(*stmt->o_label);
            if (!o_labelId) return Error<Error_BreakStmt_LabelNotFound>();

            auto o_scopeKind = contexts.scopeContext->GetReachableScopeKind(*o_labelId);
            if (!o_scopeKind) return Error<Error_BreakStmt_LabelNotReachable>();

            if (!holds_alternative<MScopeKind_Loop>(*o_scopeKind) && 
                !holds_alternative<MScopeKind_Switch>(*o_scopeKind))
                return Error<Error_BreakStmt_LabelNotCompatible>();

            return Value<MStmt_Break>(*o_labelId);
        }
        else
        {
            auto o_labelId = contexts.scopeContext->GetCurBreakLabelId();
            if (!o_labelId)
                return Error<Error_BreakStmt_ShouldUsedInLoop>();

            return Value<MStmt_Break>(*o_labelId);
        }
    }

    ResultType Visit(SStmt_Leave* stmt)
    {
        size_t labelId;
        if (stmt->o_label)
        {
            auto o_labelId = contexts.funcContext->GetLabelId(*stmt->o_label);
            if (!o_labelId) return Error<Error_LeaveStmt_LabelNotFound>();

            auto o_scopeKind = contexts.scopeContext->GetReachableScopeKind(*o_labelId);
            if (!o_scopeKind) return Error<Error_LeaveStmt_LabelNotReachable>();

            if (!holds_alternative<MScopeKind_Inline>(*o_scopeKind))
                return Error<Error_LeaveStmt_LabelNotCompatible>();

            labelId = *o_labelId;
        }
        else
        {
            auto o_labelId = contexts.scopeContext->GetCurLeaveLabelId();
            if (!o_labelId) return Error<Error_LeaveStmt_ShouldUsedInInlineScope>();

            labelId = *o_labelId;
        }
       
        auto* o_inlineScopeType = contexts.scopeContext->GetInlineScopeType();
        auto e_create = TranslateSExpToMCreate(stmt->value, o_inlineScopeType, contexts);
        RETURN_ON_ERROR(e_create);

        auto* createType = GetType(*e_create, &*contexts.rFactory);
        auto* o_newInlineScopeType = contexts.scopeContext->GetInlineScopeType();
        if (!o_newInlineScopeType)
        {
            contexts.scopeContext->SetInlineScopeType(createType);
        }
        else
        {
            if (createType != o_newInlineScopeType)
                return Error<Error_LeaveStmt_TypeMismatch>();
        }

        return Value<MStmt_Leave>(labelId, MTopLevel_Create{move(*e_create)});
    }

    ResultType Visit(SStmt_Return* stmt) 
    {
        // seq 함수는 여기서 모두 처리 
        if (contexts.funcContext->IsSeqFunc())
        {
            if (stmt->value)
            {
                return Error<Error_ReturnStmt_SeqFuncShouldReturnVoid>();
            }

            return Value<MStmt_Return>(nullopt);
        }

        // 리턴 값이 없을 경우
        
        auto funcRet = contexts.funcContext->GetUnboundFuncReturn();

        return funcRet.Visit([this, &stmt](auto& funcRet) -> ResultType
        {
            using T = remove_cvref_t<decltype(funcRet)>;

            if constexpr (same_as<T, RFuncReturn_Normal>)
            {
                if (!stmt->value)
                {
                    // 생성자거나, void 함수가 아니라면 에러
                    if (funcRet.type != contexts.rFactory->MakeVoidType())
                    {
                        return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();
                    }

                    return Value<MStmt_Return>(nullopt);
                }
                else
                {
                    // 리턴타입을 힌트로 사용한다
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    auto e_retValue = TranslateSExpToMCreate(stmt->value, /*hintType*/funcRet.type, contexts);
                    RETURN_ON_ERROR(e_retValue);

                    // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
                    //auto castRetValue = CastMExp(*e_retValue, funcRet.type, contexts);

                    //// 캐스트 실패시
                    //if (!castRetValue)
                    //    return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();

                    return Value<MStmt_Return>(MTopLevel_Create{move(*e_retValue)});
                }
            }
            else if constexpr (same_as<T, RFuncReturn_NotSet>)
            {
                if (!stmt->value)
                {
                    // 이 함수는 void로 리턴을 확정 한다.
                    contexts.funcContext->SetOpenFuncReturn(contexts.rFactory->MakeVoidType());
                    return Value<MStmt_Return>(nullopt);
                }
                else
                {
                    // 힌트타입 없이 분석
                    auto e_retValue = TranslateSExpToMCreate(stmt->value, /*hintType*/nullptr, contexts);
                    RETURN_ON_ERROR(e_retValue);

                    // 리턴값이 안 적혀 있었으므로 적는다
                    auto retValueType = GetType(*e_retValue, &*contexts.rFactory);
                    contexts.funcContext->SetOpenFuncReturn(retValueType);
                    return Value<MStmt_Return>(MTopLevel_Create{move(*e_retValue)});
                }
            }
            else if constexpr (same_as<T, RFuncReturn_None>)
            {
                if (!stmt->value)
                {
                    return Value<MStmt_Return>(nullopt);
                }
                else
                {
                    throw NotImplementedException{}; // 에러 처리
                    // return Error();
                }
            }
            else static_assert(false);
        });
    }

    ResultType Visit(SStmt_Block* stmt) 
    {
        // { }
        vector<DiagPtr> diags;
        auto blockContext = MakeTranslationContexts_DefaultScope(contexts);

        vector<MStmt*> builder;
        for(auto* stmt : stmt->stmts)
        {
            auto e_stmtResult = TranslateSStmtToMStmts(builder, stmt, blockContext);
            if (!e_stmtResult)
            {
                diags.push_back(move(e_stmtResult).error());
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }
        }
        
        if (!diags.empty()) return Error<AggregateDiag>(move(diags));
        return Value<MStmt_Scope>(MScopeKind_Default{}, move(builder));
    }

    ResultType Visit(SStmt_Blank* stmt) 
    {
        return Value<MStmt_Blank>();
    }

    ResultType Visit(SStmt_Exp* stmt)
    {
        DesignatedDiagnostic<Error_ExpStmt_ExpressionShouldBeAssignOrCall> designatedDiag;
        auto e_stmt = TranslateSExpToMStmt(stmt->exp, /*hintType*/nullptr, &designatedDiag, contexts);
        RETURN_ON_ERROR(e_stmt);

        return Value(*e_stmt);
    }

    ResultType Visit(SStmt_Task* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto e_lambdaAndArgs = TranslateSLambdaBodyToRLambdaAndArgs(contexts.rFactory->MakeVoidType(), emptyParams, stmt->body, contexts);
        RETURN_ON_ERROR(e_lambdaAndArgs);

        return Value<MStmt_Task>(e_lambdaAndArgs->decl, move(e_lambdaAndArgs->args));
    }

    ResultType Visit(SStmt_Await* stmt) 
    {   
        auto e_body = TranslateScopedSStmtsToMStmt_Scope(stmt->body, contexts);
        RETURN_ON_ERROR(e_body);

        return Value<MStmt_Await>(*e_body);
    }

    ResultType Visit(SStmt_Async* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto e_lambdaAndArgs = TranslateSLambdaBodyToRLambdaAndArgs(contexts.rFactory->MakeVoidType(), emptyParams, stmt->body, contexts);
        RETURN_ON_ERROR(e_lambdaAndArgs);

        return Value<MStmt_Async>(e_lambdaAndArgs->decl, move(e_lambdaAndArgs->args));
    }
    
    ResultType Visit(SStmt_Foreach* stmt)
    {
        // TODO: [53] foreach 구현
        throw NotImplementedException{};
        
        //struct ForeachStmtTranslator
        //{
        //    vector<MStmt*>* outStmts;
        //    SStmt_Foreach* sStmt;
        //    SmTranslationContexts& contexts;

        //public:
        //    ForeachStmtTranslator(vector<MStmt*>* outStmts, SStmt_Foreach* sStmt, SmTranslationContexts& contexts)
        //        : outStmts{outStmts}, sStmt{sStmt}, contexts{contexts}
        //    {
        //    }

        //    // syntax의 enumerableExp를 사용해서 enumerator를 가져오는 Exp를 생성한다
        //    expected<MExp*, DiagPtr> MakeEnumeratorExp()
        //    {
        //        // TranslationResult<(Exp, IType)> Error() => TranslationResult.Error<(Exp, IType)>();
        //        DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

        //        auto e_nEnumerable = TranslateSExpToMLoc(sStmt->enumerable, /*hintType*/nullptr, /*bMaterializeExp*/true, &designatedDiag, contexts);
        //        RETURN_ON_ERROR(e_nEnumerable);

        //        // GetEnumerator함수를 손으로 찾는다
        //        auto rEnumerableType = (*e_nEnumerable)->GetType();
        //        auto o_rMember = rEnumerableType->GetMember(RNames::GetEnumerator, /*explicitMemberTypeArgsCount*/0);
        //        if (!o_rMember)
        //        {
        //            // TODO: [15] foreach 에러 처리
        //            throw NotImplementedException{};
        //            return unexpected{MakePtr<Error_NotImplemented>()};
        //        }

        //        vector<DeclWithOuterTypeArgs<RFuncDecl>> candidates;

        //        for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*o_rMember))
        //        {
        //            auto* funcDecl = funcDeclWithOuter.decl;

        //            // 파라미터가 없어야 한다
        //            if (funcDecl->GetParamCount() != 0) continue;

        //            // 따라서 Type Parameter도 없어야 한다
        //            if (funcDecl->GetTypeParamCount() != 0) continue;

        //            // instance함수여야 한다
        //            if (funcDecl->GetThisKind() == RThisKind::None) continue;

        //            candidates.push_back(funcDeclWithOuter);
        //        }

        //        if (candidates.empty())
        //        {
        //            // TODO: [15] foreach 에러 처리
        //            throw NotImplementedException{};
        //            return unexpected{MakePtr<Error_NotImplemented>()};
        //        }

        //        if (candidates.size() != 1)
        //        {
        //            // TODO: [15] foreach 에러 처리
        //            throw NotImplementedException{};
        //            return unexpected{MakePtr<Error_NotImplemented>()};
        //        }

        //        auto& result = candidates[0];

        //        // 아까 갯수가 0인지 체크를 했으니 typeArgs는 default이다
        //        return TranslateRFuncAndNArgsToMStmt(result.decl, result.outerTypeArgs, *e_nEnumerable, {}, contexts);
        //    }

        //    expected<MExp*, DiagPtr> MakeNextExpAndInferItemVarType(RType* enumeratorType)
        //    {
        //        auto o_rMember = enumeratorType->GetMember(RNames::Next, /*explicitMemberTypeArgsCount*/0);
        //        if (!o_rMember) return unexpected{MakePtr<Error_NotImplemented>()};

        //        vector<MExp*> candidates;
        //        for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*o_rMember))
        //        {
        //            auto* funcDecl = funcDeclWithOuter.decl;

        //            // TODO: [16] TypeResolver적용
        //            if (funcDecl->GetTypeParamCount() != 0) continue;

        //            // typeParamCount가 0이라고 정했으면, 이 함수의 typeArgs는 outerTypeArgs
        //            auto* typeArgs = funcDeclWithOuter.outerTypeArgs;

        //            // 파라미터는 1개
        //            if (funcDecl->GetParamCount() != 1) continue;

        //            // 리턴 타입은 bool
        //            auto ret = funcDecl->GetFuncReturn(*typeArgs);
        //            auto* setRet = get_if<RFuncReturn_Normal>(&ret);
        //            assert(setRet);

        //            if (setRet->type != contexts.rFactory->MakeBoolType()) continue;

        //            // 인자는 out T&꼴이어야 한다
        //            auto param = funcDecl->GetFuncParam(*typeArgs, 0);
        //            if (param.kind != RFuncParameterKind::Out) continue;

        //            auto* ptrParamType = dynamic_cast<RType_Ptr*>(param.type);
        //            if (!ptrParamType) continue;

        //            // $enumerator.GetNext(&i);
        //            auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
        //            auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RName::Normal(sStmt->varName), ptrParamType->innerType);
        //            auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);
        //            auto e_nextExp = TranslateRFuncAndNArgsToMStmt(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp(mLocalRef)}, contexts);
        //            RETURN_ON_ERROR(e_nextExp);

        //            candidates.push_back(*e_nextExp);
        //        }

        //        if (candidates.size() == 1)
        //        {
        //            return candidates[0];
        //        }
        //        else
        //        {
        //            // TODO: [17] NextFunc가 여러개일때 처리
        //            throw NotImplementedException{};
        //            return unexpected{MakePtr<Error_NotImplemented>()};
        //        }
        //    }

        //    // NextExp를 만드는데, 캐스팅이 필요하면 CastInfo를 같이 돌려준다
        //    // (nextExp, (rawItemType, castExp)? castInfo)

        //    struct CastInfo
        //    {
        //        RType* rawItemType;
        //        MExp* castExp;
        //    };

        //    struct NextExpAndCastExp
        //    {
        //        MExp* nextExp;
        //        optional<CastInfo> castInfo;
        //    };

        //    expected<NextExpAndCastExp, DiagPtr> MakeNextExpAndCastExp(RType* enumeratorType, RType* itemTypeFromSyntax)
        //    {
        //        auto rDeclRes = enumeratorType->GetMember(RNames::Next, /*explicitMemberTypeArgsCount*/0);
        //        if (!rDeclRes) return unexpected{MakePtr<Error_NotImplemented>()};

        //        vector<NextExpAndCastExp> candidates;
        //        for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*rDeclRes))
        //        {
        //            auto* funcDecl = funcDeclWithOuter.decl;
        //            if (funcDecl->GetParamCount() != 1) continue;

        //            auto* typeArgs = funcDeclWithOuter.outerTypeArgs;

        //            // 리턴 타입은 bool
        //            auto ret = funcDecl->GetFuncReturn(*typeArgs);
        //            auto* setRet = get_if<RFuncReturn_Normal>(&ret);
        //            assert(setRet);

        //            if (setRet->type != contexts.rFactory->MakeBoolType()) continue;

        //            // TODO: [16] TypeResolver적용
        //            if (funcDecl->GetTypeParamCount() != 0) continue;

        //            // var symbol = (IFuncSymbol)contexts.InstantiateSymbol(rClass, declSymbol, typeArgs: default);

        //            // 인자는 out T*꼴이어야 한다
        //            auto param = funcDecl->GetFuncParam(*typeArgs, 0);
        //            if (param.kind != RFuncParameterKind::Out) continue;

        //            auto* ptrParamType = dynamic_cast<RType_Ptr*>(param.type);
        //            if (!ptrParamType) continue;

        //            auto itemTypeFromNextParam = ptrParamType->innerType;

        //            if (itemTypeFromNextParam == itemTypeFromSyntax)
        //            {
        //                // $enumerator.GetNext(&i);
        //                auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
        //                auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RName::Normal(sStmt->varName), itemTypeFromNextParam);
        //                auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);
        //                auto nNext = TranslateRFuncAndNArgsToMStmt(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp{mLocalRef}}, contexts);

        //                candidates.emplace_back(*nNext, nullopt);
        //            }
        //            else // 캐스팅
        //            {
        //                auto& rawItemType = itemTypeFromNextParam;
        //                
        //                auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
        //                auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam);
        //                auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);

        //                // $enumerator.GetNext(&$rawItem)
        //                auto e_nNext = TranslateRFuncAndNArgsToMStmt(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp{mLocalRef}}, contexts);
        //                RETURN_ON_ERROR(e_nNext);

        //                // $rawItem
        //                auto* rawItemExp = contexts.mFactory->MakeMExp<MExp_Load>(contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam));
        //                auto castExp = CastMExp(rawItemExp, itemTypeFromSyntax, contexts);
        //                if (castExp) // 캐스팅이 성공할때만 candidates에 넣기
        //                {
        //                    candidates.emplace_back(*e_nNext, CastInfo{rawItemType, *castExp});
        //                }
        //            }
        //        }

        //        size_t count = candidates.size();

        //        if (count == 0)
        //        {
        //            // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
        //            throw NotImplementedException{};
        //            // return nullopt;
        //        }
        //        else if (count == 1)
        //        {
        //            return candidates[0];
        //        }
        //        else
        //        {
        //            // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
        //            throw NotImplementedException{};
        //            // return nullopt;
        //        }
        //    }

        //    expected<vector<MStmt*>, DiagPtr> MakeBody(RType* itemVarType)
        //    {
        //        // 루프 컨텍스트를 하나 열고
        //        auto bodyContext = UsingLoopScope(stmt->o_label, contexts);

        //        // 루프 컨텍스트에 로컬을 하나 추가하고 (enumerator는 추가해야 할까)
        //        bodyContext.scopeContext->AddLocalVarInfo(itemVarType, RName_Normal{sStmt->varName});

        //        // 본문 분석
        //        return TranslateSEmbeddableStmtToMStmts(sStmt->body, contexts);
        //    }

        //public:
        //    expected<void, DiagPtr> Translate()
        //    {
        //        auto e_enumerator = MakeEnumeratorExp();
        //        RETURN_ON_ERROR(e_enumerator);

        //        auto enumeratorType = (*e_enumerator)->GetType();

        //        if (!IsVarType(sStmt->type))
        //        {
        //            auto e_itemType = contexts.scopeContext->TranslateSTypeExpToRType(sStmt->type);
        //            RETURN_ON_ERROR(e_itemType);

        //            auto e_nextExpCastInfo = MakeNextExpAndCastExp(enumeratorType, *e_itemType);
        //            RETURN_ON_ERROR(e_nextExpCastInfo);

        //            auto& [nextExp, oCastInfo] = *e_nextExpCastInfo;

        //            auto e_body = MakeBody(*e_itemType);
        //            RETURN_ON_ERROR(e_body);

        //            if (!oCastInfo)
        //            {
        //                outStmts.push_back(contexts.mFactory->MakeMStmt<MStmt_Foreach>(*e_enumerator, *e_itemType, RName::Normal(sStmt->varName), nextExp, move(*e_body)));
        //            }
        //            else
        //            {
        //                auto& [rawItemType, castExp] = *oCastInfo;
        //                outStmts.push_back(contexts.mFactory->MakeMStmt<MStmt_ForeachCast>(*e_enumerator, *e_itemType, RName::Normal(sStmt->varName), rawItemType, nextExp, castExp, move(*e_body)));
        //            }
        //        }
        //        else // var 일 경우
        //        {
        //            auto e_nextExp = MakeNextExpAndInferItemVarType(enumeratorType);
        //            RETURN_ON_ERROR(e_nextExp);

        //            auto itemVarType = (*e_nextExp)->GetType();

        //            auto e_body = MakeBody(itemVarType);
        //            RETURN_ON_ERROR(e_body);

        //            outStmts.push_back(contexts.mFactory->MakeMStmt<MStmt_Foreach>(*e_enumerator, itemVarType, RName::Normal(sStmt->varName), *e_nextExp, move(*e_body)));
        //        }

        //        return {};
        //    }
        //};

        //ForeachStmtTranslator translator{outStmts, stmt, contexts};
        //return translator.Translate();
    }

    ResultType Visit(SStmt_Yield* stmt) 
    {
        // TODO: ref 처리?
        if (!contexts.funcContext->IsSeqFunc())
        {
            return Error<Error_YieldStmt_YieldShouldBeInSeqFunc>();
        }

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        auto funcRet = contexts.funcContext->GetUnboundFuncReturn();
        auto* setFuncRet = funcRet.TryGetNormal();
        assert(setFuncRet); // 아닌 경우는 위에서 거른다 (sequence함수는 무조건 ret포함)

        // NOTICE: 리턴 타입을 힌트로 넣었다
        auto e_retValue = TranslateSExpToMCreate(stmt->value, /*hintType*/setFuncRet->type, contexts);
        RETURN_ON_ERROR(e_retValue);

        // TODO: [47] CastMExp의 리턴값 수정, NBC의 암시적 Cast구현하기
        // auto e_castRetValue = CastMExp(*e_retValue, setFuncRet->type, contexts);
        // RETURN_ON_ERROR(e_castRetValue);

        return Value<MStmt_Yield>(MTopLevel_Create{move(*e_retValue)});
    }

    ResultType Visit(SStmt_Directive* stmt) 
    {
        if (stmt->name == "static_notnull")
        {
            if (stmt->args.size() != 1)
            {
                return Error<Error_StaticNotNullDirective_ShouldHaveOneArgument>();
            }
            
            auto e_arg = TranslateSExpToMRead(stmt->args[0], /*hintType*/nullptr, contexts);
            RETURN_ON_ERROR(e_arg);

            return Value<MStmt_Directive>(MDirective_StaticNotNullDirective{*e_arg});
        }
        
        throw NotImplementedException{}; // 인식할 수 없는 directive입니다
    }
};

expected<void, DiagPtr> TranslateSStmtToMStmts(vector<MStmt*>& outStmts, SStmt* sStmt, SmTranslationContexts& contexts)
{
    SStmtToMStmtsTranslator translator{outStmts, contexts};
    return Accept(translator, sStmt);
}

expected<void, DiagPtr> TranslateSEmbeddableStmtToMStmts(vector<MStmt*>& outStmts, SEmbeddableStmt* embedStmt, SmTranslationContexts& contexts)
{
    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    struct EmbeddableStmtTranslator
    {
        using ResultType = expected<void, DiagPtr>;
        vector<MStmt*>& outStmts;
        SmTranslationContexts& contexts;
    
        ResultType Visit(SEmbeddableStmt_Single* stmt)
        {
            // TODO: VarDecl은 등장하면 에러를 내도록 한다
            // 지금은 그냥 패스

            return TranslateSStmtToMStmts(outStmts, stmt->stmt, contexts);
        }

        ResultType Visit(SEmbeddableStmt_Block* stmt)
        {
            return TranslateSStmtsToMStmts(outStmts, stmt->stmts, contexts);
        }
    };

    EmbeddableStmtTranslator translator{outStmts, contexts};
    return Accept(translator, embedStmt);
}

expected<MStmt_Scope*, DiagPtr> TranslateScopedSEmbeddableStmtToMStmt_Scope(SEmbeddableStmt* embedStmt, SmTranslationContexts& contexts)
{
    auto newContexts = MakeTranslationContexts_DefaultScope(contexts);

    vector<MStmt*> stmts;
    auto e_result = TranslateSEmbeddableStmtToMStmts(stmts, embedStmt, newContexts);
    RETURN_ON_ERROR(e_result);

    return contexts.mFactory->MakeMStmt<MStmt_Scope>(MScopeKind_Default{}, move(stmts));
}

expected<MStmt_Scope*, DiagPtr> TranslateLoopSEmbeddableStmtToMStmt_Scope(std::optional<std::string>& o_label, SEmbeddableStmt* sEmbedStmt, SmTranslationContexts& contexts)
{
    size_t labelId = contexts.funcContext->AddNewLabelId(o_label);

    // loop는 continue, break 둘 다 갱신한다
    auto newContexts = MakeTranslationContexts_LoopScope(labelId, contexts);

    vector<MStmt*> stmts;
    auto e_result = TranslateSEmbeddableStmtToMStmts(stmts, sEmbedStmt, newContexts);
    RETURN_ON_ERROR(e_result);

    return contexts.mFactory->MakeMStmt<MStmt_Scope>(MScopeKind_Loop{labelId}, move(stmts));
}


expected<void, DiagPtr> TranslateSForStmtInitializerToMStmts(SForStmtInitializer* forInit, vector<MStmt*>& outStmts, SmTranslationContexts& contexts)
{
    struct ForInitTranslator
    {
        using ResultType = expected<void, DiagPtr>;

        vector<MStmt*>& outStmts;
        SmTranslationContexts& contexts;

        ResultType Visit(SForStmtInitializer_Exp* forInit)
        {
            DesignatedDiagnostic<Error_ForStmt_ExpInitializerShouldBeAssignOrCall> designatedDiag;
            auto e_stmt = TranslateSExpToMStmt(forInit->exp, /*hintType*/nullptr, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_stmt);

            outStmts.push_back(*e_stmt);
            return {};
        }

        ResultType Visit(SForStmtInitializer_VarDecl* forInit)
        {   
            auto e_stmtsResult = TranslateSVarDeclToMStmts(outStmts, &forInit->varDecl, contexts);
            RETURN_ON_ERROR(e_stmtsResult);

            return {};
        }
    };

    ForInitTranslator translator{outStmts, contexts};
    return Accept(translator, forInit);
}

expected<MStmt*, DiagPtr> TranslateSExpToMStmt(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, SmTranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return visit([&contexts](auto& reExp) -> expected<MStmt*, DiagPtr> {
        using T = remove_cvref_t<decltype(reExp)>;

        if constexpr (same_as<T, ReExp_Loc>)
        {
            return Error<Error_ExpStmt_ExpressionShouldBeAssignOrCall>();
        }
        else if constexpr (same_as<T, ReExp_Exp>) 
        {
            if (!IsTopLevelExp(reExp.mExp))
                return Error<Error_ExpStmt_ExpressionShouldBeAssignOrCall>();

            return contexts.mFactory->MakeMStmt<MStmt_Exp>(MTopLevel_Create{MCreate_BC{reExp.mExp}});
        }
        else if constexpr (same_as<T, ReExp_InitExp>) 
        {
            if (!IsTopLevelInitExp(reExp.mInitExp))
                return Error<Error_ExpStmt_ExpressionShouldBeAssignOrCall>();

            return contexts.mFactory->MakeMStmt<MStmt_Exp>(MTopLevel_Create{MCreate_NBC{reExp.mInitExp}});
        }
        else if constexpr (same_as<T, ReExp_StmtCall>) 
        {
            return reExp.mCallStmt;
        }
        else if constexpr (same_as<T, ReExp_StmtAssign>) 
        {
            return reExp.mAssignStmt;
        }
        else static_assert(false);

    }, *e_reExp);
}

expected<tuple<vector<RFuncParameter>, bool>, DiagPtr> MakeParameters(vector<SLambdaExpParam>& sParams, SmTranslationContexts& contexts)
{
    bool bLastParamVariadic = false;
    size_t sParamCount = sParams.size();

    vector<RFuncParameter> rParams;
    rParams.reserve(sParamCount);
    for (size_t i = 0; i < sParamCount; i++)
    {
        auto& sParam = sParams[i];

        // TODO: [28] lambda parameter에 reference들어오도록 추가
        auto e_rParamKind = MakeParamKind(sParam.o_paramModifier, false); 
        RETURN_ON_ERROR_REFDECL(e_rParamKind, rParamKind);

        // 파라미터에 Type이 명시되어있지 않으면 hintType기반으로 inference 해야 한다.
        if (!sParam.type)
            throw NotImplementedException{};

        auto e_rParamType = contexts.scopeContext->TranslateSTypeExpToRType(sParam.type);
        RETURN_ON_ERROR(e_rParamType);

        rParams.emplace_back(rParamKind, *e_rParamType, RName::Normal(sParam.name));

        if (rParamKind == RFuncParameterKind::Params)
        {
            if (i == sParamCount - 1)
            {
                bLastParamVariadic = true;
            }
            else
            {
                throw NotImplementedException{}; // 에러 처리. bVariadic은 마지막에 있어야 합니다
            }
        }
    }

    return make_tuple(move(rParams), bLastParamVariadic);
}

RLambdaDeclAndArgs MakeLambdaDeclAndArgs(std::vector<MStmt*>&& body, SmTranslationContexts& contexts)
{
    throw NotImplementedException{};
}

expected<RLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToRLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, SmTranslationContexts& contexts)
{
    // 람다를 분석합니다
    // [int x = x](int p) => { return 3; }

    // 파라미터는 람다 함수의 지역변수로 취급한다
    // var newLambdaBodyContext = funcContext.NewLambdaBodyContext(localContext); // new SmFuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

    // 람다 관련 정보는 여기서 수집한다
    RFuncReturn funcRet = retType ? (RFuncReturn)RFuncReturn_Normal{retType} : RFuncReturn_NotSet();

    auto e_funcParamsInfo = MakeParameters(sParams, contexts);
    RETURN_ON_ERROR_REFDECL(e_funcParamsInfo, [funcParams, bLastParamVariadic]);

    // Lambda를 만들고 context 인스턴스 안에 저장한다
    // DeclSymbol tree로의 Commit은 함수 백트래킹이 다 끝났을 때 (그냥 Translation이 끝났을때 해도 될거 같다)
    auto newContexts = MakeTranslationContexts_Lambda(move(funcRet), move(funcParams), bLastParamVariadic, contexts); // 중첩된 bodyContext를 만들고, 새 scopeContext도 만든다

    // 람다 파라미터(int p)를 지역 변수로 추가한다
    for (auto& sParam : sParams)
    {
        // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
        if (!sParam.type)
        {
            return unexpected{MakePtr<Error_NotSupported_LambdaParameterInference>()};
        }

        auto e_rParamType = contexts.scopeContext->TranslateSTypeExpToRType(sParam.type);
        RETURN_ON_ERROR(e_rParamType);

        newContexts.scopeContext->AddLocalVarInfo(*e_rParamType, RName::Normal(sParam.name));
    }

    vector<MStmt*> rBody;
    auto e_rBodyResult = TranslateSStmtsToMStmts(rBody, sBody, newContexts);
    RETURN_ON_ERROR(e_rBodyResult);

    // body분석을 했던것을 토대로 캡쳐한 변수들을 LambdaVarDecl로 만들고, 현재 context에서 전달할 argument로 만든다
    return MakeLambdaDeclAndArgs(move(rBody), newContexts);
}

} // namespace 

expected<void, DiagPtr> TranslateSStmtsToMStmts(vector<MStmt*>& outBody, span<SStmt*> sStmts, SmTranslationContexts& contexts)
{
    for(auto* sStmt : sStmts)
    {
        auto e_result = TranslateSStmtToMStmts(outBody, sStmt, contexts);
        RETURN_ON_ERROR(e_result);
    }

    return {};
}

expected<MStmt_Scope*, DiagPtr> TranslateScopedSStmtsToMStmt_Scope(span<SStmt*> sStmts, SmTranslationContexts& contexts)
{
    auto innerContexts = MakeTranslationContexts_DefaultScope(contexts);
    vector<MStmt*> mStmts;
    auto e_result = TranslateSStmtsToMStmts(mStmts, sStmts, innerContexts);
    RETURN_ON_ERROR(e_result);

    return contexts.mFactory->MakeMStmt<MStmt_Scope>(MScopeKind_Default{}, move(mStmts));
}

}
