#include "SStmtToMStmtTranslation.h"

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
#include "RSymbol/DeclWithOuterTypeArgs.h"
#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFactory.h"
#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "SExpToMExpTranslation.h"
#include "SVarDeclToMStmtsTranslation.h"
#include "SExpToMLocTranslation.h"

#include "ScopeContext.h"
#include "FuncContext.h"
#include "DesignatedDiagnostic.h"
#include "Misc.h"
#include "RFuncAndRArgsToMExpTranslation.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

namespace {

struct NLambdaDeclAndArgs
{
    NLambdaDecl* decl;
    std::vector<MArgument> args;   // ctor args
};

expected<void, DiagPtr> TranslateSStmtToMStmts(std::vector<MStmt*>* outStmts, SStmt* sStmt, TranslationContexts& contexts);
expected<void, DiagPtr> TranslateSEmbeddableStmtToMStmts(std::vector<MStmt*>* outStmts, SEmbeddableStmt* embedStmt, TranslationContexts& contexts);
expected<vector<MStmt*>, DiagPtr> TranslateSEmbeddableStmtToMStmts(SEmbeddableStmt* embedStmt, TranslationContexts& contexts);
expected<vector<MStmt*>, DiagPtr> TranslateSForStmtInitializerToMStmts(SForStmtInitializer* forInit, TranslationContexts& contexts);
expected<MExp*, DiagPtr> TranslateSExpAsTopLevelExpToMExp(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, TranslationContexts& contexts);
expected<NLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToNLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, TranslationContexts& contexts);

bool IsTopLevelRExp(MExp* exp)
{
    return dynamic_cast<MExp_CallInternalUnaryAssignOperator*>(exp) != nullptr
        || dynamic_cast<MExp_Assign*>(exp) != nullptr
        || dynamic_cast<MExp_CallGlobalFunc*>(exp) != nullptr
        || dynamic_cast<MExp_CallClassFunc*>(exp) != nullptr
        || dynamic_cast<MExp_CallStructFunc*>(exp) != nullptr
        || dynamic_cast<MExp_CallLambda*>(exp) != nullptr;
}

class SStmtToMStmtsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;

private:
    vector<MStmt*>* outStmts;
    TranslationContexts& contexts;

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, MStmt>
    ResultType Value(TArgs&&... args)
    {
        outStmts->push_back(contexts.mFactory->MakeMStmt<TValue>(forward<TArgs>(args)...));
        return {};
    }

    ResultType Values(vector<MStmt*>&& stmts)
    {
        outStmts->insert(outStmts->end(), make_move_iterator(stmts.begin()), make_move_iterator(stmts.end()));
        return {};
    }

