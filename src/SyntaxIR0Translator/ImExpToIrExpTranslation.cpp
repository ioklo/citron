#include "pch.h"
#include "ImExpToIrExpTranslation.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

#include "ImExp.h"
#include "IrExp.h"

namespace Citron::SyntaxIR0Translator {

struct ImExpToIrExpTranslator : public ImExpVisitor
{
    IrExpPtr* result;
    RTypeFactory& factory;

    ImExpToIrExpTranslator(IrExpPtr* result, RTypeFactory& factory)
        : result(result), factory(factory)
    {
    }

    void Visit(ImNamespaceExp& imExp) override 
    { 
        *result = MakePtr<IrNamespaceExp>(imExp._namespace);
    }

    void Visit(ImGlobalFuncsExp& imExp) override 
    { 
        // static_assert(false); 
        *result = nullptr;
    }

    void Visit(ImTypeVarExp& imExp) override 
    { 
        *result = MakePtr<IrTypeVarExp>(imExp.type);
    }

    void Visit(ImClassExp& imExp) override
    {
        *result = MakePtr<IrClassExp>(imExp.classDecl, imExp.typeArgs);
    }

    void Visit(ImClassMemberFuncsExp& imExp) override 
    { 
        // static_assert(false);
        *result = nullptr;
    }

    void Visit(ImStructExp& imExp) override
    {
        *result = MakePtr<IrStructExp>(imExp.structDecl, imExp.typeArgs);
    }

    void Visit(ImStructMemberFuncsExp& imExp) override
    {
        // static_assert(false);
        *result = nullptr;
    }

    void Visit(ImEnumExp& imExp) override
    {
        *result = MakePtr<IrEnumExp>(imExp.decl, imExp.typeArgs);
    }

    void Visit(ImEnumElemExp& imExp) override 
    { 
        // static_assert(false); 
        *result = nullptr;
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    void Visit(ImThisVarExp& imExp) override 
    { 
        *result = MakePtr<IrThisVarExp>(imExp.type);
    }

    // &id
    void Visit(ImLocalVarExp& imExp) override 
    { 
        *result = MakePtr<IrLocalRefExp>(MakePtr<RLocalVarLoc>(imExp.name, imExp.type));
    }

    // &x
    void Visit(ImLambdaMemberVarExp& imExp) override 
    { 
        // TODO: [10] box lambda이면 box로 판단해야 한다
        *result = MakePtr<IrLocalRefExp>(MakePtr<RLambdaMemberVarLoc>(imExp.decl, imExp.typeArgs));
    }

    // x (C.x, this.x)
    void Visit(ImClassMemberVarExp& imExp) override 
    { 
        if (imExp.decl->bStatic) // &C.x
        {
            *result = MakePtr<IrStaticRefExp>(MakePtr<RClassMemberLoc>(nullptr, imExp.decl, imExp.typeArgs));
        }
        else // &this.x
        {
            auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            *result = MakePtr<IrClassMemberBoxRefExp>(MakePtr<RThisLoc>(classType), imExp.decl, imExp.typeArgs);
        }
    }

    // x (S.x, this->x)
    void Visit(ImStructMemberVarExp& imExp) override 
    {
        if (imExp.decl->bStatic)
        {
            *result = MakePtr<IrStaticRefExp>(MakePtr<RStructMemberLoc>(nullptr, imExp.decl, imExp.typeArgs));
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            auto structType = imExp.decl->GetStructType(imExp.typeArgs, factory);
            auto rDerefThisLoc = MakePtr<RLocalDerefLoc>(MakePtr<RThisLoc>(structType));
            *result = MakePtr<IrLocalRefExp>(MakePtr<RStructMemberLoc>(rDerefThisLoc, imExp.decl, imExp.typeArgs));
        }
    }

    // &x (E.First.x)    
    void Visit(ImEnumElemMemberVarExp& imExp) override 
    { 
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImListIndexerExp& imExp) override 
    { 
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImLocalDerefExp& imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImBoxDerefExp& imExp) override 
    { 
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImElseExp& imExp) override 
    { 
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }
};

IrExpPtr TranslateImExpToIrExp(const ImExpPtr& imExp, RTypeFactory& factory)
{
    IrExpPtr result;
    ImExpToIrExpTranslator translator(&result, factory);
    imExp->Accept(translator);

    return result;
}

} // namespace Citron::SyntaxIR0Translation

