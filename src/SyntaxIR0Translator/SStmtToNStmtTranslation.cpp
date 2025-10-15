#include "SStmtToNStmtTranslation.h"

#include <optional>
#include <variant>
#include <cassert>
#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"
#include "Logging/Diag.h"

#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "IR0/DeclWithOuterTypeArgs.h"
#include "IR0/RFuncReturn.h"
#include "IR0/RTypes.h"
#include "IR0/RFuncDecl.h"
#include "IR0/NStmt.h"
#include "IR0/NExp.h"
#include "IR0/NLoc.h"

#include "SExpToNExpTranslation.h"
#include "SVarDeclToNStmtsTranslation.h"
#include "SExpToNLocTranslation.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "FuncContext.h"
#include "DesignatedDiagnostic.h"
#include "Misc.h"
#include "RFuncAndRArgsToNExpTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

expected<void, DiagPtr> TranslateSStmtToNStmts(std::vector<NStmt*>* outStmts, SStmt* sStmt, TranslationContext& context);
expected<void, DiagPtr> TranslateSEmbeddableStmtToNStmts(std::vector<NStmt*>* outStmts, SEmbeddableStmt* embedStmt, TranslationContext& context);
expected<vector<NStmt*>, DiagPtr> TranslateSEmbeddableStmtToNStmts(SEmbeddableStmt* embedStmt, TranslationContext& context);
expected<vector<NStmt*>, DiagPtr> TranslateSForStmtInitializerToNStmts(SForStmtInitializer* forInit, TranslationContext& context);
expected<NExp*, DiagPtr> TranslateSExpAsTopLevelExpToNExp(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, TranslationContext& context);
expected<NLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToNLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, TranslationContext& context);

bool IsTopLevelRExp(NExp* exp)
{
    return dynamic_cast<NExp_CallInternalUnaryAssignOperator*>(exp) != nullptr
        || dynamic_cast<NExp_Assign*>(exp) != nullptr
        || dynamic_cast<NExp_CallGlobalFunc*>(exp) != nullptr
        || dynamic_cast<NExp_CallClassFunc*>(exp) != nullptr
        || dynamic_cast<NExp_CallStructFunc*>(exp) != nullptr
        || dynamic_cast<NExp_CallLambda*>(exp) != nullptr;
}

class SStmtToNStmtsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;

private:
    vector<NStmt*>* outStmts;
    TranslationContext& context;

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

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, NStmt>
    ResultType Value(TArgs&&... args)
    {
        outStmts->push_back(context.MakeNStmt<TValue>(forward<TArgs>(args)...));
        return {};
    }

    ResultType Values(vector<NStmt*>&& stmts)
    {
        outStmts->insert(outStmts->end(), make_move_iterator(stmts.begin()), make_move_iterator(stmts.end()));
        return {};
    }

