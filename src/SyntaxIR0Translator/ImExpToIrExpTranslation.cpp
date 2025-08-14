#include "ImExpToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "IR0/RNamespaceDecl.h"
#include "IR0/RTypes.h"
#include "IR0/RClassVarDecl.h"
#include "IR0/RStructVarDecl.h"
#include "IR0/NLambdaVarDecl.h"
#include "IR0/NLoc.h"
#include "ImExp.h"
#include "IrExp.h"

#include "TranslationContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

struct ImExpToIrExpTranslator : public ImExpVisitor
{
    expected<IrExp*, DiagPtr>* result;
    TranslationContext& context;

    ImExpToIrExpTranslator(expected<IrExp*, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    void Value(TArgs&&... args)
    {
        *result = context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    void Visit(ImExp_Namespace* imExp) override
    {
        return Value<IrExp_Namespace>(imExp->_namespace);
    }

    void Visit(ImExp_GlobalFuncs* imExp) override
    {
        // Intermediate Exp -> Intermediate Ref Exp
        return Error<Error_NotImplemented>();
    }

    void Visit(ImExp_TypeVar* imExp) override
    {
        return Value<IrExp_TypeVar>(imExp->type);
    }

    void Visit(ImExp_Class* imExp) override
    {
        return Value<IrExp_Class>(imExp->classDecl, imExp->typeArgs);
    }

    void Visit(ImExp_ClassFuncs* imExp) override
    {
        return Error<Error_NotImplemented>();
    }

    void Visit(ImExp_Struct* imExp) override
    {
        return Value<IrExp_Struct>(imExp->structDecl, imExp->typeArgs);
    }

    void Visit(ImExp_StructFuncs* imExp) override
    {
        return Error<Error_NotImplemented>();
    }

    void Visit(ImExp_Enum* imExp) override
    {
        return Value<IrExp_Enum>(imExp->decl, imExp->typeArgs);
    }

    void Visit(ImExp_EnumElem* imExp) override
    {
        return Error<Error_NotImplemented>();
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    void Visit(ImExp_ThisVar* imExp) override
    {
        return Value<IrExp_ThisVar>(imExp->type);
    }

    // &id
    void Visit(ImExp_LocalVar* imExp) override
    {
        return Value<IrExp_LocalRef>(context.MakeNLoc<NLoc_LocalVar>(RName_Normal(imExp->name), imExp->type));
    }

    // &x
    void Visit(ImExp_LambdaVar* imExp) override
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다
        return Value<IrExp_LocalRef>(context.MakeNLoc<NLoc_LambdaVar>(imExp->decl, imExp->typeArgs));
    }

    // x (C.x, this.x)
    void Visit(ImExp_ClassVar* imExp) override
    {
        if (imExp->decl->IsStatic()) // &C.x
        {
            return Value<IrExp_StaticRef>(context.MakeNLoc<NLoc_ClassVar>(nullptr, imExp->decl, imExp->typeArgs));
        }
        else // &this.x
        {
            // auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            return Value<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(), imExp->decl, imExp->typeArgs);
        }
    }

    // x (S.x, this->x)
    void Visit(ImExp_StructVar* imExp) override
    {
        if (imExp->decl->IsStatic())
        {
            return Value<IrExp_StaticRef>(context.MakeNLoc<NLoc_StructVar>(nullptr, imExp->decl, imExp->typeArgs));
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            auto nDerefThisLoc = context.MakeNLoc<NLoc_LocalDeref>(context.MakeThisLoc());
            return Value<IrExp_LocalRef>(context.MakeNLoc<NLoc_StructVar>(nDerefThisLoc, imExp->decl, imExp->typeArgs));
        }
    }

    // &x (E.First.x)    
    void Visit(ImExp_EnumElemVar* imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_ListIndexer* imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_LocalDeref* imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_BoxDeref* imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }

    void Visit(ImExp_Else* imExp) override
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException();
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContext& context)
{
    expected<IrExp*, DiagPtr> result;
    ImExpToIrExpTranslator translator(&result, context);
    imExp->Accept(translator);

    return result;
}

} // namespace Citron::SyntaxIR0Translation

