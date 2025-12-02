#include "ImExpToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MLoc.h"
#include "ImExp.h"
#include "IrExp.h"

#include "TranslationContext.h"

using namespace std;

namespace Citron {

namespace {

struct ImExpToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContext& context;

    ImExpToIrExpTranslator(TranslationContext& context)
        : context(context)
    {
    }

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

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

public:
    ResultType Visit(ImExp_Namespace* imExp)
    {
        return Value<IrExp_Namespace>(imExp->_namespace);
    }

    ResultType Visit(ImExp_GlobalFuncs* imExp)
    {
        // Intermediate Exp -> Intermediate Ref Exp
        return Error<Error_NotImplemented>();
    }

    ResultType Visit(ImExp_TypeVar* imExp)
    {
        return Value<IrExp_TypeVar>(imExp->type);
    }

    ResultType Visit(ImExp_Class* imExp)
    {
        return Value<IrExp_Class>(imExp->classDecl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_ClassFuncs* imExp)
    {
        return Error<Error_NotImplemented>();
    }

    ResultType Visit(ImExp_Struct* imExp)
    {
        return Value<IrExp_Struct>(imExp->structDecl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_StructFuncs* imExp)
    {
        return Error<Error_NotImplemented>();
    }

    ResultType Visit(ImExp_Enum* imExp)
    {
        return Value<IrExp_Enum>(imExp->decl, imExp->typeArgs);
    }

    ResultType Visit(ImExp_EnumElem* imExp)
    {
        return Error<Error_NotImplemented>();
    }

    // &this   -> invalid
    // &this.a -> valid, box ptr
    ResultType Visit(ImExp_ThisVar* imExp)
    {
        return Value<IrExp_ThisVar>(imExp->type);
    }

    // &id
    ResultType Visit(ImExp_LocalVar* imExp)
    {
        return Value<IrExp_LocalRef>(context.MakeNLoc<MLoc_LocalVar>(imExp->name, imExp->type));
    }

    // &x
    ResultType Visit(ImExp_LambdaVar* imExp)
    {
        // TODO: [10] box lambda이면 box로 판단해야 한다
        return Value<IrExp_LocalRef>(context.MakeNLoc<MLoc_LambdaVar>(imExp->decl, imExp->typeArgs));
    }

    // x (C.x, this.x)
    ResultType Visit(ImExp_ClassVar* imExp)
    {
        if (imExp->decl->IsStatic()) // &C.x
        {
            return Value<IrExp_StaticRef>(context.MakeNLoc<MLoc_ClassVar>(nullptr, imExp->decl, imExp->typeArgs));
        }
        else // &this.x
        {
            // auto classType = imExp.decl->GetClassType(imExp.typeArgs, factory);
            return Value<IrExp_BoxRef_ClassMember>(context.MakeThisLoc(), imExp->decl, imExp->typeArgs);
        }
    }

    // x (S.x, this->x)
    ResultType Visit(ImExp_StructVar* imExp)
    {
        if (imExp->decl->IsStatic())
        {
            return Value<IrExp_StaticRef>(context.MakeNLoc<MLoc_StructVar>(nullptr, imExp->decl, imExp->typeArgs));
        }
        else
        {
            // this의 타입이 S*이다.
            // TODO: [10] box함수이면 this를 box로 판단해야 한다
            auto* nDerefThisLoc = context.MakeNLoc<MLoc_LocalDeref>(context.MakeThisLoc());
            return Value<IrExp_LocalRef>(context.MakeNLoc<MLoc_StructVar>(nDerefThisLoc, imExp->decl, imExp->typeArgs));
        }
    }

    // &x (E.First.x)    
    ResultType Visit(ImExp_EnumElemVar* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_ListIndexer* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_LocalDeref* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_BoxDeref* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }

    ResultType Visit(ImExp_Else* imExp)
    {
        // 유일한 경로가 syntax id -> intermediateExp -> intermediateRefExp이기 때문에 불가능하다
        throw RuntimeFatalException{};
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateImExpToIrExp(ImExp* imExp, TranslationContext& context)
{
    ImExpToIrExpTranslator translator{context};
    return Accept(translator, imExp);
}

} // namespace Citron::SyntaxIR0Translation