public:
    SStmtToNStmtsTranslator(vector<NStmt*>* outStmts, TranslationContext& context)
        : outStmts{outStmts}, context{context}
    {
    }

    ResultType Visit(SStmt_Command* stmt) 
    {
        // CommandStmt에 있는 expStringElement를 분석한다

        vector<NExp_String*> builder;

        for(auto* cmd : stmt->commands)
        {
            auto eNStringExp = TranslateSStringExpToNStringExp(cmd, context);
            if (!eNStringExp) return Error(move(eNStringExp));

            builder.push_back(*eNStringExp);
        }

        return Value<NStmt_Command>(move(builder));
    }

    ResultType Visit(SStmt_VarDecl* stmt) 
    {
        // int a;
        // auto x = 
        return TranslateSVarDeclToNStmts(outStmts, &stmt->varDecl, context);
    }

    ResultType Visit(SStmt_If* stmt) 
    {
        // 순회
        auto eNCond = TranslateSExpToNExp(stmt->cond, /*hintType*/ context.MakeBoolType(), context);
        if (!eNCond) return Error(move(eNCond));

        // cast
        eNCond = CastNExp(*eNCond, context.MakeBoolType(), context);
        if (!eNCond) return Error<Error_IfStmt_ConditionShouldBeBool>();

        auto nestedContext = context.MakeNestedScopeContext();
        
        auto eBodyStmts = TranslateSEmbeddableStmtToNStmts(stmt->body, nestedContext);
        if (!eBodyStmts) return Error(move(eBodyStmts));

        vector<NStmt*> elseStmts;
        if (stmt->elseBody != nullptr)
        {
            auto elseContext = context.MakeNestedScopeContext();
            
            auto eElseResult = TranslateSEmbeddableStmtToNStmts(stmt->elseBody, elseContext);
            if (!eElseResult) return Error(move(eElseResult));

            elseStmts = move(*eElseResult);
        }

        return Value<NStmt_If>(*eNCond, move(*eBodyStmts), move(elseStmts));
    }

    ResultType Visit(SStmt_IfTest* stmt) 
    {
        auto varName = RName_Normal(stmt->varName);

        // if (Type varName = e) eBody         
        auto eRTestType = context.TranslateSTypeExpToRType(stmt->testType);

        auto eNTarget = TranslateSExpToNExp(stmt->exp, /*hintType*/ nullptr, context);
        if (!eNTarget) return Error(move(eNTarget));

        auto bodyContext = context.MakeNestedScopeContext();
        bodyContext.AddLocalVarInfo(*eRTestType, varName);
        
        auto eBodyStmts = TranslateSEmbeddableStmtToNStmts(stmt->body, bodyContext);
        if (!eBodyStmts) return Error(move(eBodyStmts));

        vector<NStmt*> elseStmts;
        if (stmt->elseBody)
        {
            auto elseContext = context.MakeNestedScopeContext();            
            auto elseResult = TranslateSEmbeddableStmtToNStmts(stmt->elseBody, elseContext);

            if (!elseResult)
                return Error(move(elseResult));

            elseStmts = move(*elseResult);
        }

        auto eNAsExp = context.MakeNExp_As(*eNTarget, *eRTestType);
        if (!eNAsExp) return Error(move(eNAsExp));

        auto rTestTypeKind = (*eRTestType)->GetCustomTypeKind();
        if (rTestTypeKind == RCustomTypeKind::Class || rTestTypeKind == RCustomTypeKind::Interface)
            return Value<NStmt_IfNullableRefTest>(*eRTestType, move(varName), *eNAsExp, move(*eBodyStmts), move(elseStmts));
        else if (rTestTypeKind == RCustomTypeKind::Enum)
            return Value<NStmt_IfNullableValueTest>(*eRTestType, move(varName), *eNAsExp, move(*eBodyStmts), move(elseStmts));
        else
            throw NotImplementedException{}; // 에러
    }

    ResultType Visit(SStmt_For* stmt) 
    {
        // for(
        //     int i = 0; <- forStmtContext 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }
        auto forStmtContext = context.MakeNestedScopeContext(); // prelude는 loop가 아니다

        vector<NStmt*> initStmts;
        if (stmt->initializer)
        {   
            auto eInitResult = TranslateSForStmtInitializerToNStmts(stmt->initializer, forStmtContext);
            if (!eInitResult) return Error(move(eInitResult));

            initStmts = move(*eInitResult);
        }

        NExp* condExp;
        if (stmt->cond)
        {
            auto boolType = context.MakeBoolType();
            auto eRawCond = TranslateSExpToNExp(stmt->cond, /*hintType*/ boolType, forStmtContext);
            if (!eRawCond) return Error(move(eRawCond));

            eRawCond = CastNExp(*eRawCond, boolType, context);
            if (!eRawCond) return Error(move(eRawCond));

            condExp = *eRawCond;
        }

        NExp* continueExp;
        if (stmt->cont)
        {
            DesignatedDiagnostic<Error_ForStmt_ContinueExpShouldBeAssignOrCall> designatedDiag;
            auto eContResult = TranslateSExpAsTopLevelExpToNExp(stmt->cont, /*hintType*/ nullptr, &designatedDiag, forStmtContext);
            if (!eContResult) return Error(move(eContResult));

            continueExp = *eContResult;
        }

        auto bodyContext = forStmtContext.MakeNestedLoopScopeContext();
        
        auto eBodyStmts = TranslateSEmbeddableStmtToNStmts(stmt->body, bodyContext);
        if (!eBodyStmts) return Error(move(eBodyStmts));

        return Value<NStmt_For>(move(initStmts), condExp, continueExp, move(*eBodyStmts));
    }

    ResultType Visit(SStmt_Continue* stmt)
    {
        if (!context.IsInLoop())
        {
            return Error<Error_ContinueStmt_ShouldUsedInLoop>();
        }

        return Value<NStmt_Continue>();
    }

    ResultType Visit(SStmt_Break* stmt)
    {
        if (!context.IsInLoop())
        {
            return Error<Error_BreakStmt_ShouldUsedInLoop>();
        }

        return Value<NStmt_Break>();
    }

    ResultType Visit(SStmt_Return* stmt) 
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (stmt->value)
            {
                return Error<Error_ReturnStmt_SeqFuncShouldReturnVoid>();
            }

            return Value<NStmt_Return>(nullptr);
        }

        // 리턴 값이 없을 경우
        
        auto funcRet = context.GetUnboundFuncReturn();

        return visit(overloaded {
            [this, &stmt](RFuncReturn_Set& set)
            {
                if (!stmt->value)
                {
                    // 생성자거나, void 함수가 아니라면 에러
                    if (set.type != context.MakeVoidType())
                    {
                        return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();
                    }

                    return Value<NStmt_Return>(nullptr);
                }
                else
                {
                    // 리턴타입을 힌트로 사용한다
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    auto eRetValue = TranslateSExpToNExp(stmt->value, /*hintType*/ set.type, context);
                    if (!eRetValue) return Error(move(eRetValue));

                    auto castRetValue = CastNExp(*eRetValue, set.type, context);

                    // 캐스트 실패시
                    if (!castRetValue)
                        return Error<Error_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType>();

                    return Value<NStmt_Return>(*castRetValue);
                }
            },

            [this, &stmt](RFuncReturn_NotSet& notset)
            {
                if (!stmt->value)
                {
                    // 이 함수는 void로 리턴을 확정 한다.
                    context.SetOpenFuncReturn(context.MakeVoidType());
                    return Value<NStmt_Return>(nullptr);
                }
                else
                {
                    // 힌트타입 없이 분석
                    auto eRetValue = TranslateSExpToNExp(stmt->value, /*hintType*/ nullptr, context);
                    if (!eRetValue) return Error(move(eRetValue));

                    // 리턴값이 안 적혀 있었으므로 적는다
                    context.SetOpenFuncReturn(context.GetType(*eRetValue));
                    return Value<NStmt_Return>(*eRetValue);
                }
            },

            [this, &stmt](RFuncReturn_ForCtor& retType)
            {
                if (!stmt->value)
                {
                    return Value<NStmt_Return>(nullptr);
                }
                else
                {   
                    throw NotImplementedException{}; // 에러 처리
                    // return Error();
                }
            }
        }, funcRet);
    }

    ResultType Visit(SStmt_Block* stmt) 
    {
        // { }
        vector<DiagPtr> diags;
        auto blockContext = context.MakeNestedScopeContext();

        vector<NStmt*> builder;
        for(auto* stmt : stmt->stmts)
        {
            auto eStmtResult = TranslateSStmtToNStmts(&builder, stmt, blockContext);
            if (!eStmtResult)
            {
                diags.push_back(move(eStmtResult).error());
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }
        }
        
        if (!diags.empty()) return Error<AggregateDiag>(move(diags));
        return Value<NStmt_Block>(move(builder));
    }

    ResultType Visit(SStmt_Blank* stmt) 
    {
        return Value<NStmt_Blank>();
    }

    ResultType Visit(SStmt_Exp* stmt)
    {
        DesignatedDiagnostic<Error_ExpStmt_ExpressionShouldBeAssignOrCall> designatedDiag;
        auto eExp = TranslateSExpAsTopLevelExpToNExp(stmt->exp, /*hintType*/ nullptr, &designatedDiag, context);
        if (!eExp) return Error(move(eExp));

        return Value<NStmt_Exp>(*eExp);
    }

    ResultType Visit(SStmt_Task* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto eLambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(context.MakeVoidType(), emptyParams, stmt->body, context);
        if (!eLambdaAndArgs) return Error(move(eLambdaAndArgs));

        return Value<NStmt_Task>(eLambdaAndArgs->decl, move(eLambdaAndArgs->args));
    }

    ResultType Visit(SStmt_Await* stmt) 
    {
        auto newContext = context.MakeNestedScopeContext();
        auto eBody = TranslateSBodyToNStmts(stmt->body, newContext);
        if (!eBody) return Error(move(eBody));

        return Value<NStmt_Await>(move(*eBody));
    }

    ResultType Visit(SStmt_Async* stmt) 
    {
        vector<SLambdaExpParam> emptyParams;
        auto eLambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(context.MakeVoidType(), emptyParams, stmt->body, context);
        if (!eLambdaAndArgs) return Error(move(eLambdaAndArgs));

        return Value<NStmt_Async>(eLambdaAndArgs->decl, move(eLambdaAndArgs->args));
    }
    
    ResultType Visit(SStmt_Foreach* stmt)
    {
        struct ForeachStmtTranslator
        {
            vector<NStmt*>* outStmts;
            SStmt_Foreach* stmt;
            TranslationContext& context;

            RName itemVarName;

        public:
            ForeachStmtTranslator(vector<NStmt*>* outStmts, SStmt_Foreach* stmt, TranslationContext& context)
                : outStmts(outStmts), stmt(stmt), context(context)
            {
                itemVarName = RName_Normal(stmt->varName);
            }

            // syntax의 enumerableExp를 사용해서 enumerator를 가져오는 Exp를 생성한다
            expected<NExp*, DiagPtr> MakeEnumeratorExp()
            {
                // TranslationResult<(Exp, IType)> Error() => TranslationResult.Error<(Exp, IType)>();
                DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

                auto eNEnumerable = TranslateSExpToNLoc(stmt->enumerable, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
                if (!eNEnumerable) return unexpected{move(eNEnumerable).error()};

                // GetEnumerator함수를 손으로 찾는다
                auto rEnumerableType = context.GetType(*eNEnumerable);
                auto oRMember = rEnumerableType->GetMember(RNames::GetEnumerator, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!oRMember)
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException{};
                    return unexpected{MakePtr<Error_NotImplemented>()};
                }

                vector<DeclWithOuterTypeArgs<RFuncDecl>> candidates;

                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*oRMember))
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
                return TranslateRFuncAndNArgsToNExp(result.decl, result.outerTypeArgs, *eNEnumerable, {}, context);
            }

            expected<NExp*, DiagPtr> MakeNextExpAndInferItemVarType(RType* enumeratorType)
            {
                auto oRMember = enumeratorType->GetMember(RNames::Next, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!oRMember) return unexpected{MakePtr<Error_NotImplemented>()};

                vector<NExp*> candidates;
                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*oRMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl;

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // typeParamCount가 0이라고 정했으면, 이 함수의 typeArgs는 outerTypeArgs
                    auto* typeArgs = funcDeclWithOuter.outerTypeArgs;

                    // 파라미터는 1개
                    if (funcDecl->GetParamCount() != 1) continue;

                    // 리턴 타입은 bool
                    auto ret = context.GetFuncReturn(*funcDecl, *typeArgs);
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != context.MakeBoolType()) continue;

                    // 인자는 out T*꼴이어야 한다
                    auto param = context.GetFuncParam(*funcDecl, *typeArgs, 0);
                    if (!param.bOut) continue;

                    auto* localPtrParamType = dynamic_cast<RType_LocalPtr*>(param.type);
                    if (!localPtrParamType) continue;

                    // $enumerator.GetNext(&i);
                    auto nArg = NArgument_Normal(context.MakeNExp<NExp_LocalRef>(context.MakeNLoc<NLoc_LocalVar>(itemVarName, localPtrParamType->innerType)));
                    auto* nEnumerator = context.MakeNLoc<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                    auto eNextExp = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, { move(nArg) }, context);
                    if (!eNextExp) return unexpected{move(eNextExp).error()};

                    candidates.push_back(*eNextExp);
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
                NExp* castExp;
            };

            struct NextExpAndCastExp
            {
                NExp* nextExp;
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
                    auto ret = context.GetFuncReturn(*funcDecl, *typeArgs);
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != context.MakeBoolType()) continue;

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // var symbol = (IFuncSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs: default);

                    // 인자는 out T*꼴이어야 한다
                    auto param = context.GetFuncParam(*funcDecl, *typeArgs, 0);
                    if (!param.bOut) continue;

                    auto* localPtrParamType = dynamic_cast<RType_LocalPtr*>(param.type);
                    if (!localPtrParamType) continue;

                    auto itemTypeFromNextParam = localPtrParamType->innerType;

                    if (itemTypeFromNextParam == itemTypeFromSyntax)
                    {
                        // $enumerator.GetNext(&i);
                        NArgument_Normal rArg(context.MakeNExp<NExp_LocalRef>(context.MakeNLoc<NLoc_LocalVar>(itemVarName, itemTypeFromNextParam)));
                        auto* nEnumerator = context.MakeNLoc<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                        auto nNext = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, {move(rArg)}, context);

                        candidates.emplace_back(*nNext, nullopt);
                    }
                    else // 캐스팅
                    {
                        auto& rawItemType = itemTypeFromNextParam;
                        NArgument_Normal rArg(context.MakeNExp<NExp_LocalRef>(context.MakeNLoc<NLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam)));
                        auto* nEnumerator = context.MakeNLoc<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);

                        // $enumerator.GetNext(&$rawItem)
                        auto eNNext = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, nEnumerator, { move(rArg) }, context);
                        if (!eNNext) return unexpected{move(eNNext).error()};

                        // $rawItem
                        auto* rawItemExp = context.MakeNExp<NExp_Load>(context.MakeNLoc<NLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam));
                        auto castExp = CastNExp(rawItemExp, itemTypeFromSyntax, context);
                        if (castExp) // 캐스팅이 성공할때만 candidates에 넣기
                        {
                            candidates.emplace_back(*eNNext, CastInfo{rawItemType, *castExp});
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

            expected<vector<NStmt*>, DiagPtr> MakeBody(RType* itemVarType)
            {
                // 루프 컨텍스트를 하나 열고
                auto bodyContext = context.MakeNestedLoopScopeContext();

                // 루프 컨텍스트에 로컬을 하나 추가하고 (enumerator는 추가해야 할까)
                bodyContext.AddLocalVarInfo(itemVarType, RName(itemVarName));

                // 본문 분석
                return TranslateSEmbeddableStmtToNStmts(stmt->body, context);
            }

        public:
            expected<void, DiagPtr> Translate()
            {
                auto eEnumerator = MakeEnumeratorExp();
                if (!eEnumerator) return unexpected{move(eEnumerator).error()};

                auto enumeratorType = context.GetType(*eEnumerator);

                if (!IsVarType(stmt->type))
                {
                    auto eItemType = context.TranslateSTypeExpToRType(stmt->type);
                    if (!eItemType) return unexpected{move(eItemType).error()};

                    auto eNextExpCastInfo = MakeNextExpAndCastExp(enumeratorType, *eItemType);
                    if (!eNextExpCastInfo) return unexpected{move(eNextExpCastInfo).error()};

                    auto& [nextExp, oCastInfo] = *eNextExpCastInfo;

                    auto eBody = MakeBody(*eItemType);
                    if (!eBody) return unexpected{move(eBody).error()};

                    if (!oCastInfo)
                    {
                        outStmts->push_back(context.MakeNStmt<NStmt_Foreach>(*eEnumerator, *eItemType, itemVarName, nextExp, move(*eBody)));
                    }
                    else
                    {
                        auto& [rawItemType, castExp] = *oCastInfo;
                        outStmts->push_back(context.MakeNStmt<NStmt_ForeachCast>(*eEnumerator, *eItemType, itemVarName, rawItemType, nextExp, castExp, move(*eBody)));
                    }
                }
                else // var 일 경우
                {
                    auto eNextExp = MakeNextExpAndInferItemVarType(enumeratorType);
                    if (!eNextExp) return unexpected{move(eNextExp).error()};

                    auto itemVarType = context.GetType(*eNextExp);

                    auto eBody = MakeBody(itemVarType);
                    if (!eBody) return unexpected{move(eBody).error()};

                    outStmts->push_back(context.MakeNStmt<NStmt_Foreach>(*eEnumerator, itemVarType, itemVarName, *eNextExp, move(*eBody)));
                }

                return {};
            }
        };

        ForeachStmtTranslator translator{outStmts, stmt, context};
        return translator.Translate();
    }

    ResultType Visit(SStmt_Yield* stmt) 
    {
        // TODO: ref 처리?
        if (!context.IsSeqFunc())
        {
            return Error<Error_YieldStmt_YieldShouldBeInSeqFunc>();
        }

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        auto funcRet= context.GetUnboundFuncReturn();
        auto* setFuncRet = get_if<RFuncReturn_Set>(&funcRet);

        assert(setFuncRet); // 아닌 경우는 위에서 거른다 (sequence함수는 무조건 ret포함)

        // NOTICE: 리턴 타입을 힌트로 넣었다
        auto eRetValue = TranslateSExpToNExp(stmt->value, /*hintType*/ setFuncRet->type, context);
        if (!eRetValue) return Error(move(eRetValue));

        auto eCastRetValue = CastNExp(*eRetValue, setFuncRet->type, context);
        if (!eCastRetValue) return Error(move(eCastRetValue));

        return Value<NStmt_Yield>(*eCastRetValue);
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
            auto eArg = TranslateSExpToNLoc(stmt->args[0], /*hintType*/ nullptr, /*bWrapExpAsLoc*/ false, &designatedDiag, context);
            if (!eArg) return Error(move(eArg));

            return Value<NStmt_NotNullDirective>(*eArg);
        }
        
        throw NotImplementedException{}; // 인식할 수 없는 directive입니다
    }
};

