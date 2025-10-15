#include "NExpQInstsTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"

#include "IR0/NExp.h"
#include "IR1/QFactory.h"
#include "IR1/QBlock.h"
#include "IR1/QInsts.h"
#include "IR1/QValues.h"

#include "NLocQInstsTranslation.h"
#include "QBodyContext.h"


using namespace std;

namespace Citron {

namespace IR0IR1Translator {
// NExp는 QInst와 1:1로 대응되지는 않는다

class NExpQInstsTranslator
{
public:
    using ResultType = expected<QValue*, DiagPtr>;
        
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    NExpQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // load(loc),
    ResultType Visit(NExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eLV = TranslateNLocToQInsts(exp->loc, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // 새로운 value 도입
        auto* v = bodyContext->MakeValue();
        auto* inst = factory->MakeQInst_Load(v, *eLV);

        block->AddInst(inst);
        return v;
    }

    // Assign(loc dest, src exp)
    ResultType Visit(NExp_Assign* exp)
    {
        // dest를 먼저 계산한다
        auto eLV = TranslateNLocToQInsts(exp->dest, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // src계산
        auto eV = TranslateNExpToQInsts(exp->src, block, bodyContext, factory);
        if (!eV) return unexpected{eV.error()};

        auto* inst = factory->MakeQInst_Store(*eLV, *eV);
        block->AddInst(inst);

        // src를 평가한 값을 그대로 리턴
        return eV;
    }

    // box 3;
    // IR1에서 힙으로 값을 올리려면,
    // runtime 함수를 콜 하고,
    // 그 위치에 exp를 넣도록
    ResultType Visit(NExp_Box* exp)
    {
        // QInst_BoxAlloc(lv, RType), innerExp의 type
        //


        throw NotImplementedException{};
    }

    ResultType Visit(NExp_StaticBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ClassMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_StructIndirectMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_StructMemberBoxRef* exp) { throw NotImplementedException{}; }

    // LocalRef (Loc)
    ResultType Visit(NExp_LocalRef* exp)
    {
        // local ref는 &s.a 같은 걸 수 있다
        auto eLV = TranslateNLocToQInsts(exp->innerLoc, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // lv 그대로 리턴하면 될거 같다
        return *eLV;
    }

    ResultType Visit(NExp_BoolLiteral* exp)
    {
        return factory->MakeQValue_ConstBool(exp->value);
    }

    ResultType Visit(NExp_IntLiteral* exp)
    {
        return factory->MakeQValue_ConstInteger(exp->value);
    }

    ResultType Visit(NExp_String* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(NExp_List* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ListIterator* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallInternalUnaryOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallInternalUnaryAssignOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallInternalBinaryOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallGlobalFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NewClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallClassFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CastClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NewStruct* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallStructFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NewEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CastEnumElemToEnum* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NewNullable* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NullableValueNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_NullableRefNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_Lambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CallLambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_CastBoxedLambdaToFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_InlineBlock* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ClassIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ClassAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ClassIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_ClassAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_InterfaceIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_InterfaceAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_InterfaceIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_InterfaceAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_EnumIsEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(NExp_EnumAsEnumElem* exp) { throw NotImplementedException{}; }
};

expected<QValue*, DiagPtr> TranslateNExpToQInsts(NExp* nExp, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    NExpQInstsTranslator translator{block, bodyContext, factory};
    return Accept(translator, nExp);
}

} // namesapce IR0IR1Tranaslator

} // namespace Citron
