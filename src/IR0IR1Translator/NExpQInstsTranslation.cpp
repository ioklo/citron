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
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    NExpQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // load(loc),
    expected<QValue*, DiagPtr> Translate(NExp_Load& exp)
    {
        // 이 translation으로 lv가 하나 나올 것이다
        auto eLV = TranslateNLocToQInsts(exp.loc.get(), block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // 새로운 value 도입
        auto* v = bodyContext->MakeValue();
        auto* inst = factory->MakeQInst_Load(v, *eLV);

        block->AddInst(inst);
        return v;
    }

    // Assign(loc dest, src exp)
    expected<QValue*, DiagPtr> Translate(NExp_Assign& exp)
    {
        // dest를 먼저 계산한다
        auto eLV = TranslateNLocToQInsts(exp.dest.get(), block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // src계산
        auto eV = TranslateNExpToQInsts(exp.src.get(), block, bodyContext, factory);
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
    expected<QValue*, DiagPtr> Translate(NExp_Box& exp)
    {
        // QInst_BoxAlloc(lv, RType), innerExp의 type
        //


        throw NotImplementedException{};
    }

    expected<QValue*, DiagPtr> Translate(NExp_StaticBoxRef& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ClassMemberBoxRef& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_StructIndirectMemberBoxRef& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_StructMemberBoxRef& exp) { throw NotImplementedException{}; }

    // LocalRef (Loc)
    expected<QValue*, DiagPtr> Translate(NExp_LocalRef& exp)
    {
        // local ref는 &s.a 같은 걸 수 있다
        auto eLV = TranslateNLocToQInsts(exp.innerLoc.get(), block, bodyContext, factory);
        if (!eLV) return unexpected{eLV.error()};

        // lv 그대로 리턴하면 될거 같다
        return *eLV;
    }

    expected<QValue*, DiagPtr> Translate(NExp_BoolLiteral& exp)
    {
        return factory->MakeQValue_ConstBool(exp.value);
    }

    expected<QValue*, DiagPtr> Translate(NExp_IntLiteral& exp)
    {
        return factory->MakeQValue_ConstInteger(exp.value);
    }

    expected<QValue*, DiagPtr> Translate(NExp_String& exp)
    {
        throw NotImplementedException{};
    }

    expected<QValue*, DiagPtr> Translate(NExp_List& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ListIterator& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallInternalUnaryOperator& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallInternalUnaryAssignOperator& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallInternalBinaryOperator& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallGlobalFunc& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NewClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallClassFunc& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CastClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NewStruct& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallStructFunc& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NewEnumElem& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CastEnumElemToEnum& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NewNullable& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NullableValueNullLiteral& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_NullableRefNullLiteral& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_Lambda& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CallLambda& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_CastBoxedLambdaToFunc& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_InlineBlock& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ClassIsClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ClassAsClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ClassIsInterface& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_ClassAsInterface& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_InterfaceIsClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_InterfaceAsClass& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_InterfaceIsInterface& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_InterfaceAsInterface& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_EnumIsEnumElem& exp) { throw NotImplementedException{}; }
    expected<QValue*, DiagPtr> Translate(NExp_EnumAsEnumElem& exp) { throw NotImplementedException{}; }
};

class NExpQInstsTranslatorWrapper : public NExpVisitor
{
    expected<QValue*, DiagPtr>* result;
    NExpQInstsTranslator translator;

public:
    NExpQInstsTranslatorWrapper(expected<QValue*, DiagPtr>* result, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : result{result}
        , translator{block, bodyContext, factory}
    {}

    void Visit(NExp_Load& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_Assign& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_Box& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_StaticBoxRef& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ClassMemberBoxRef& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_StructIndirectMemberBoxRef& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_StructMemberBoxRef& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_LocalRef& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_BoolLiteral& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_IntLiteral& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_String& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_List& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ListIterator& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallInternalUnaryOperator& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallInternalUnaryAssignOperator& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallInternalBinaryOperator& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallGlobalFunc& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NewClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallClassFunc& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CastClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NewStruct& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallStructFunc& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NewEnumElem& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CastEnumElemToEnum& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NewNullable& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NullableValueNullLiteral& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_NullableRefNullLiteral& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_Lambda& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CallLambda& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_CastBoxedLambdaToFunc& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_InlineBlock& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ClassIsClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ClassAsClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ClassIsInterface& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_ClassAsInterface& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_InterfaceIsClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_InterfaceAsClass& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_InterfaceIsInterface& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_InterfaceAsInterface& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_EnumIsEnumElem& exp) override { *result = translator.Translate(exp); }
    void Visit(NExp_EnumAsEnumElem& exp) override { *result = translator.Translate(exp); }
};

expected<QValue*, DiagPtr> TranslateNExpToQInsts(NExp* nExp, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    expected<QValue*, DiagPtr> result;
    NExpQInstsTranslatorWrapper translator{&result, block, bodyContext, factory};
    nExp->Accept(translator);

    return result;
}

} // namesapce IR0IR1Tranaslator

} // namespace Citron