expected<void, DiagPtr> TranslateSStmtToNStmts(vector<NStmt*>* outStmts, SStmt* sStmt, TranslationContext& context)
{
    SStmtToNStmtsTranslator translator{outStmts, context};
    return Accept(translator, sStmt);
}

expected<void, DiagPtr> TranslateSEmbeddableStmtToNStmts(vector<NStmt*>* outStmts, SEmbeddableStmt* embedStmt, TranslationContext& context)
{
    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    class EmbeddableStmtTranslator
    {
    public:
        using ResultType = expected<void, DiagPtr>;
        vector<NStmt*>* outStmts;
        TranslationContext& context;

    public:
        EmbeddableStmtTranslator(vector<NStmt*>* outStmts, TranslationContext& context)
            : outStmts{outStmts}, context{context}
        {
        }

        ResultType Visit(SEmbeddableStmt_Single* stmt)
        {
            // TODO: VarDecl은 등장하면 에러를 내도록 한다
            // 지금은 그냥 패스

            return TranslateSStmtToNStmts(outStmts, stmt->stmt, context);
        }

        ResultType Visit(SEmbeddableStmt_Block* stmt)
        {
            return TranslateSBodyToNStmts(outStmts, stmt->stmts, context);
        }
    };

    EmbeddableStmtTranslator translator{outStmts, context};
    return Accept(translator, embedStmt);
}

