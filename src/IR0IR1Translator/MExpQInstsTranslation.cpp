#include "MExpQInstsTranslation.h"

#include <expected>

#include "Infra/Exceptions.h"

#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"
#include "QIR/QValues.h"

#include "MLocQInstsTranslation.h"
#include "QBodyContext.h"


using namespace std;

namespace Citron {

namespace IR0IR1Translator {
// MExp는 QInst와 1:1로 대응되지는 않는다

class MExpQInstsTranslator
{
public:
    using ResultType = expected<QValue*, DiagPtr>;
        
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    MExpQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // load(loc),
    ResultType Visit(MExp_Load* exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eLV = TranslateMLocToQInsts(exp->loc, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // 새로운 value 도입
        auto* v = bodyContext->MakeValue();
        auto* inst = factory->MakeQInst_Load(v, *eLV);

        block->AddInst(inst);
        return v;
    }

    // Assign(loc dest, src exp)
    ResultType Visit(MExp_Assign* exp)
    {
        // dest를 먼저 계산한다
        auto eLV = TranslateMLocToQInsts(exp->dest, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // src계산
        auto eV = TranslateMExpToQInsts(exp->src, block, bodyContext, factory);
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
    ResultType Visit(MExp_Box* exp)
    {
        // QInst_BoxAlloc(lv, RType), innerExp의 type
        //


        throw NotImplementedException{};
    }

    ResultType Visit(MExp_StaticBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructIndirectMemberBoxRef* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_StructMemberBoxRef* exp) { throw NotImplementedException{}; }

    // LocalRef (Loc)
    ResultType Visit(MExp_LocalRef* exp)
    {
        // local ref는 &s.a 같은 걸 수 있다
        auto eLV = TranslateMLocToQInsts(exp->innerLoc, block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // lv 그대로 리턴하면 될거 같다
        return *eLV;
    }

    ResultType Visit(MExp_BoolLiteral* exp)
    {
        return factory->MakeQValue_ConstBool(exp->value);
    }

    ResultType Visit(MExp_IntLiteral* exp)
    {
        return factory->MakeQValue_ConstInteger(exp->value);
    }

    ResultType Visit(MExp_String* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(MExp_List* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ListIterator* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallInternalUnaryOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallInternalUnaryAssignOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallInternalBinaryOperator* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallGlobalFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallClassFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewStruct* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallStructFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastEnumElemToEnum* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NewNullable* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableValueNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_NullableRefNullLiteral* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_Lambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CallLambda* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_CastBoxedLambdaToFunc* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InlineBlock* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_ClassAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsClass* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceIsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_InterfaceAsInterface* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumIsEnumElem* exp) { throw NotImplementedException{}; }
    ResultType Visit(MExp_EnumAsEnumElem* exp) { throw NotImplementedException{}; }
};

expected<QValue*, DiagPtr> TranslateMExpToQInsts(MExp* mExp, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    MExpQInstsTranslator translator{block, bodyContext, factory};
    return Accept(translator, mExp);
}

} // namesapce IR0IR1Tranaslator

} // namespace Citron
