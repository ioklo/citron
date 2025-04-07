module Citron.SyntaxIR0Translator:ImExpToIrExpTranslation;

import <expected>;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.RDecls;
import Citron.NDecls;

import :ImExp;
import :IrExp;

import :TranslationContext;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct ImExpToIrExpTranslator : public ImExpVisitor
{
    expected<IrExpPtr, DiagPtr>* result;
    TranslationContext& context;
    RTypeFactory& factory;

    ImExpToIrExpTranslator(expected<IrExpPtr, DiagPtr>* result, TranslationContext& context, RTypeFactory& factory)
        : result(result), context(context), factory(factory)
    {
    }

    void Value(IrExpPtr&& nExp)
    {
        *result = std::move(nExp);
    }

    void Error(const DiagPtr& diag)
    {
        *result = unexpected{diag};
    }


    void Visit(ImExp_Namespace& imExp) override
    {
        return Value(MakePtr<IrExp_Namespace>(imExp._namespace));
    }

    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        static_assert(false);
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        return Value(MakePtr<IrExp_TypeVar>(imExp.type));
    }

    void Visit(ImExp_Class& imExp) override
    {
        return Value(MakePtr<IrExp_Class>(imExp.classDecl, imExp.typeArgs));
    }

    void Visit(ImExp_ClassFuncs& imExp) override
    {
        static_assert(false);
    }

    void Visit(ImExp_Struct& imExp) override
    {
        return Value(MakePtr<IrExp_Struct>(imExp.structDecl, imExp.typeArgs));
    }

    void Visit(ImExp_StructFuncs& imExp) override
    {
        static_assert(false);
        // return Error();
    }

    void Visit(ImExp_Enum& imExp) override
    {
        return Value(MakePtr<IrExp_Enum>(imExp.decl, imExp.typeArgs));
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        static_assert(false);
        // return Error()
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    void Visit(ImExp_ThisVar& imExp) override
    {
        return Value(MakePtr<IrExp_ThisVar>());
    }

    // &id
    void Visit(ImExp_LocalVar& imExp) override
    {
        return Value(MakePtr<IrExp_LocalRef>(MakePtr<NLoc_LocalVar>(RName_Normal(imExp.name), imExp.type)));
    }

    // &x
    void Visit(ImExp_LambdaVar& imExp) override
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다
        return Value(MakePtr<IrExp_LocalRef>(MakePtr<NLoc_LambdaVar>(imExp.decl, imExp.typeArgs)));
    }

    // x (C.x, this.x)
    void Visit(ImExp_ClassVar& imExp) override
    {
        if (imExp.decl->IsStatic()) // &C.x
        {
            return Value(MakePtr<IrExp_StaticRef>(MakePtr<NLoc_ClassVar>(nullptr, imExp.decl, imExp.typeArgs)));
        }
        else // &this.x
        {
            // auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            return Value(MakePtr<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(), imExp.decl, imExp.typeArgs));
        }
    }

    // x (S.x, this->x)
    void Visit(ImExp_StructVar& imExp) override
    {
        if (imExp.decl->IsStatic())
        {
            return Value(MakePtr<IrExp_StaticRef>(MakePtr<NLoc_StructVar>(nullptr, imExp.decl, imExp.typeArgs)));
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            auto nDerefThisLoc = MakePtr<NLoc_LocalDeref>(context.MakeThisLoc());
            return Value(MakePtr<IrExp_LocalRef>(MakePtr<NLoc_StructVar>(nDerefThisLoc, imExp.decl, imExp.typeArgs)));
        }
    }

    // &x (E.First.x)    
    void Visit(ImExp_EnumElemVar& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_ListIndexer& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_LocalDeref& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_BoxDeref& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_Else& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }
};

} // namespace 

expected<IrExpPtr, DiagPtr> TranslateImExpToIrExp(const ImExpPtr& imExp, TranslationContext& context, RTypeFactory& factory)
{
    expected<IrExpPtr, DiagPtr> result;
    ImExpToIrExpTranslator translator(&result, context, factory);
    imExp->Accept(translator);

    return result;
}

} // namespace Citron::SyntaxIR0Translation