expected<vector<NStmt*>, DiagPtr> TranslateSEmbeddableStmtToNStmts(SEmbeddableStmt* embedStmt, TranslationContext& context)
{
    vector<NStmt*> stmts;
    auto eResult = TranslateSEmbeddableStmtToNStmts(&stmts, embedStmt, context);
    if (!eResult) return unexpected{move(eResult).error()};
    return stmts;
}

expected<vector<NStmt*>, DiagPtr> TranslateSForStmtInitializerToNStmts(SForStmtInitializer* forInit, TranslationContext& context)
{
    class ForInitTranslator
    {
    public:
        using ResultType = expected<vector<NStmt*>, DiagPtr>;

    private:
        TranslationContext& context;

    public:
        ForInitTranslator(TranslationContext& context)
            : context{context}
        {
        }

        ResultType Visit(SForStmtInitializer_Exp* forInit)
        {
            DesignatedDiagnostic<Error_ForStmt_ExpInitializerShouldBeAssignOrCall> designatedDiag;
            auto eExp = TranslateSExpAsTopLevelExpToNExp(forInit->exp, /*hintType*/ nullptr, &designatedDiag, context);
            if (!eExp)
            {   
                return unexpected{move(eExp).error()};
            }

            return vector<NStmt*>{context.MakeNStmt<NStmt_Exp>(*eExp)};
        }

        ResultType Visit(SForStmtInitializer_VarDecl* forInit)
        {   
            vector<NStmt*> stmts;
            auto eStmtsResult = TranslateSVarDeclToNStmts(&stmts, &forInit->varDecl, context);
            if (!eStmtsResult)
            {
                return unexpected{move(eStmtsResult).error()};
            }

            return std::move(stmts); // for making ResultType
        }
    };

    ForInitTranslator translator{context};
    return Accept(translator, forInit);
}