public:
    SStmtToMStmtsTranslator(vector<MStmt*>* outStmts, TranslationContexts& contexts)
        : outStmts{outStmts}, contexts{contexts}
    {
    }

    ResultType Visit(SStmt_Command* stmt) 
    {
        // CommandStmt에 있는 expStringElement를 분석한다

        vector<MExp_String*> builder;

        for(auto* cmd : stmt->commands)
        {
            auto e_nStringExp = TranslateSStringExpToNStringExp(cmd, contexts);
            RETURN_ON_ERROR(e_nStringExp);

            builder.push_back(*e_nStringExp);
        }

        return Value<MStmt_Command>(move(builder));
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
        auto e_nCond = TranslateSExpToMExp(stmt->cond, /*hintType*/ contexts.rFactory->MakeBoolType(), contexts);
        RETURN_ON_ERROR(e_nCond);

        // cast
        e_nCond = CastMExp(*e_nCond, contexts.rFactory->MakeBoolType(), contexts);
        if (!e_nCond) return Error<Error_IfStmt_ConditionShouldBeBool>();

        auto nestedContext = MakeTranslationContexts_NestedScope(contexts);
        
        auto e_bodyStmts = TranslateSEmbeddableStmtToMStmts(stmt->body, nestedContext);
        RETURN_ON_ERROR(e_bodyStmts);

        vector<MStmt*> elseStmts;
        if (stmt->elseBody != nullptr)
        {
            auto elseContext = MakeTranslationContexts_NestedScope(contexts);
            
            auto e_elseResult = TranslateSEmbeddableStmtToMStmts(stmt->elseBody, elseContext);
            RETURN_ON_ERROR(e_elseResult);

            elseStmts = move(*e_elseResult);
        }

        return Value<MStmt_If>(*e_nCond, move(*e_bodyStmts), move(elseStmts));
    }

    ResultType Visit(SStmt_IfTest* stmt) 
    {
        // if (Type varName = e) e_body         
        auto e_rTestType = contexts.scopeContext->TranslateSTypeExpToRType(stmt->testType);
        RETURN_ON_ERROR(e_rTestType);

        auto e_nTarget = TranslateSExpToMExp(stmt->exp, /*hintType*/ nullptr, contexts);
        RETURN_ON_ERROR(e_nTarget);

        auto bodyContext = MakeTranslationContexts_NestedScope(contexts);
        bodyContext.scopeContext->AddLocalVarInfo(*e_rTestType, RName_Normal{stmt->varName});
        
        auto e_bodyStmts = TranslateSEmbeddableStmtToMStmts(stmt->body, bodyContext);
        RETURN_ON_ERROR(e_bodyStmts);

        vector<MStmt*> elseStmts;
        if (stmt->elseBody)
        {
            auto elseContext = MakeTranslationContexts_NestedScope(contexts);            
            auto elseResult = TranslateSEmbeddableStmtToMStmts(stmt->elseBody, elseContext);

            if (!elseResult)
                return Error(move(elseResult));

            elseStmts = move(*elseResult);
        }

        auto e_nAsExp = MakeMExp_As(*e_nTarget, *e_rTestType, contexts);
        RETURN_ON_ERROR(e_nAsExp);

        auto rTestTypeKind = (*e_rTestType)->GetCustomTypeKind();
        if (rTestTypeKind == RCustomTypeKind::Class || rTestTypeKind == RCustomTypeKind::Interface)
            return Value<MStmt_IfNullableRefTest>(*e_rTestType, RName_Normal(stmt->varName), *e_nAsExp, move(*e_bodyStmts), move(elseStmts));
        else if (rTestTypeKind == RCustomTypeKind::Enum)
            return Value<MStmt_IfNullableValueTest>(*e_rTestType, RName_Normal(stmt->varName), *e_nAsExp, move(*e_bodyStmts), move(elseStmts));
        else
            throw NotImplementedException{}; // 에러
    }

    ResultType Visit(SStmt_For* stmt) 
    {
        // for(
        //     int i = 0; <- forStmtContexts 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }
        auto forStmtContexts = MakeTranslationContexts_NestedScope(contexts); // prelude는 loop가 아니다

        vector<MStmt*> initStmts;
        if (stmt->initializer)
        {   
            auto e_initResult = TranslateSForStmtInitializerToMStmts(stmt->initializer, forStmtContexts);
            RETURN_ON_ERROR(e_initResult);

            initStmts = move(*e_initResult);
        }

        MExp* condExp = nullptr;
        if (stmt->cond)
        {
            auto boolType = contexts.rFactory->MakeBoolType();
            auto e_rawCond = TranslateSExpToMExp(stmt->cond, /*hintType*/ boolType, forStmtContexts);
            RETURN_ON_ERROR(e_rawCond);

            e_rawCond = CastMExp(*e_rawCond, boolType, contexts);
            RETURN_ON_ERROR(e_rawCond);

            condExp = *e_rawCond;
        }

        MExp* continueExp = nullptr;
        if (stmt->cont)
        {
            DesignatedDiagnostic<Error_ForStmt_ContinueExpShouldBeAssignOrCall> designatedDiag;
            auto e_contResult = TranslateSExpAsTopLevelExpToMExp(stmt->cont, /*hintType*/ nullptr, &designatedDiag, forStmtContexts);
            RETURN_ON_ERROR(e_contResult);

            continueExp = *e_contResult;
        }

        auto bodyContext = MakeTranslationContexts_NestedLoop(forStmtContexts);
        
        auto e_bodyStmts = TranslateSEmbeddableStmtToMStmts(stmt->body, bodyContext);
        RETURN_ON_ERROR(e_bodyStmts);

        return Value<MStmt_For>(move(initStmts), condExp, continueExp, move(*e_bodyStmts));
    }

    ResultType Visit(SStmt_Continue* stmt)
    {
        if (!contexts.scopeContext->IsInLoop())
        {
            return Error<Error_ContinueStmt_ShouldUsedInLoop>();
        }

        return Value<MStmt_Continue>();
    }

    ResultType Visit(SStmt_Break* stmt)
    {
        if (!contexts.scopeContext->IsInLoop())
        {
            return Error<Error_BreakStmt_ShouldUsedInLoop>();
        }

        return Value<MStmt_Break>();
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

            return Value<MStmt_Return>(nullptr);
        }

        // 리턴 값이 없을 경우
        
        auto funcRet = contexts.funcContext->GetUnboundFuncReturn();

        return visit([this, &stmt](auto& funcRet) -> ResultType
        {
            using T = remove_cvref_t<decltype(funcRet)>;

            if constexpr (same_as<T, RFuncReturn_Set>)
            {
                if (!stmt->value)
                {
                    // 생성자거나, void 함수가 아니라면 에러
                    if (funcRet.type != contexts.rFactory->MakeVoidType())
                    {
                        return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();
                    }

                    return Value<MStmt_Return>(nullptr);
                }
                else
                {
                    // 리턴타입을 힌트로 사용한다
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    auto e_retValue = TranslateSExpToMExp(stmt->value, /*hintType*/ funcRet.type, contexts);
                    RETURN_ON_ERROR(e_retValue);

                    auto castRetValue = CastMExp(*e_retValue, funcRet.type, contexts);

                    // 캐스트 실패시
                    if (!castRetValue)
                        return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();

                    return Value<MStmt_Return>(*castRetValue);
                }
            }
            else if constexpr (same_as<T, RFuncReturn_NotSet>)
            {
                if (!stmt->value)
                {
                    // 이 함수는 void로 리턴을 확정 한다.
                    contexts.funcContext->SetOpenFuncReturn(contexts.rFactory->MakeVoidType());
                    return Value<MStmt_Return>(nullptr);
                }
                else
                {
                    // 힌트타입 없이 분석
                    auto e_retValue = TranslateSExpToMExp(stmt->value, /*hintType*/ nullptr, contexts);
                    RETURN_ON_ERROR(e_retValue);

                    // 리턴값이 안 적혀 있었으므로 적는다
                    contexts.funcContext->SetOpenFuncReturn((*e_retValue)->GetType());
                    return Value<MStmt_Return>(*e_retValue);
                }
            }
            else if constexpr (same_as<T, RFuncReturn_ForCtor>)
            {
                if (!stmt->value)
                {
                    return Value<MStmt_Return>(nullptr);
                }
                else
                {
                    throw NotImplementedException{}; // 에러 처리
                    // return Error();
                }
            }
            else static_assert(false);
        }, funcRet);
    }

    ResultType Visit(SStmt_Block* stmt) 
    {
        // { }
        vector<DiagPtr> diags;
        auto blockContext = MakeTranslationContexts_NestedScope(contexts);

        vector<MStmt*> builder;
        for(auto* stmt : stmt->stmts)
        {
            auto e_stmtResult = TranslateSStmtToMStmts(&builder, stmt, blockContext);
            if (!e_stmtResult)
            {
                diags.push_back(move(e_stmtResult).error());
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }
        }
        
        if (!diags.empty()) return Error<AggregateDiag>(move(diags));
        return Value<MStmt_Block>(move(builder));
    }

    ResultType Visit(SStmt_Blank* stmt) 
    {
        return Value<MStmt_Blank>();
    }

    ResultType Visit(SStmt_Exp* stmt)
    {
        DesignatedDiagnostic<Error_ExpStmt_ExpressionShouldBeAssignOrCall> designatedDiag;
        auto e_exp = TranslateSExpAsTopLevelExpToMExp(stmt->exp, /*hintType*/ nullptr, &designatedDiag, contexts);
        RETURN_ON_ERROR(e_exp);

        return Value<MStmt_Exp>(*e_exp);
    }

    ResultType Visit(SStmt_Task* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto e_lambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(contexts.rFactory->MakeVoidType(), emptyParams, stmt->body, contexts);
        RETURN_ON_ERROR(e_lambdaAndArgs);

        return Value<MStmt_Task>(e_lambdaAndArgs->decl, move(e_lambdaAndArgs->args));
    }

    ResultType Visit(SStmt_Await* stmt) 
    {
        auto newContext = MakeTranslationContexts_NestedScope(contexts);
        auto e_body = TranslateSBodyToMStmts(stmt->body, newContext);
        RETURN_ON_ERROR(e_body);

        return Value<MStmt_Await>(move(*e_body));
    }

    ResultType Visit(SStmt_Async* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto e_lambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(contexts.rFactory->MakeVoidType(), emptyParams, stmt->body, contexts);
        RETURN_ON_ERROR(e_lambdaAndArgs);

        return Value<MStmt_Async>(e_lambdaAndArgs->decl, move(e_lambdaAndArgs->args));
    }
    
    ResultType Visit(SStmt_Foreach* stmt)
    {
        struct ForeachStmtTranslator
        {
            vector<MStmt*>* outStmts;
            SStmt_Foreach* sStmt;
            TranslationContexts& contexts;

        public:
            ForeachStmtTranslator(vector<MStmt*>* outStmts, SStmt_Foreach* sStmt, TranslationContexts& contexts)
                : outStmts{outStmts}, sStmt{sStmt}, contexts{contexts}
            {
            }

            // syntax의 enumerableExp를 사용해서 enumerator를 가져오는 Exp를 생성한다
            expected<MExp*, DiagPtr> MakeEnumeratorExp()
            {
                // TranslationResult<(Exp, IType)> Error() => TranslationResult.Error<(Exp, IType)>();
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

                auto e_nEnumerable = TranslateSExpToMLoc(sStmt->enumerable, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
                RETURN_ON_ERROR(e_nEnumerable);

                // GetEnumerator함수를 손으로 찾는다
                auto rEnumerableType = (*e_nEnumerable)->GetType();
                auto o_rMember = rEnumerableType->GetMember(RNames::GetEnumerator, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!o_rMember)
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException{};
                    return unexpected{MakePtr<Error_NotImplemented>()};
                }

                vector<DeclWithOuterTypeArgs<RFuncDecl>> candidates;

                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*o_rMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl;

                    // 파라미터가 없어야 한다
                    if (funcDecl->GetParamCount() != 0) continue;

                    // 따라서 Type Parameter도 없어야 한다
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // instance함수여야 한다
                    if (funcDecl->IsStatic()) continue;

                    candidates.push_back(funcDeclWithOuter);
                }

                if (candidates.empty())
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException{};
                    return unexpected{MakePtr<Error_NotImplemented>()};
                }

                if (candidates.size() != 1)
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException{};
                    return unexpected{MakePtr<Error_NotImplemented>()};
                }

                auto& result = candidates[0];

                // 아까 갯수가 0인지 체크를 했으니 typeArgs는 default이다
                return TranslateRFuncAndNArgsToMExp(result.decl, result.outerTypeArgs, *e_nEnumerable, {}, contexts);
            }

            expected<MExp*, DiagPtr> MakeNextExpAndInferItemVarType(RType* enumeratorType)
            {
                auto o_rMember = enumeratorType->GetMember(RNames::Next, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!o_rMember) return unexpected{MakePtr<Error_NotImplemented>()};

                vector<MExp*> candidates;
                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*o_rMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl;

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // typeParamCount가 0이라고 정했으면, 이 함수의 typeArgs는 outerTypeArgs
                    auto* typeArgs = funcDeclWithOuter.outerTypeArgs;

                    // 파라미터는 1개
                    if (funcDecl->GetParamCount() != 1) continue;

                    // 리턴 타입은 bool
                    auto ret = funcDecl->GetFuncReturn(*typeArgs);
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != contexts.rFactory->MakeBoolType()) continue;

                    // 인자는 out T&꼴이어야 한다
                    auto param = funcDecl->GetFuncParam(*typeArgs, 0);
                    if (param.kind != RFuncParameterKind::Out) continue;

                    auto* ptrParamType = dynamic_cast<RType_Ptr*>(param.type);
                    if (!ptrParamType) continue;

                    // $enumerator.GetNext(&i);
                    auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                    auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RName_Normal(sStmt->varName), ptrParamType->innerType);
                    auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);
                    auto e_nextExp = TranslateRFuncAndNArgsToMExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp(mLocalRef)}, contexts);
                    RETURN_ON_ERROR(e_nextExp);

                    candidates.push_back(*e_nextExp);
                }

                if (candidates.size() == 1)
                {
                    return candidates[0];
                }
                else
                {
                    // TODO: [17] NextFunc가 여러개일때 처리
                    throw NotImplementedException{};
                    return unexpected{MakePtr<Error_NotImplemented>()};
                }
            }

            // NextExp를 만드는데, 캐스팅이 필요하면 CastInfo를 같이 돌려준다
            // (nextExp, (rawItemType, castExp)? castInfo)

            struct CastInfo
            {
                RType* rawItemType;
                MExp* castExp;
            };

            struct NextExpAndCastExp
            {
                MExp* nextExp;
                optional<CastInfo> castInfo;
            };

            expected<NextExpAndCastExp, DiagPtr> MakeNextExpAndCastExp(RType* enumeratorType, RType* itemTypeFromSyntax)
            {
                auto rMember = enumeratorType->GetMember(RNames::Next, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!rMember) return unexpected{MakePtr<Error_NotImplemented>()};

                vector<NextExpAndCastExp> candidates;
                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*rMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl;
                    if (funcDecl->GetParamCount() != 1) continue;

                    auto* typeArgs = funcDeclWithOuter.outerTypeArgs;

                    // 리턴 타입은 bool
                    auto ret = funcDecl->GetFuncReturn(*typeArgs);
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != contexts.rFactory->MakeBoolType()) continue;

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // var symbol = (IFuncSymbol)contexts.InstantiateSymbol(outer, declSymbol, typeArgs: default);

                    // 인자는 out T*꼴이어야 한다
                    auto param = funcDecl->GetFuncParam(*typeArgs, 0);
                    if (param.kind != RFuncParameterKind::Out) continue;

                    auto* ptrParamType = dynamic_cast<RType_Ptr*>(param.type);
                    if (!ptrParamType) continue;

                    auto itemTypeFromNextParam = ptrParamType->innerType;

                    if (itemTypeFromNextParam == itemTypeFromSyntax)
                    {
                        // $enumerator.GetNext(&i);
                        auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                        auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RName_Normal(sStmt->varName), itemTypeFromNextParam);
                        auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);
                        auto nNext = TranslateRFuncAndNArgsToMExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp{mLocalRef}}, contexts);

                        candidates.emplace_back(*nNext, nullopt);
                    }
                    else // 캐스팅
                    {
                        auto& rawItemType = itemTypeFromNextParam;
                        
                        auto* nEnumerator = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                        auto* mLocalVar = contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam);
                        auto* mLocalRef = contexts.mFactory->MakeMExp<MExp_PtrRef>(mLocalVar, contexts.rFactory);

                        // $enumerator.GetNext(&$rawItem)
                        auto e_nNext = TranslateRFuncAndNArgsToMExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {MArgument_Exp{mLocalRef}}, contexts);
                        RETURN_ON_ERROR(e_nNext);

                        // $rawItem
                        auto* rawItemExp = contexts.mFactory->MakeMExp<MExp_Load>(contexts.mFactory->MakeMLoc<MLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam));
                        auto castExp = CastMExp(rawItemExp, itemTypeFromSyntax, contexts);
                        if (castExp) // 캐스팅이 성공할때만 candidates에 넣기
                        {
                            candidates.emplace_back(*e_nNext, CastInfo{rawItemType, *castExp});
                        }
                    }
                }

                size_t count = candidates.size();

                if (count == 0)
                {
                    // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                    throw NotImplementedException{};
                    // return nullopt;
                }
                else if (count == 1)
                {
                    return candidates[0];
                }
                else
                {
                    // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                    throw NotImplementedException{};
                    // return nullopt;
                }
            }

            expected<vector<MStmt*>, DiagPtr> MakeBody(RType* itemVarType)
            {
                // 루프 컨텍스트를 하나 열고
                auto bodyContext = MakeTranslationContexts_NestedLoop(contexts);

                // 루프 컨텍스트에 로컬을 하나 추가하고 (enumerator는 추가해야 할까)
                bodyContext.scopeContext->AddLocalVarInfo(itemVarType, RName_Normal{sStmt->varName});

                // 본문 분석
                return TranslateSEmbeddableStmtToMStmts(sStmt->body, contexts);
            }

        public:
            expected<void, DiagPtr> Translate()
            {
                auto e_enumerator = MakeEnumeratorExp();
                RETURN_ON_ERROR(e_enumerator);

                auto enumeratorType = (*e_enumerator)->GetType();

                if (!IsVarType(sStmt->type))
                {
                    auto e_itemType = contexts.scopeContext->TranslateSTypeExpToRType(sStmt->type);
                    RETURN_ON_ERROR(e_itemType);

                    auto e_nextExpCastInfo = MakeNextExpAndCastExp(enumeratorType, *e_itemType);
                    RETURN_ON_ERROR(e_nextExpCastInfo);

                    auto& [nextExp, oCastInfo] = *e_nextExpCastInfo;

                    auto e_body = MakeBody(*e_itemType);
                    RETURN_ON_ERROR(e_body);

                    if (!oCastInfo)
                    {
                        outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_Foreach>(*e_enumerator, *e_itemType, RName_Normal(sStmt->varName), nextExp, move(*e_body)));
                    }
                    else
                    {
                        auto& [rawItemType, castExp] = *oCastInfo;
                        outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_ForeachCast>(*e_enumerator, *e_itemType, RName_Normal(sStmt->varName), rawItemType, nextExp, castExp, move(*e_body)));
                    }
                }
                else // var 일 경우
                {
                    auto e_nextExp = MakeNextExpAndInferItemVarType(enumeratorType);
                    RETURN_ON_ERROR(e_nextExp);

                    auto itemVarType = (*e_nextExp)->GetType();

                    auto e_body = MakeBody(itemVarType);
                    RETURN_ON_ERROR(e_body);

                    outStmts->push_back(contexts.mFactory->MakeMStmt<MStmt_Foreach>(*e_enumerator, itemVarType, RName_Normal(sStmt->varName), *e_nextExp, move(*e_body)));
                }

                return {};
            }
        };

        ForeachStmtTranslator translator{outStmts, stmt, contexts};
        return translator.Translate();
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
        auto* setFuncRet = get_if<RFuncReturn_Set>(&funcRet);

        assert(setFuncRet); // 아닌 경우는 위에서 거른다 (sequence함수는 무조건 ret포함)

        // NOTICE: 리턴 타입을 힌트로 넣었다
        auto e_retValue = TranslateSExpToMExp(stmt->value, /*hintType*/ setFuncRet->type, contexts);
        RETURN_ON_ERROR(e_retValue);

        auto e_castRetValue = CastMExp(*e_retValue, setFuncRet->type, contexts);
        RETURN_ON_ERROR(e_castRetValue);

        return Value<MStmt_Yield>(*e_castRetValue);
    }

    ResultType Visit(SStmt_Directive* stmt) 
    {
        if (stmt->name == "static_notnull")
        {
            if (stmt->args.size() != 1)
            {
                return Error<Error_StaticNotNullDirective_ShouldHaveOneArgument>();
            }

            DesignatedDiagnostic<Error_StaticNotNullDirective_ArgumentMustBeLocation> designatedDiag;
            auto e_arg = TranslateSExpToMLoc(stmt->args[0], /*hintType*/ nullptr, /*bWrapExpAsLoc*/ false, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_arg);

            return Value<MStmt_NotNullDirective>(*e_arg);
        }
        
        throw NotImplementedException{}; // 인식할 수 없는 directive입니다
    }
};

