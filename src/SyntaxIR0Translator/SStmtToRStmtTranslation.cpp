#include "pch.h"
#include "SStmtToRStmtTranslation.h"

#include <optional>
#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Infra/Variants.h>

#include <IR0/RStmt.h>
#include <IR0/RExp.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RFuncDecl.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include "SExpToRExpTranslation.h"
#include "SVarDeclToRStmtsTranslation.h"
#include "SExpToRLocTranslation.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "BodyContext.h"
#include "DesignatedErrorLogger.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct RLambdaDeclAndArgs
{
    shared_ptr<RLambdaDecl> decl;
    shared_ptr<RArgument> args;   // constructor args
};

optional<vector<RStmtPtr>> TranslateSStmtToRStmts(SStmt& sStmt, TranslationContext& context);
optional<vector<RStmtPtr>> TranslateSEmbeddableStmtToRStmts(SEmbeddableStmt& embedStmt, TranslationContext& context);
optional<vector<RStmtPtr>> TranslateSForStmtInitializerToRStmts(SForStmtInitializer& forInit, TranslationContext& context);
RExpPtr TranslateSExpAsTopLevelExpToRExp(SExp& sExp, const RTypePtr& hintType, IDesignatedErrorLogger* designatedErrorLogger, TranslationContext& context);
optional<RLambdaDeclAndArgs> TranslateSLambdaBodyToRLambdaAndArgs(const RTypePtr& retType, vector<SLambdaExpParam>& sParams, vector<SStmtPtr>& sBody, TranslationContext& context);

bool IsTopLevelRExp(RExp& exp)
{
    return dynamic_cast<RExp_CallInternalUnaryAssignOperator*>(&exp) != nullptr
        || dynamic_cast<RExp_Assign*>(&exp) != nullptr
        || dynamic_cast<RExp_CallGlobalFunc*>(&exp) != nullptr
        || dynamic_cast<RExp_CallClassMemberFunc*>(&exp) != nullptr
        || dynamic_cast<RExp_CallStructMemberFunc*>(&exp) != nullptr
        || dynamic_cast<RExp_CallLambda*>(&exp) != nullptr;
}

class SStmtToRStmtsTranslator : public SStmtVisitor
{
    vector<RStmtPtr>* result;
    bool* bFatal;
    TranslationContext& context;
    
    void Fatal()
    {
        *bFatal = true;
    }

    void Valid(RStmtPtr&& stmt)
    {
        result->push_back(std::move(stmt));
    }

    void Valid(vector<RStmtPtr>&& stmts)
    {
        result->insert(result->end(), make_move_iterator(stmts.begin()), make_move_iterator(stmts.end()));
    }

public:
    SStmtToRStmtsTranslator(vector<RStmtPtr>* result, bool* bFatal, TranslationContext& context)
        : result(result), bFatal(bFatal), context(context)
    {
    }

    void Visit(SStmt_Command& stmt) override 
    {
        // CommandStmt에 있는 expStringElement를 분석한다

        vector<shared_ptr<RExp_String>> builder;

        for(auto& cmd : stmt.commands)
        {
            auto rStringExp = TranslateSStringExpToRStringExp(*cmd, context);
            if (!rStringExp) return Fatal();

            builder.push_back(rStringExp);
        }

        return Valid(MakePtr<RStmt_Command>(std::move(builder)));
    }

    void Visit(SStmt_VarDecl& stmt) override 
    {
        // int a;
        // auto x = 
        auto rStmts = TranslateSVarDeclToRStmt(stmt.varDecl, context);
        if (!rStmts) return Fatal();

        return Valid(std::move(*rStmts));
    }

    void Visit(SStmt_If& stmt) override 
    {
        // 순회
        auto rCond = TranslateSExpToRExp(*stmt.cond, /*hintType*/ context.factory->MakeBoolType(), context);
        if (!rCond) return Fatal();

        // cast
        rCond = TryCastRExp(std::move(rCond), context.factory->MakeBoolType(), context);
        if (!rCond)
        {
            context.logger->Fatal_IfStmt_ConditionShouldBeBool();
            return Fatal();
        }

        auto bodyContext = context.MakeNestedScopeContext();
        auto oBodyStmts = TranslateSEmbeddableStmtToRStmts(*stmt.body, bodyContext);
        if (!oBodyStmts) return Fatal();

        optional<vector<RStmtPtr>> oElseStmts;
        if (stmt.elseBody != nullptr)
        {
            auto elseContext = context.MakeNestedScopeContext();
            oElseStmts = TranslateSEmbeddableStmtToRStmts(*stmt.elseBody, elseContext);
            if (!oElseStmts) return Fatal();
        }

        return Valid(MakePtr<RStmt_If>(std::move(rCond), std::move(*oBodyStmts), std::move(*oElseStmts)));
    }

