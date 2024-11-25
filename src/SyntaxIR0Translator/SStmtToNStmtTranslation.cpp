#include "pch.h"
#include "SStmtToNStmtTranslation.h"

#include <optional>
#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Infra/Variants.h>

#include <IR0/NStmt.h>
#include <IR0/NExp.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RFuncDecl.h>
#include <IR0/NLoc.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include "SExpToNExpTranslation.h"
#include "SVarDeclToNStmtsTranslation.h"
#include "SExpToNLocTranslation.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "FuncContext.h"
#include "DesignatedErrorLogger.h"
#include "Misc.h"
#include "RFuncAndRArgsToNExpTranslation.h"
#include <IR0/DeclWithOuterTypeArgs.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

bool TranslateSStmtToNStmts(SStmt& sStmt, std::vector<NStmtPtr>* outStmts, TranslationContext& context);
bool TranslateSEmbeddableStmtToNStmts(SEmbeddableStmt& embedStmt, std::vector<NStmtPtr>* outStmts, TranslationContext& context);
bool TranslateSForStmtInitializerToNStmts(SForStmtInitializer& forInit, std::vector<NStmtPtr>* outStmts, TranslationContext& context);
NExpPtr TranslateSExpAsTopLevelExpToNExp(SExp& sExp, const RTypePtr& hintType, IDesignatedErrorLogger* designatedErrorLogger, TranslationContext& context);
optional<NLambdaDeclAndArgs> TranslateSLambdaBodyToNLambdaAndArgs(const RTypePtr& retType, vector<SLambdaExpParam>& sParams, vector<SStmtPtr>& sBody, TranslationContext& context);

bool IsTopLevelRExp(NExp& exp)
{
    return dynamic_cast<NExp_CallInternalUnaryAssignOperator*>(&exp) != nullptr
        || dynamic_cast<NExp_Assign*>(&exp) != nullptr
        || dynamic_cast<NExp_CallGlobalFunc*>(&exp) != nullptr
        || dynamic_cast<NExp_CallClassFunc*>(&exp) != nullptr
        || dynamic_cast<NExp_CallStructFunc*>(&exp) != nullptr
        || dynamic_cast<NExp_CallLambda*>(&exp) != nullptr;
}

class SStmtToNStmtsTranslator : public SStmtVisitor
{
    bool* bOutFatal;
    vector<NStmtPtr>* outStmts;
    TranslationContext& context;
    
    void Fatal()
    {
        *bOutFatal = true;
    }

    void Valid(NStmtPtr&& stmt)
    {
        outStmts->push_back(std::move(stmt));
    }

    void Valid(vector<NStmtPtr>&& stmts)
    {
        outStmts->insert(outStmts->end(), make_move_iterator(stmts.begin()), make_move_iterator(stmts.end()));
    }

public:
    SStmtToNStmtsTranslator(bool* bOutFatal, vector<NStmtPtr>* outStmts, TranslationContext& context)
        : bOutFatal(bOutFatal), outStmts(outStmts), context(context)
    {
    }

    void Visit(SStmt_Command& stmt) override 
    {
        // CommandStmt에 있는 expStringElement를 분석한다

        vector<shared_ptr<NExp_String>> builder;

        for(auto& cmd : stmt.commands)
        {
            auto nStringExp = TranslateSStringExpToNStringExp(*cmd, context);
            if (!nStringExp) return Fatal();

            builder.push_back(nStringExp);
        }

        return Valid(MakePtr<NStmt_Command>(std::move(builder)));
    }

    void Visit(SStmt_VarDecl& stmt) override 
    {
        // int a;
        // auto x = 
        if (!TranslateSVarDeclToNStmts(stmt.varDecl, outStmts, context))
        {
            *bOutFatal = true;
            return;
        }
    }

