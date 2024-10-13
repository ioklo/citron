#include "pch.h"
#include "ImExpToIrExpTranslation.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

#include "ImExp.h"
#include "IrExp.h"

#include "ScopeContext.h"

namespace Citron::SyntaxIR0Translator {

namespace {

struct ImExpToIrExpTranslator : public ImExpVisitor
{
    IrExpPtr* result;
    ScopeContext& context;
    RTypeFactory& factory;

    ImExpToIrExpTranslator(IrExpPtr* result, ScopeContext& context, RTypeFactory& factory)
        : result(result), context(context), factory(factory)
    {
    }

    void Visit(ImExp_Namespace& imExp) override
    {
        *result = MakePtr<IrExp_Namespace>(imExp._namespace);
    }

    void Visit(ImExp_GlobalFuncs& imExp) override
    {
        static_assert(false);
        *result = nullptr;
    }

    void Visit(ImExp_TypeVar& imExp) override
    {
        *result = MakePtr<IrExp_TypeVar>(imExp.type);
    }

    void Visit(ImExp_Class& imExp) override
    {
        *result = MakePtr<IrExp_Class>(imExp.classDecl, imExp.typeArgs);
    }

    void Visit(ImExp_ClassMemberFuncs& imExp) override
    {
        static_assert(false);
        *result = nullptr;
    }

    void Visit(ImExp_Struct& imExp) override
    {
        *result = MakePtr<IrExp_Struct>(imExp.structDecl, imExp.typeArgs);
    }

    void Visit(ImExp_StructMemberFuncs& imExp) override
    {
        static_assert(false);
        *result = nullptr;
    }

    void Visit(ImExp_Enum& imExp) override
    {
        *result = MakePtr<IrExp_Enum>(imExp.decl, imExp.typeArgs);
    }

    void Visit(ImExp_EnumElem& imExp) override
    {
        static_assert(false);
        *result = nullptr;
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    void Visit(ImExp_ThisVar& imExp) override
    {
        *result = MakePtr<IrExp_ThisVar>();
    }

    // &id
    void Visit(ImExp_LocalVar& imExp) override
    {
        *result = MakePtr<IrExp_LocalRef>(MakePtr<RLoc_LocalVar>(imExp.name, imExp.type));
    }

    // &x
    void Visit(ImExp_LambdaMemberVar& imExp) override
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다
        *result = MakePtr<IrExp_LocalRef>(MakePtr<RLoc_LambdaMemberVar>(imExp.decl, imExp.typeArgs));
    }

    // x (C.x, this.x)
    void Visit(ImExp_ClassMemberVar& imExp) override
    {
        if (imExp.decl->bStatic) // &C.x
        {
            *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_ClassMember>(nullptr, imExp.decl, imExp.typeArgs));
        }
        else // &this.x
        {
            // auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            *result = MakePtr<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(factory), imExp.decl, imExp.typeArgs);
        }
    }

    // x (S.x, this->x)
    void Visit(ImExp_StructMemberVar& imExp) override
    {
        if (imExp.decl->bStatic)
        {
            *result = MakePtr<IrExp_StaticRef>(MakePtr<RLoc_StructMember>(nullptr, imExp.decl, imExp.typeArgs));
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            auto rDerefThisLoc = MakePtr<RLoc_LocalDeref>(context.MakeThisLoc(factory));
            *result = MakePtr<IrExp_LocalRef>(MakePtr<RLoc_StructMember>(rDerefThisLoc, imExp.decl, imExp.typeArgs));
        }
    }

    // &x (E.First.x)    
    void Visit(ImExp_EnumElemMemberVar& imExp) override
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

IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, ScopeContext& context, RTypeFactory& factory)
{
    IrExpPtr result;
    ImExpToIrExpTranslator translator(&result, context, factory);
    imExp->Accept(translator);

    return result;
}

} // namespace Citron::SyntaxIR0Translation