expected<NExp*, DiagPtr> TranslateSExpAsTopLevelExpToNExp(SExp* sExp, RType* hintType, IDesignatedDiagnostic* designatedDiag, TranslationContext& context)
{
    auto eNExp = TranslateSExpToNExp(sExp, hintType, context);
    if (!eNExp) return unexpected{move(eNExp).error()};

    if (!IsTopLevelRExp(*eNExp))
    {
        return unexpected{designatedDiag->MakeDiag()};
    }

    return eNExp;
}

tuple<vector<RFuncParameter>, bool> MakeParameters(vector<SLambdaExpParam>& sParams, TranslationContext& context)
{
    bool bLastParamVariadic = false;
    size_t sParamCount = sParams.size();

    vector<RFuncParameter> rParams;
    rParams.reserve(sParamCount);
    for (size_t i = 0; i < sParamCount; i++)
    {
        auto& sParam = sParams[i];

        // 파라미터에 Type이 명시되어있지 않으면 hintType기반으로 inference 해야 한다.
        if (!sParam.type)
            throw NotImplementedException{};

        auto eRParamType = context.TranslateSTypeExpToRType(sParam.type);
        rParams.emplace_back(sParam.hasOut, *eRParamType, RName_Normal(sParam.name));

        if (sParam.hasParams)
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

expected<NLambdaDeclAndArgs, DiagPtr> TranslateSLambdaBodyToNLambdaAndArgs(RType* retType, vector<SLambdaExpParam>& sParams, vector<SStmt*>& sBody, TranslationContext& context)
{
    // 람다를 분석합니다
    // [int x = x](int p) => { return 3; }

    // 파라미터는 람다 함수의 지역변수로 취급한다
    // var newLambdaBodyContext = funcContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

    // 람다 관련 정보는 여기서 수집한다
    RFuncReturn funcRet = retType ? (RFuncReturn)RFuncReturn_Set{retType} : RFuncReturn_NotSet();

    auto [funcParams, bLastParamVariadic] = MakeParameters(sParams, context);

    // Lambda를 만들고 context 인스턴스 안에 저장한다
    // DeclSymbol tree로의 Commit은 함수 백트래킹이 다 끝났을 때 (그냥 Translation이 끝났을때 해도 될거 같다)
    auto newContext = context.MakeLambdaBodyContext(move(funcRet), move(funcParams), bLastParamVariadic); // 중첩된 bodyContext를 만들고, 새 scopeContext도 만든다

    // 람다 파라미터(int p)를 지역 변수로 추가한다
    for (auto& sParam : sParams)
    {
        // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
        if (!sParam.type)
        {
            return unexpected{MakePtr<Error_NotSupported_LambdaParameterInference>()};
        }

        auto eRParamType = context.TranslateSTypeExpToRType(sParam.type);
        if (!eRParamType) return unexpected{move(eRParamType).error()};

        auto name = RName_Normal(sParam.name);
        newContext.AddLocalVarInfo(*eRParamType, name);
    }

    vector<NStmt*> rBody;
    auto eRBodyResult = TranslateSBodyToNStmts(&rBody, sBody, newContext);
    if (!eRBodyResult) return unexpected{move(eRBodyResult).error()};

    // body분석을 했던것을 토대로 캡쳐한 변수들을 LambdaVarDecl로 만들고, 현재 context에서 전달할 argument로 만든다
    return newContext.MakeLambdaDeclAndArgs(move(rBody));
}

} // namespace 

expected<void, DiagPtr> TranslateSBodyToNStmts(vector<NStmt*>* outBody, const vector<SStmt*>& sStmts, TranslationContext& context)
{
    for(auto* sStmt : sStmts)
    {
        SStmtToNStmtsTranslator translator{outBody, context};
        auto eResult = Accept(translator, sStmt);
        if (!eResult) return unexpected{move(eResult).error()};
    }

    return {};
}

expected<vector<NStmt*>, DiagPtr> TranslateSBodyToNStmts(const vector<SStmt*>& sStmts, TranslationContext& context)
{
    vector<NStmt*> body;
    auto eResult = TranslateSBodyToNStmts(&body, sStmts, context);
    if (!eResult) return unexpected{move(eResult).error()};
    return body;
}


}