    void Visit(SStmt_If& stmt) override 
    {
        // 순회
        auto nCond = TranslateSExpToNExp(*stmt.cond, /*hintType*/ context.MakeBoolType(), context);
        if (!nCond) return Fatal();

        // cast
        nCond = TryCastRExp(std::move(nCond), context.MakeBoolType(), context);
        if (!nCond)
        {
            context.Log(&Logger::Fatal_IfStmt_ConditionShouldBeBool);
            return Fatal();
        }

        auto nestedContext = context.MakeNestedScopeContext();

        vector<NStmtPtr> bodyStmts;
        if (!TranslateSEmbeddableStmtToNStmts(*stmt.body, &bodyStmts, nestedContext))
            return Fatal();

        optional<vector<NStmtPtr>> oElseStmts;
        if (stmt.elseBody != nullptr)
        {
            auto elseContext = context.MakeNestedScopeContext();

            vector<NStmtPtr> elseStmts;
            if (!TranslateSEmbeddableStmtToNStmts(*stmt.elseBody, &elseStmts, elseContext))
                return Fatal();

            oElseStmts = std::move(elseStmts);
        }

        return Valid(MakePtr<NStmt_If>(std::move(nCond), std::move(bodyStmts), std::move(*oElseStmts)));
    }

    void Visit(SStmt_IfTest& stmt) override 
    {
        auto varName = RName_Normal(stmt.varName);

        // if (Type varName = e) body         
        auto testType = context.TranslateSTypeExpToRType(*stmt.testType);

        auto target = TranslateSExpToNExp(*stmt.exp, /*hintType*/ nullptr, context);
        if (!target) return Fatal();

        auto bodyContext = context.MakeNestedScopeContext();
        bodyContext.AddLocalVarInfo(testType, varName);        

        vector<NStmtPtr> bodyStmts;
        if (!TranslateSEmbeddableStmtToNStmts(*stmt.body, &bodyStmts, bodyContext))
            return Fatal();

        optional<vector<NStmtPtr>> oElseStmts;
        if (stmt.elseBody)
        {
            auto elseContext = context.MakeNestedScopeContext();
            vector<NStmtPtr> elseStmts;
            if (!TranslateSEmbeddableStmtToNStmts(*stmt.elseBody, &elseStmts, elseContext))
                return Fatal();

            oElseStmts = std::move(elseStmts);
        }

        auto asExp = context.MakeNExp_As(std::move(target), testType);
        if (!target) return Fatal();

        auto testTypeKind = testType->GetCustomTypeKind();
        if (testTypeKind == RCustomTypeKind::Class || testTypeKind == RCustomTypeKind::Interface)
            return Valid(MakePtr<NStmt_IfNullableRefTest>(std::move(testType), std::move(varName), std::move(asExp), std::move(bodyStmts), std::move(*oElseStmts)));
        else if (testTypeKind == RCustomTypeKind::Enum)
            return Valid(MakePtr<NStmt_IfNullableValueTest>(std::move(testType), std::move(varName), std::move(asExp), std::move(bodyStmts), std::move(*oElseStmts)));
        else
            throw NotImplementedException(); // 에러
    }