    void Visit(SStmt_IfTest& stmt) override 
    {
        auto varName = RName_Normal(stmt.varName);

        // if (Type varName = e) body         
        auto testType = context.scopeContext->MakeType(*stmt.testType, *context.factory);

        auto target = TranslateSExpToRExp(*stmt.exp, /*hintType*/ nullptr, context);
        if (!target) return Fatal();

        auto bodyContext = context.MakeNestedScopeContext();
        bodyContext.scopeContext->AddLocalVarInfo(testType, varName);        

        auto oBodyStmts = TranslateSEmbeddableStmtToRStmts(*stmt.body, bodyContext);
        if (!oBodyStmts) return Fatal();

        optional<vector<RStmtPtr>> oElseStmts;
        if (stmt.elseBody)
        {
            auto elseContext = context.MakeNestedScopeContext();
            oElseStmts = TranslateSEmbeddableStmtToRStmts(*stmt.elseBody, elseContext);
            if (!oElseStmts) return Fatal();
        }

        auto asExp = MakeRExp_As(std::move(target), testType, *context.factory);
        if (!target) return Fatal();

        auto testTypeKind = testType->GetCustomTypeKind();
        if (testTypeKind == RCustomTypeKind::Class || testTypeKind == RCustomTypeKind::Interface)
            return Valid(MakePtr<RStmt_IfNullableRefTest>(std::move(testType), std::move(varName), std::move(asExp), std::move(*oBodyStmts), std::move(*oElseStmts)));
        else if (testTypeKind == RCustomTypeKind::Enum)
            return Valid(MakePtr<RStmt_IfNullableValueTest>(std::move(testType), std::move(varName), std::move(asExp), std::move(*oBodyStmts), std::move(*oElseStmts)));
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

        optional<vector<RStmtPtr>> initStmts;
        if (stmt.initializer)
        {
            initStmts = TranslateSForStmtInitializerToRStmts(*stmt.initializer, forStmtContext);
            if (!initStmts) return Fatal();
        }

        RExpPtr condExp;
        if (stmt.cond)
        {
            auto boolType = context.factory->MakeBoolType();
            auto rawCond = TranslateSExpToRExp(*stmt.cond, /*hintType*/ boolType, forStmtContext);
            if (!rawCond) return Fatal();

            condExp = TryCastRExp(std::move(rawCond), boolType, context);
            if (!condExp) return Fatal();
        }

        RExpPtr continueExp;
        if (stmt.cont)
        {
            DesignatedErrorLogger errorLogger(*context.logger, &Logger::Fatal_ForStmt_ContinueExpShouldBeAssignOrCall);
            continueExp = TranslateSExpAsTopLevelExpToRExp(*stmt.cont, /*hintType*/ nullptr, &errorLogger, forStmtContext);
            if (!continueExp) return Fatal();
        }

        auto bodyContext = forStmtContext.MakeNestedLoopScopeContext();

        auto bodyStmts = TranslateSEmbeddableStmtToRStmts(*stmt.body, bodyContext);
        if (!bodyStmts) return Fatal();

        return Valid(MakePtr<RStmt_For>(std::move(initStmts), std::move(condExp), std::move(continueExp), std::move(bodyStmts)));
    }

    void Visit(SStmt_Continue& stmt) override
    {
        if (!context.scopeContext->IsInLoop())
        {
            context.logger->Fatal_ContinueStmt_ShouldUsedInLoop();
            return Fatal();
        }

        return Valid(MakePtr<RStmt_Continue>());
    }

    void Visit(SStmt_Break& stmt) override
    {
        if (!context.scopeContext->IsInLoop())
        {
            context.logger->Fatal_BreakStmt_ShouldUsedInLoop();
            return Fatal();
        }

        return Valid(MakePtr<RStmt_Break>());
    }