expected<void, DiagPtr> TranslateSStmtToMStmts(vector<MStmt*>* outStmts, SStmt* sStmt, TranslationContexts& contexts)
{
    SStmtToMStmtsTranslator translator{outStmts, contexts};
    return Accept(translator, sStmt);
}

expected<void, DiagPtr> TranslateSEmbeddableStmtToMStmts(vector<MStmt*>* outStmts, SEmbeddableStmt* embedStmt, TranslationContexts& contexts)
{
    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    class EmbeddableStmtTranslator
    {
    public:
        using ResultType = expected<void, DiagPtr>;
        vector<MStmt*>* outStmts;
        TranslationContexts& contexts;

    public:
        EmbeddableStmtTranslator(vector<MStmt*>* outStmts, TranslationContexts& contexts)
            : outStmts{outStmts}, contexts{contexts}
        {
        }

        ResultType Visit(SEmbeddableStmt_Single* stmt)
        {
            // TODO: VarDecl은 등장하면 에러를 내도록 한다
            // 지금은 그냥 패스

            return TranslateSStmtToMStmts(outStmts, stmt->stmt, contexts);
        }

        ResultType Visit(SEmbeddableStmt_Block* stmt)
        {
            return TranslateSBodyToMStmts(outStmts, stmt->stmts, contexts);
        }
    };

    EmbeddableStmtTranslator translator{outStmts, contexts};
    return Accept(translator, embedStmt);
}