    void Visit(SStmt_For& stmt) override 
    {
        // for(
        //     int i = 0; <- forStmtContext 
        //     i < 20; <- condition
        //     i++)
        // {
        // 
        // }
        auto forStmtContext = context.MakeNestedScopeContext(); // prelude는 loop가 아니다

        vector<NStmtPtr> initStmts;
        if (stmt.initializer)
        {   
            if (!TranslateSForStmtInitializerToNStmts(*stmt.initializer, &initStmts, forStmtContext))
                return Fatal();
        }

        NExpPtr condExp;
        if (stmt.cond)
        {
            auto boolType = context.MakeBoolType();
            auto rawCond = TranslateSExpToNExp(*stmt.cond, /*hintType*/ boolType, forStmtContext);
            if (!rawCond) return Fatal();

            condExp = TryCastRExp(std::move(rawCond), boolType, context);
            if (!condExp) return Fatal();
        }

        NExpPtr continueExp;
        if (stmt.cont)
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ForStmt_ContinueExpShouldBeAssignOrCall);
            continueExp = TranslateSExpAsTopLevelExpToNExp(*stmt.cont, /*hintType*/ nullptr, &designatedErrorLogger, forStmtContext);
            if (!continueExp) return Fatal();
        }

        auto bodyContext = forStmtContext.MakeNestedLoopScopeContext();

        vector<NStmtPtr> bodyStmts;
        if (!TranslateSEmbeddableStmtToNStmts(*stmt.body, &bodyStmts, bodyContext))
            return Fatal();

        return Valid(MakePtr<NStmt_For>(std::move(initStmts), std::move(condExp), std::move(continueExp), std::move(bodyStmts)));
    }

    void Visit(SStmt_Continue& stmt) override
    {
        if (!context.IsInLoop())
        {
            context.Log(&Logger::Fatal_ContinueStmt_ShouldUsedInLoop);
            return Fatal();
        }

        return Valid(MakePtr<NStmt_Continue>());
    }

    void Visit(SStmt_Break& stmt) override
    {
        if (!context.IsInLoop())
        {
            context.Log(&Logger::Fatal_BreakStmt_ShouldUsedInLoop);
            return Fatal();
        }

        return Valid(MakePtr<NStmt_Break>());
    }

    void Visit(SStmt_Return& stmt) override 
    {
        // seq 함수는 여기서 모두 처리 
        if (context.IsSeqFunc())
        {
            if (stmt.value)
            {
                context.Log(&Logger::Fatal_ReturnStmt_SeqFuncShouldReturnVoid);
                return Fatal();
            }

            return Valid(MakePtr<NStmt_Return>(nullptr));
        }

        // 리턴 값이 없을 경우
        
        auto funcRet = context.GetUnboundFuncReturn();

        return visit(overloaded {
            [this, &stmt](RFuncReturn_Set& set)
            {
                if (!stmt.value)
                {
                    // 생성자거나, void 함수가 아니라면 에러
                    if (set.type != context.MakeVoidType())
                    {
                        context.Log(&Logger::Fatal_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType);
                        return Fatal();
                    }

                    return Valid(MakePtr<NStmt_Return>(nullptr));
                }
                else
                {
                    // 리턴타입을 힌트로 사용한다
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    auto retValue = TranslateSExpToNExp(*stmt.value, /*hintType*/ set.type, context);
                    if (!retValue) return Fatal();

                    auto castRetValue = TryCastRExp(std::move(retValue), set.type, context);

                    // 캐스트 실패시
                    if (!castRetValue)
                    {
                        context.Log(&Logger::Fatal_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType);
                        return Fatal();
                    }

                    return Valid(MakePtr<NStmt_Return>(std::move(castRetValue)));
                }
            },

            [this, &stmt](RFuncReturn_NotSet& notset)
            {
                if (!stmt.value)
                {
                    // 이 함수는 void로 리턴을 확정 한다.
                    context.SetOpenFuncReturn(context.MakeVoidType());
                    return Valid(MakePtr<NStmt_Return>(nullptr));
                }
                else
                {
                    // 힌트타입 없이 분석
                    auto retValue = TranslateSExpToNExp(*stmt.value, /*hintType*/ nullptr, context);
                    if (!retValue) return Fatal();

                    // 리턴값이 안 적혀 있었으므로 적는다
                    context.SetOpenFuncReturn(context.GetType(*retValue));
                    return Valid(MakePtr<NStmt_Return>(std::move(retValue)));
                }
            },

            [this, &stmt](RFuncReturn_ForCtor& retType)
            {
                if (!stmt.value)
                {
                    return Valid(MakePtr<NStmt_Return>(nullptr));
                }
                else
                {   
                    throw NotImplementedException(); // 에러 처리
                    return Fatal();
                }
            }
        }, funcRet);
    }

    void Visit(SStmt_Block& stmt) override 
    {
        // { }
        bool bFatalLocal = false;
        auto blockContext = context.MakeNestedScopeContext();

        vector<NStmtPtr> builder;
        for(auto& stmt : stmt.stmts)
        {
            if (!TranslateSStmtToNStmts(*stmt, &builder, blockContext))
            {
                bFatalLocal = true;
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }
        }

        if (bFatalLocal) return Fatal();
        return Valid(MakePtr<NStmt_Block>(std::move(builder)));
    }

    void Visit(SStmt_Blank& stmt) override 
    {
        return Valid(MakePtr<NStmt_Blank>());
    }

    void Visit(SStmt_Exp& stmt) override
    {
        auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ExpStmt_ExpressionShouldBeAssignOrCall);
        auto exp = TranslateSExpAsTopLevelExpToNExp(*stmt.exp, /*hintType*/ nullptr, &designatedErrorLogger, context);
        if (!exp) return Fatal();

        return Valid(MakePtr<NStmt_Exp>(std::move(exp)));
    }

    void Visit(SStmt_Task& stmt) override 
    {
        vector<SLambdaExpParam> emptyParams;
        auto oLambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(context.MakeVoidType(), emptyParams, stmt.body, context);
        if (!oLambdaAndArgs) return Fatal();

        return Valid(MakePtr<NStmt_Task>(std::move(oLambdaAndArgs->decl), std::move(oLambdaAndArgs->args)));
    }

    void Visit(SStmt_Await& stmt) override 
    {
        auto newContext = context.MakeNestedScopeContext();
        vector<NStmtPtr> body;
        if (!TranslateSBodyToNStmts(stmt.body, &body, newContext))
            return Fatal();

        return Valid(MakePtr<NStmt_Await>(std::move(body)));
    }

    void Visit(SStmt_Async& stmt) override 
    {
        vector<SLambdaExpParam> emptyParams;
        auto oLambdaAndArgs = TranslateSLambdaBodyToNLambdaAndArgs(context.MakeVoidType(), emptyParams, stmt.body, context);
        if (!oLambdaAndArgs) return Fatal();

        return Valid(MakePtr<NStmt_Async>(std::move(oLambdaAndArgs->decl), std::move(oLambdaAndArgs->args)));
    }
    
    void Visit(SStmt_Foreach& stmt) override
    {
        struct ForeachStmtTranslator
        {
            SStmt_Foreach& stmt;
            vector<NStmtPtr>* outStmts;
            TranslationContext& context;

            RName itemVarName;

        public:
            ForeachStmtTranslator(SStmt_Foreach& stmt, vector<NStmtPtr>* outStmts, TranslationContext& context)
                : stmt(stmt), outStmts(outStmts), context(context)
            {
                itemVarName = RName_Normal(stmt.varName);
            }

            // syntax의 enumerableExp를 사용해서 enumerator를 가져오는 Exp를 생성한다
            NExpPtr MakeEnumeratorExp()
            {
                // TranslationResult<(Exp, IType)> Error() => TranslationResult.Error<(Exp, IType)>();
                auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

                auto nEnumerable = TranslateSExpToNLoc(*stmt.enumerable, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
                if (!nEnumerable) return nullptr;

                // GetEnumerator함수를 손으로 찾는다
                auto rEnumerableType = context.GetType(*nEnumerable);
                auto oRMember = rEnumerableType->GetMember(RNames::GetEnumerator, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!oRMember)
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException();
                    return nullptr;
                }

                vector<DeclWithOuterTypeArgs<RFuncDecl>> candidates;

                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*oRMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl.get();

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
                    throw NotImplementedException();
                    return nullptr;
                }

                if (candidates.size() != 1)
                {
                    // TODO: [15] foreach 에러 처리
                    throw NotImplementedException();
                    return nullptr;
                }

                auto& result = candidates[0];

                // 아까 갯수가 0인지 체크를 했으니 typeArgs는 default이다
                return TranslateRFuncAndNArgsToNExp(result.decl, result.outerTypeArgs, std::move(nEnumerable), {});
            }

            NExpPtr MakeNextExpAndInferItemVarType(const RTypePtr& enumeratorType)
            {
                auto oRMember = enumeratorType->GetMember(RNames::Next, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!oRMember) return nullptr;

                vector<NExpPtr> candidates;
                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*oRMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl.get();

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;

                    // typeParamCount가 0이라고 정했으면, 이 함수의 typeArgs는 outerTypeArgs
                    auto* typeArgs = funcDeclWithOuter.outerTypeArgs.get();

                    // 파라미터는 1개
                    if (funcDecl->GetParamCount() != 1) continue;

                    // 리턴 타입은 bool
                    auto ret = context.GetUnboundFuncReturn(*funcDecl, *typeArgs);
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != context.MakeBoolType()) continue;

                    // 인자는 out T*꼴이어야 한다
                    auto oParam = context.GetFuncParameter(*funcDecl, *typeArgs, 0);
                    if (!oParam) continue;
                    if (!oParam->bOut) continue;
                    auto* localPtrParamType = dynamic_cast<RType_LocalPtr*>(oParam->type.get());
                    if (!localPtrParamType) continue;

                    // $enumerator.GetNext(&i);
                    auto nArg = NArgument_Normal(MakePtr<NExp_LocalRef>(MakePtr<NLoc_LocalVar>(itemVarName, localPtrParamType->innerType)));
                    auto nEnumerator = MakePtr<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                    auto nextExp = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, std::move(nEnumerator), { std::move(nArg) });

                    candidates.push_back(std::move(nextExp));
                }

                if (candidates.size() == 1)
                {
                    return candidates[0];
                }
                else
                {
                    // TODO: [17] NextFunc가 여러개일때 처리
                    return nullptr;
                }
            }

            // NextExp를 만드는데, 캐스팅이 필요하면 CastInfo를 같이 돌려준다
            // (nextExp, (rawItemType, castExp)? castInfo)
            optional<tuple<NExpPtr, optional<tuple<RTypePtr, NExpPtr>>>> MakeNextExpAndCastExp(const RTypePtr& enumeratorType, const RTypePtr& itemTypeFromSyntax)
            {
                auto oRMember = enumeratorType->GetMember(RNames::Next, /*explicitTypeArgsExceptOuterCount*/ 0);
                if (!oRMember) return nullopt;

                vector<tuple<NExpPtr, optional<tuple<RTypePtr, NExpPtr>>>> candidates;
                for (auto& funcDeclWithOuter : GetFuncDeclWithOuterTypeArgs(*oRMember))
                {
                    auto* funcDecl = funcDeclWithOuter.decl.get();

                    if (funcDecl->GetParamCount() != 1) continue;

                    // 리턴 타입은 bool
                    auto ret = funcDecl->GetReturn();
                    auto* setRet = get_if<RFuncReturn_Set>(&ret);
                    assert(setRet);

                    if (setRet->type != context.MakeBoolType()) continue;

                    // TODO: [16] TypeResolver적용
                    if (funcDecl->GetTypeParamCount() != 0) continue;
                    auto* typeArgs = funcDeclWithOuter.outerTypeArgs.get();

                    // var symbol = (IFuncSymbol)context.InstantiateSymbol(outer, declSymbol, typeArgs: default);

                    // 인자는 out T*꼴이어야 한다
                    auto oParam = context.GetFuncParameter(*funcDecl, *typeArgs, 0);
                    if (!oParam) continue;
                    if (!oParam->bOut) continue;

                    auto* localPtrParamType = dynamic_cast<RType_LocalPtr*>(oParam->type.get());
                    if (!localPtrParamType) continue;

                    auto itemTypeFromNextParam = localPtrParamType->innerType;

                    if (itemTypeFromNextParam == itemTypeFromSyntax)
                    {
                        // $enumerator.GetNext(&i);
                        NArgument_Normal rArg(MakePtr<NExp_LocalRef>(MakePtr<NLoc_LocalVar>(itemVarName, itemTypeFromNextParam)));
                        auto nEnumerator = MakePtr<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);
                        auto nNext = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, std::move(nEnumerator), { std::move(rArg) });

                        candidates.push_back(make_tuple(std::move(nNext), nullopt));
                    }
                    else // 캐스팅
                    {
                        auto& rawItemType = itemTypeFromNextParam;
                        NArgument_Normal rArg(MakePtr<NExp_LocalRef>(MakePtr<NLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam)));
                        auto nEnumerator = MakePtr<NLoc_LocalVar>(RNames::Enumerator, enumeratorType);

                        // $enumerator.GetNext(&$rawItem)
                        auto nNext = TranslateRFuncAndNArgsToNExp(funcDeclWithOuter.decl, funcDeclWithOuter.outerTypeArgs, std::move(nEnumerator), { std::move(rArg) });

                        // $rawItem
                        auto rawItemExp = MakePtr<NExp_Load>(MakePtr<NLoc_LocalVar>(RNames::RawItem, itemTypeFromNextParam));
                        auto castExp = CastNExp(std::move(rawItemExp), itemTypeFromSyntax, context);
                        if (!castExp) // 캐스팅이 성공할때만 candidates에 넣기
                        {
                            candidates.push_back(make_tuple(std::move(nNext), make_tuple(rawItemType, castExp)));
                        }
                    }
                }

                size_t count = candidates.size();

                if (count == 0)
                {
                    // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                    throw NotImplementedException();
                    return nullopt;
                }
                else if (count == 1)
                {
                    return candidates[0];
                }
                else
                {
                    // TODO: [17] NextFunc가 0개 혹은 여러개일때 처리
                    throw NotImplementedException();
                    return nullopt;
                }
            }

            bool MakeBody(const RTypePtr& itemVarType, vector<NStmtPtr>* outBody)
            {
                // 루프 컨텍스트를 하나 열고
                auto bodyContext = context.MakeNestedLoopScopeContext();

                // 루프 컨텍스트에 로컬을 하나 추가하고 (enumerator는 추가해야 할까)
                bodyContext.AddLocalVarInfo(itemVarType, RName(itemVarName));

                // 본문 분석
                return TranslateSEmbeddableStmtToNStmts(*stmt.body, outBody, context);
            }

        public:
            bool Translate()
            {
                auto enumerator = MakeEnumeratorExp();
                if (!enumerator) return false;

                auto enumeratorType = context.GetType(*enumerator);

                if (!IsVarType(*stmt.type))
                {
                    auto itemType = context.TranslateSTypeExpToRType(*stmt.type);

                    auto oNextExpCastInfo = MakeNextExpAndCastExp(enumeratorType, itemType);
                    if (!oNextExpCastInfo) return false;

                    auto& [nextExp, oCastInfo] = *oNextExpCastInfo;

                    vector<NStmtPtr> body;
                    if (!MakeBody(itemType, &body)) return false;

                    if (!oCastInfo)
                    {
                        outStmts->push_back(MakePtr<NStmt_Foreach>(std::move(enumerator), std::move(itemType), itemVarName, std::move(nextExp), std::move(body)));
                    }
                    else
                    {
                        auto& [rawItemType, castExp] = *oCastInfo;
                        outStmts->push_back(MakePtr<NStmt_ForeachCast>(std::move(enumerator), std::move(itemType), itemVarName, std::move(rawItemType), std::move(nextExp), std::move(castExp), std::move(body)));
                    }
                }
                else // var 일 경우
                {
                    auto nextExp = MakeNextExpAndInferItemVarType(enumeratorType);
                    if (!nextExp) return false;

                    auto itemVarType = context.GetType(*nextExp);

                    vector<NStmtPtr> body;
                    if (!MakeBody(itemVarType, &body)) return false;

                    outStmts->push_back(MakePtr<NStmt_Foreach>(std::move(enumerator), std::move(itemVarType), itemVarName, std::move(nextExp), std::move(body)));
                }

                return true;
            }
        };

        ForeachStmtTranslator translator(stmt, outStmts, context);
        *bOutFatal = !translator.Translate();
    }

    void Visit(SStmt_Yield& stmt) override 
    {
        // TODO: ref 처리?
        if (!context.IsSeqFunc())
        {
            context.Log(&Logger::Fatal_YieldStmt_YieldShouldBeInSeqFunc);
            return Fatal();
        }

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        auto funcRet= context.GetUnboundFuncReturn();
        auto* setFuncRet = get_if<RFuncReturn_Set>(&funcRet);

        assert(setFuncRet); // 아닌 경우는 위에서 거른다 (sequence함수는 무조건 ret포함)

        // NOTICE: 리턴 타입을 힌트로 넣었다
        auto retValue = TranslateSExpToNExp(*stmt.value, /*hintType*/ setFuncRet->type, context);
        if (!retValue) return Fatal();

        auto castRetValue = CastNExp(std::move(retValue), setFuncRet->type, context);
        if (!castRetValue) return Fatal();

        return Valid(MakePtr<NStmt_Yield>(std::move(castRetValue)));
    }

    void Visit(SStmt_Directive& stmt) override 
    {
        if (stmt.name == "static_notnull")
        {
            if (stmt.args.size() != 1)
            {
                context.Log(&Logger::Fatal_StaticNotNullDirective_ShouldHaveOneArgument);
                return Fatal();
            }

            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_StaticNotNullDirective_ArgumentMustBeLocation);
            auto arg = TranslateSExpToNLoc(*stmt.args[0], /*hintType*/ nullptr, /*bWrapExpAsLoc*/ false, &designatedErrorLogger, context);
            if (!arg) return Fatal();

            return Valid(MakePtr<NStmt_NotNullDirective>(std::move(arg)));
        }
        
        throw NotImplementedException(); // 인식할 수 없는 directive입니다
    }
};