    void Visit(SStmt_Return& stmt) override 
    {
        // seq 함수는 여기서 모두 처리 
        if (context.bodyContext->bSeqFunc)
        {
            if (stmt.value)
            {
                context.logger->Fatal_ReturnStmt_SeqFuncShouldReturnVoid();
                return Fatal();
            }

            return Valid(MakePtr<RStmt_Return>(nullptr));
        }

        // 리턴 값이 없을 경우
        
        auto funcRet = context.bodyContext->GetFuncReturn();

        return visit(overloaded {
            [this, &stmt](RFuncReturn_Set& set)
            {
                if (!stmt.value)
                {
                    // 생성자거나, void 함수가 아니라면 에러
                    if (set.type != context.factory->MakeVoidType())
                    {
                        context.logger->Fatal_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType();
                        return Fatal();
                    }

                    return Valid(MakePtr<RStmt_Return>(nullptr));
                }
                else
                {
                    // 리턴타입을 힌트로 사용한다
                    // 현재 함수 시그니처랑 맞춰서 같은지 확인한다
                    auto retValue = TranslateSExpToRExp(*stmt.value, /*hintType*/ set.type, context);
                    if (!retValue) return Fatal();

                    auto castRetValue = TryCastRExp(std::move(retValue), set.type, context);

                    // 캐스트 실패시
                    if (!castRetValue)
                    {
                        context.logger->Fatal_ReturnStmt_MismatchBetweenReturnValueAndFuncReturnType();
                        return Fatal();
                    }

                    return Valid(MakePtr<RStmt_Return>(std::move(castRetValue)));
                }
            },

            [this, &stmt](RFuncReturn_NotSet& notset)
            {
                if (!stmt.value)
                {
                    // 이 함수는 void로 리턴을 확정 한다.
                    context.bodyContext->SetFuncReturn(context.factory->MakeVoidType());
                    return Valid(MakePtr<RStmt_Return>(nullptr));
                }
                else
                {
                    // 힌트타입 없이 분석
                    auto retValue = TranslateSExpToRExp(*stmt.value, /*hintType*/ nullptr, context);
                    if (!retValue) return Fatal();

                    // 리턴값이 안 적혀 있었으므로 적는다
                    context.bodyContext->SetFuncReturn(retValue->GetType(*context.factory));
                    return Valid(MakePtr<RStmt_Return>(std::move(retValue)));
                }
            },

            [this, &stmt](RFuncReturn_ForConstructor& retType)
            {
                if (!stmt.value)
                {
                    return Valid(MakePtr<RStmt_Return>(nullptr));
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

        vector<RStmtPtr> builder;
        for(auto& stmt : stmt.stmts)
        {
            auto oInnerStmts = TranslateSStmtToRStmts(*stmt, blockContext);
            if (!oInnerStmts)
            {
                bFatalLocal = true;
                continue; // 중간에 에러가 발생해도, 로그를 수집하기 위해서 일단 계속 진행한다
            }

            result->insert(result->end(), make_move_iterator(oInnerStmts->begin()), make_move_iterator(oInnerStmts->end()));
        }

        if (bFatalLocal) return Fatal();

        return Valid(MakePtr<RStmt_Block>(std::move(builder)));
    }

    void Visit(SStmt_Blank& stmt) override 
    {
        return Valid(MakePtr<RStmt_Blank>());
    }

    void Visit(SStmt_Exp& stmt) override
    {
        DesignatedErrorLogger errorLogger(*context.logger, &Logger::Fatal_ExpStmt_ExpressionShouldBeAssignOrCall);

        auto exp = TranslateSExpAsTopLevelExpToRExp(*stmt.exp, /*hintType*/ nullptr, &errorLogger, context);
        if (!exp) return Fatal();

        return Valid(MakePtr<RStmt_Exp>(exp));
    }

    void Visit(SStmt_Task& stmt) override 
    {
        vector<SLambdaExpParam> emptyParams;
        auto oLambdaAndArgs = TranslateSLambdaBodyToRLambdaAndArgs(context.factory->MakeVoidType(), emptyParams, stmt.body, context);
        if (!oLambdaAndArgs) return Fatal();

        return Valid(MakePtr<RStmt_Task>(std::move(oLambdaAndArgs->decl), std::move(oLambdaAndArgs->args)));
    }

    void Visit(SStmt_Await& stmt) override 
    {
        auto newContext = context.scopeContext->MakeNestedScopeContext();
        auto oBody = TranslateSBodyToRStmts(stmt.body, newContext);
        if (!oBody) return Fatal();

        return Valid(MakePtr<RStmt_Await>(std::move(oBody)));
    }

    void Visit(SStmt_Async& stmt) override 
    {
        vector<SLambdaExpParam> emptyParams;
        auto oLambdaAndArgs = TranslateSLambdaBodyToRLambdaAndArgs(context.factory->MakeVoidType(), emptyParams, stmt.body, context);
        if (!oLambdaAndArgs) return Fatal();

        return Valid(MakePtr<RStmt_Async>(std::move(oLambdaAndArgs->decl), std::move(oLambdaAndArgs->args)));
    }

    void Visit(SStmt_Foreach& stmt) override
    {
        return new ForeachStmtBuilder(foreachStmt, context).Build();
    }

    void Visit(SStmt_Yield& stmt) override 
    {
        // TODO: ref 처리?
        if (!context.bodyContext->bSeqFunc)
        {
            context.logger->Fatal_YieldStmt_YieldShouldBeInSeqFunc();
            return Fatal();
        }

        // yield에서는 retType이 명시되는 경우만 있을 것이다
        auto funcRet= context.bodyContext->GetFuncReturn();
        auto* setFuncRet = get_if<RFuncReturn_Set>(&funcRet);

        assert(setFuncRet); // 아닌 경우는 위에서 거른다 (sequence함수는 무조건 ret포함)

        // NOTICE: 리턴 타입을 힌트로 넣었다
        auto retValue = TranslateSExpToRExp(*stmt.value, /*hintType*/ setFuncRet->type, context);
        if (!retValue) return Fatal();

        auto castRetValue = CastRExp(std::move(retValue), setFuncRet->type, context);
        if (!castRetValue) return Fatal();

        return Valid(MakePtr<RStmt_Yield>(castRetValue));
    }

    void Visit(SStmt_Directive& stmt) override 
    {
        if (stmt.name == "static_notnull")
        {
            if (stmt.args.size() != 1)
            {
                context.logger->Fatal_StaticNotNullDirective_ShouldHaveOneArgument();
                return Fatal();
            }

            DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_StaticNotNullDirective_ArgumentMustBeLocation);
            auto arg = TranslateSExpToRLoc(*stmt.args[0], /*hintType*/ nullptr, /*bWrapExpAsLoc*/ false, &designatedErrorLogger, context);
            if (!arg) return Fatal();

            return Valid(MakePtr<RStmt_NotNullDirective>(std::move(arg)));
        }
        
        throw NotImplementedException(); // 인식할 수 없는 directive입니다
    }
};

optional<vector<RStmtPtr>> TranslateSStmtToRStmts(SStmt& sStmt, TranslationContext& context)
{
    vector<RStmtPtr> rStmts;
    bool bFatal;

    SStmtToRStmtsTranslator translator(&rStmts, &bFatal, context);
    sStmt.Accept(translator);

    if (bFatal) return nullopt;
    return rStmts;
}

optional<vector<RStmtPtr>> TranslateSEmbeddableStmtToRStmts(SEmbeddableStmt& embedStmt, TranslationContext& context)
{
    // if (...) 'stmt'
    // if (...) '{ stmt... }' 를 받는다
    class EmbeddableStmtTranslator : public SEmbeddableStmtVisitor
    {
        optional<vector<RStmtPtr>>* result;
        TranslationContext& context;

    public:
        EmbeddableStmtTranslator(optional<vector<RStmtPtr>>* result, TranslationContext& context)
            : result(result), context(context)
        {
        }

        void Visit(SEmbeddableStmt_Single& stmt) override
        {
            // TODO: VarDecl은 등장하면 에러를 내도록 한다
            // 지금은 그냥 패스
            *result = TranslateSStmtToRStmts(*stmt.stmt, context);
        }

        void Visit(SEmbeddableStmt_Block& stmt) override
        {
            *result = TranslateSBodyToRStmts(stmt.stmts, context);
        }
    };

    optional<vector<RStmtPtr>> result;
    EmbeddableStmtTranslator translator(&result, context);
    embedStmt.Accept(translator);
    return result;
}

optional<vector<RStmtPtr>> TranslateSForStmtInitializerToRStmts(SForStmtInitializer& forInit, TranslationContext& context)
{
    class ForInitTranslator : public SForStmtInitializerVisitor
    {
        optional<vector<RStmtPtr>>* result;
        TranslationContext& context;

    public:
        ForInitTranslator(optional<vector<RStmtPtr>>* result, TranslationContext& context)
            : result(result), context(context)
        {
        }

        void Visit(SForStmtInitializer_Exp& forInit) override
        {
            DesignatedErrorLogger designatedErrorLogger(*context.logger, &Logger::Fatal_ForStmt_ExpInitializerShouldBeAssignOrCall);
            auto exp = TranslateSExpAsTopLevelExpToRExp(*forInit.exp, /*hintType*/ nullptr, &designatedErrorLogger, context);
            if (!exp)
            {
                *result = nullopt;
                return;
            }

            *result = vector<RStmtPtr> { MakePtr<RStmt_Exp>(exp) };
        }

        void Visit(SForStmtInitializer_VarDecl& forInit) override
        {
            *result = TranslateSVarDeclToRStmts(forInit.varDecl, context);
        }
    };

    optional<vector<RStmtPtr>> stmts;
    ForInitTranslator translator(&stmts, context);
    forInit.Accept(translator);
    return stmts;
}

RExpPtr TranslateSExpAsTopLevelExpToRExp(SExp& sExp, const RTypePtr& hintType, IDesignatedErrorLogger* designatedErrorLogger, TranslationContext& context)
{
    auto rExp = TranslateSExpToRExp(sExp, hintType, context);
    if (!rExp) return nullptr;

    if (!IsTopLevelRExp(*rExp))
    {
        designatedErrorLogger->Log();
        return nullptr;
    }

    return rExp;
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

        auto rParamType = context.scopeContext->MakeType(*sParam.type, *context.factory);
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

    make_tuple(std::move(rParams), bLastParamVariadic);
}

optional<RLambdaDeclAndArgs> TranslateSLambdaBodyToRLambdaAndArgs(const RTypePtr& retType, vector<SLambdaExpParam>& sParams, vector<SStmtPtr>& sBody, TranslationContext& context)
{
    // 람다를 분석합니다
    // [int x = x](int p) => { return 3; }

    // 파라미터는 람다 함수의 지역변수로 취급한다
    // var newLambdaBodyContext = bodyContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

    // 람다 관련 정보는 여기서 수집한다
    RFuncReturn funcRet = retType ? (RFuncReturn)RFuncReturn_Set(std::move(retType)) : RFuncReturn_NotSet();

    auto [funcParams, bLastParamVariadic] = MakeParameters(sParams, context);

    // Lambda를 만들고 context 인스턴스 안에 저장한다
    // DeclSymbol tree로의 Commit은 함수 백트래킹이 다 끝났을 때 (그냥 Translation이 끝났을때 해도 될거 같다)
    auto [newContext, lambda] = context.MakeLambdaBodyContext(std::move(funcRet), funcParams, bLastParamVariadic); // 중첩된 bodyContext를 만들고, 새 scopeContext도 만든다

    auto lambdaName = context.bodyContext->NewLambdaName();
    auto rLambdaDecl = MakePtr<RLambdaDecl>(context.bodyContext->curFuncDecl, lambdaName);
    vector<RLambdaMemberVarDecl> memberVarDecls;

    // 람다 파라미터(int p)를 지역 변수로 추가한다
    for (auto& sParam : sParams)
    {
        // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
        if (!sParam.type)
        {
            context.logger->Fatal_NotSupported_LambdaParameterInference();
            return nullopt;
        }

        auto rParamType = context.MakeType(*sParam.type, factory);

        auto name = RName_Normal(sParam.name);
        memberVarDecls.emplace_back(rLambdaDecl, rParamType, name);

        newContext.AddLocalVarInfo(rParamType, name);
    }

    auto oRBodyStmts = TranslateSBodyToRStmts(sBody, *newContext);
    if (!oRBodyStmts) return nullopt;

    auto args = newContext->MakeLambdaArgs();
    rLambdaDecl->Init(std::move(memberVarDecls), std::move(funcRet), std::move(funcParams), bLastParamVariadic, std::move(*oRBodyStmts));

    return RLambdaDeclAndArgs { std::move(rLambaDecl), std::move(args) };
}

} // namespace 


optional<vector<RStmtPtr>> TranslateSBodyToRStmts(const vector<SStmtPtr>& stmts, TranslationContext& context)
{
    vector<RStmtPtr> builder; // keep appending

    for(auto& stmt : stmts)
    {
        bool bFatal = false;
        SStmtToRStmtsTranslator translator(&builder, &bFatal, context);
        stmt->Accept(translator);
        if (bFatal) return nullopt;
    }

    return builder;
}


}