expected<vector<MStmt*>, DiagPtr> TranslateSEmbeddableStmtToMStmts(SEmbeddableStmt* embedStmt, TranslationContexts& contexts)
{
    vector<MStmt*> stmts;
    auto e_result = TranslateSEmbeddableStmtToMStmts(&stmts, embedStmt, contexts);
    RETURN_ON_ERROR(e_result);
    return stmts;
}

expected<vector<MStmt*>, DiagPtr> TranslateSForStmtInitializerToMStmts(SForStmtInitializer* forInit, TranslationContexts& contexts)
{
    class ForInitTranslator
    {
    public:
        using ResultType = expected<vector<MStmt*>, DiagPtr>;

    private:
        TranslationContexts& contexts;

    public:
        ForInitTranslator(TranslationContexts& contexts)
            : contexts{contexts}
        {
        }

        ResultType Visit(SForStmtInitializer_Exp* forInit)
        {
            DesignatedDiagnostic<Error_ForStmt_ExpInitializerShouldBeAssignOrCall> designatedDiag;
            auto e_exp = TranslateSExpAsTopLevelExpToMExp(forInit->exp, /*hintType*/ nullptr, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_exp);

            return vector<MStmt*>{contexts.mFactory->MakeMStmt<MStmt_Exp>(*e_exp)};
        }

        ResultType Visit(SForStmtInitializer_VarDecl* forInit)
        {   
            vector<MStmt*> stmts;
            auto e_stmtsResult = TranslateSVarDeclToMStmts(&stmts, &forInit->varDecl, contexts);
            RETURN_ON_ERROR(e_stmtsResult);

            return std::move(stmts); // for making ResultType
        }
    };

    ForInitTranslator translator{contexts};
    return Accept(translator, forInit);
}