bool TranslateSStmtToNStmts(SStmt& sStmt, vector<NStmtPtr>* outStmts, TranslationContext& context)
{   
    bool bFatal;

    SStmtToNStmtsTranslator translator(&bFatal, outStmts, context);
    sStmt.Accept(translator);
    return !bFatal;
}

bool TranslateSEmbeddableStmtToNStmts(SEmbeddableStmt& embedStmt, vector<NStmtPtr>* outStmts, TranslationContext& context)
{
    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    class EmbeddableStmtTranslator : public SEmbeddableStmtVisitor
    {
        bool* bOutFatal;
        vector<NStmtPtr>* outStmts;
        TranslationContext& context;

    public:
        EmbeddableStmtTranslator(bool* bOutFatal, vector<NStmtPtr>* outStmts, TranslationContext& context)
            : bOutFatal(bOutFatal), outStmts(outStmts), context(context)
        {
        }

        void Visit(SEmbeddableStmt_Single& stmt) override
        {
            // TODO: VarDecl은 등장하면 에러를 내도록 한다
            // 지금은 그냥 패스
            if (!TranslateSStmtToNStmts(*stmt.stmt, outStmts, context))
                *bOutFatal = true;
        }

        void Visit(SEmbeddableStmt_Block& stmt) override
        {
            if (!TranslateSBodyToNStmts(stmt.stmts, outStmts, context))
                *bOutFatal = true;
        }
    };

    bool bFatal = false;
    EmbeddableStmtTranslator translator(&bFatal, outStmts, context);
    embedStmt.Accept(translator);
    return !bFatal;
}

