#include "MSharedExp.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "MLoc.h"

namespace Citron {

void MSharedExp_Static::Accept(MSharedExpVisitor& visitor) { visitor.Visit(this); }
void MSharedExp_ClassVar::Accept(MSharedExpVisitor& visitor) { visitor.Visit(this); }
void MSharedExp_SharedStructVar::Accept(MSharedExpVisitor& visitor) { visitor.Visit(this); }
void MSharedExp_StructVar::Accept(MSharedExpVisitor& visitor) { visitor.Visit(this); }

RType* GetType(MSharedExp* sharedExp, RFactory* rFactory)
{
    struct Visitor
    {
        using ResultType = RType*;
        RFactory* rFactory;

        ResultType Visit(MSharedExp_Static* sharedExp) { return rFactory->MakeSharedType(GetType(sharedExp, rFactory)); }

        ResultType Visit(MSharedExp_ClassVar* sharedExp) 
        { 
            auto* declType = sharedExp->decl->GetDeclType(*sharedExp->typeArgs);
            return rFactory->MakeSharedType(declType);
        }

        ResultType Visit(MSharedExp_SharedStructVar* sharedExp) 
        { 
            auto* declType = sharedExp->decl->GetDeclType(*sharedExp->typeArgs);
            return rFactory->MakeSharedType(declType);
        }

        ResultType Visit(MSharedExp_StructVar* sharedExp) 
        { 
            auto* declType = sharedExp->decl->GetDeclType(*sharedExp->typeArgs);
            return rFactory->MakeSharedType(declType);
        }
    };

    return Accept(Visitor{rFactory}, sharedExp);
}

} // namespace Citron