expected<MExp*, DiagPtr> TranslateSExpAsTopLevelExpToMExp(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, TranslationContexts& contexts)
{
    auto e_nExp = TranslateSExpToMExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_nExp);

    if (!IsTopLevelRExp(*e_nExp))
    {
        return unexpected{designatedDiag->MakeDiag()};
    }

    return e_nExp;
}

tuple<vector<RFuncParameter>, bool> MakeParameters(vector<SLambdaExpParam>& sParams, TranslationContexts& contexts)
{
    bool bLastParamVariadic = false;
    size_t sParamCount = sParams.size();

    vector<RFuncParameter> rParams;
    rParams.reserve(sParamCount);
    for (size_t i = 0; i < sParamCount; i++)
    {
        auto& sParam = sParams[i];

        auto rParamKind = MakeParamKind(sParam.o_paramModifier);

        // 파라미터에 Type이 명시되어있지 않으면 hintType기반으로 inference 해야 한다.
        if (!sParam.type)
            throw NotImplementedException{};

        auto e_rParamType = contexts.scopeContext->TranslateSTypeExpToRType(sParam.type);
        // RETURN_ON_ERROR(e_rParamType);
        assert(false); // TODO: expected리턴 하도록 수정

        rParams.emplace_back(rParamKind, /*bRef*/false, *e_rParamType, RName_Normal(sParam.name));

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

NLambdaDeclAndArgs MakeLambdaDeclAndArgs(std::vector<MStmt*>&& body, TranslationContexts& contexts)
{
    throw NotImplementedException{};
}

expected<NLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToNLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, TranslationContexts& contexts)
{
    // 람다를 분석합니다
    // [int x = x](int p) => { return 3; }

    // 파라미터는 람다 함수의 지역변수로 취급한다
    // var newLambdaBodyContext = funcContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

    // 람다 관련 정보는 여기서 수집한다
    RFuncReturn funcRet = retType ? (RFuncReturn)RFuncReturn_Set{retType} : RFuncReturn_NotSet();

    auto [funcParams, bLastParamVariadic] = MakeParameters(sParams, contexts);

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

        newContexts.scopeContext->AddLocalVarInfo(*e_rParamType, RName_Normal{sParam.name});
    }

    vector<MStmt*> rBody;
    auto e_rBodyResult = TranslateSBodyToMStmts(&rBody, sBody, newContexts);
    RETURN_ON_ERROR(e_rBodyResult);

    // body분석을 했던것을 토대로 캡쳐한 변수들을 LambdaVarDecl로 만들고, 현재 context에서 전달할 argument로 만든다
    return MakeLambdaDeclAndArgs(move(rBody), newContexts);
}

} // namespace 

expected<void, DiagPtr> TranslateSBodyToMStmts(vector<MStmt*>* outBody, span<SStmt*> sStmts, TranslationContexts& contexts)
{
    for(auto* sStmt : sStmts)
    {
        SStmtToMStmtsTranslator translator{outBody, contexts};
        auto e_result = Accept(translator, sStmt);
        RETURN_ON_ERROR(e_result);
    }

    return {};
}

expected<vector<MStmt*>, DiagPtr> TranslateSBodyToMStmts(span<SStmt*> sStmts, TranslationContexts& contexts)
{
    vector<MStmt*> body;
    auto e_result = TranslateSBodyToMStmts(&body, sStmts, contexts);
    RETURN_ON_ERROR(e_result);
    return body;
}


}