bool TranslateSForStmtInitializerToNStmts(SForStmtInitializer& forInit, vector<NStmtPtr>* outStmts, TranslationContext& context)
{
    class ForInitTranslator : public SForStmtInitializerVisitor
    {
        bool* bOutFatal;
        vector<NStmtPtr>* outStmts;
        TranslationContext& context;

    public:
        ForInitTranslator(bool* bOutFatal, vector<NStmtPtr>* outStmts, TranslationContext& context)
            : bOutFatal(bOutFatal), outStmts(outStmts), context(context)
        {
        }

        void Visit(SForStmtInitializer_Exp& forInit) override
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ForStmt_ExpInitializerShouldBeAssignOrCall);
            auto exp = TranslateSExpAsTopLevelExpToNExp(*forInit.exp, /*hintType*/ nullptr, &designatedErrorLogger, context);
            if (!exp)
            {   
                *bOutFatal = true;
                return;
            }

            outStmts->push_back(MakePtr<NStmt_Exp>(std::move(exp)));
        }

        void Visit(SForStmtInitializer_VarDecl& forInit) override
        {   
            if (!TranslateSVarDeclToNStmts(forInit.varDecl, outStmts, context))
            {   
                *bOutFatal = true;
                return;
            }
        }
    };

    bool bFatal = false;
    vector<NStmtPtr> stmts;
    ForInitTranslator translator(&bFatal, &stmts, context);
    forInit.Accept(translator);
    return !bFatal;
}

NExpPtr TranslateSExpAsTopLevelExpToNExp(SExp& sExp, const RTypePtr& hintType, IDesignatedErrorLogger* designatedErrorLogger, TranslationContext& context)
{
    auto nExp = TranslateSExpToNExp(sExp, hintType, context);
    if (!nExp) return nullptr;

    if (!IsTopLevelRExp(*nExp))
    {
        designatedErrorLogger->Log();
        return nullptr;
    }

    return nExp;
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
            throw NotImplementedException();

        auto rParamType = context.TranslateSTypeExpToRType(*sParam.type);
        rParams.emplace_back(sParam.hasOut, std::move(rParamType), sParam.name);

        if (sParam.hasParams)
        {
            if (i == sParamCount - 1)
            {
                bLastParamVariadic = true;
            }
            else
            {
                throw NotImplementedException(); // 에러 처리. bVariadic은 마지막에 있어야 합니다
            }
        }
    }

    return make_tuple(std::move(rParams), bLastParamVariadic);
}

optional<NLambdaDeclAndArgs> TranslateSLambdaBodyToNLambdaAndArgs(const RTypePtr& retType, vector<SLambdaExpParam>& sParams, vector<SStmtPtr>& sBody, TranslationContext& context)
{
    // 람다를 분석합니다
    // [int x = x](int p) => { return 3; }

    // 파라미터는 람다 함수의 지역변수로 취급한다
    // var newLambdaBodyContext = funcContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

    // 람다 관련 정보는 여기서 수집한다
    RFuncReturn funcRet = retType ? (RFuncReturn)RFuncReturn_Set(std::move(retType)) : RFuncReturn_NotSet();

    auto [funcParams, bLastParamVariadic] = MakeParameters(sParams, context);

    // Lambda를 만들고 context 인스턴스 안에 저장한다
    // DeclSymbol tree로의 Commit은 함수 백트래킹이 다 끝났을 때 (그냥 Translation이 끝났을때 해도 될거 같다)
    auto newContext = context.MakeLambdaBodyContext(std::move(funcRet), std::move(funcParams), bLastParamVariadic); // 중첩된 bodyContext를 만들고, 새 scopeContext도 만든다

    // 람다 파라미터(int p)를 지역 변수로 추가한다
    for (auto& sParam : sParams)
    {
        // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
        if (!sParam.type)
        {
            context.Log(&Logger::Fatal_NotSupported_LambdaParameterInference);
            return nullopt;
        }

        auto rParamType = context.TranslateSTypeExpToRType(*sParam.type);

        auto name = RName_Normal(sParam.name);
        newContext.AddLocalVarInfo(rParamType, name);
    }

    vector<NStmtPtr> rBody;
    if (!TranslateSBodyToNStmts(sBody, &rBody, newContext)) return nullopt;

    // body분석을 했던것을 토대로 캡쳐한 변수들을 LambdaVarDecl로 만들고, 현재 context에서 전달할 argument로 만든다
    return newContext.MakeLambdaDeclAndArgs(std::move(rBody));
}

} // namespace 

bool TranslateSBodyToNStmts(const vector<SStmtPtr>& stmts, vector<NStmtPtr>* outStmts, TranslationContext& context)
{
    for(auto& stmt : stmts)
    {
        bool bFatal = false;
        SStmtToNStmtsTranslator translator(&bFatal, outStmts, context);
        stmt->Accept(translator);
        if (bFatal) return false;
    }

    return true;
}


}
