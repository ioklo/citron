#include "MLoc.h"
#include <cassert>

#include "Infra/Exceptions.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"

#include "MExp.h"

namespace Citron {

void MLoc_Materialize::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LocalVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LocalRef::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LambdaVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_ListIndexer::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_StructVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_ClassVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_EnumElemVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_This::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_PtrDeref::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_SharedDeref::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_NullableValue::Accept(MLocVisitor& visitor) { visitor.Visit(this); }

RType* GetType(MLoc* loc, RFactory* rFactory)
{
    struct Visitor
    {
        using ResultType = RType*;
        RFactory* rFactory;

        ResultType Visit(MLoc_Materialize* loc) { return GetType(loc->create, rFactory); }
        ResultType Visit(MLoc_LocalVar* loc) { return loc->declType; }
        ResultType Visit(MLoc_LocalRef* loc) { return loc->declType; }
        ResultType Visit(MLoc_LambdaVar* loc) { return loc->decl->GetDeclType(loc->typeArgs); }
        ResultType Visit(MLoc_ListIndexer* loc) { return loc->itemType; }
        ResultType Visit(MLoc_StructVar* loc) { return loc->decl->GetDeclType(loc->typeArgs); }
        ResultType Visit(MLoc_ClassVar* loc) { return loc->decl->GetDeclType(loc->typeArgs); }
        ResultType Visit(MLoc_EnumElemVar* loc) { return loc->decl->GetDeclType(loc->typeArgs); }
        ResultType Visit(MLoc_This* loc) { return loc->type; }
        ResultType Visit(MLoc_PtrDeref* loc) 
        {  
            auto* ptrType = dynamic_cast<RType_Ptr*>(GetType(loc->srcPtr, rFactory));
            assert(ptrType);

            return ptrType->innerType;
        }

        ResultType Visit(MLoc_SharedDeref* loc) 
        { 
            auto* sharedType = dynamic_cast<RType_Shared*>(GetType(loc->srcShared.loc, rFactory));
            assert(sharedType);

            return sharedType->innerType;
        }

        ResultType Visit(MLoc_NullableValue* loc) 
        { 
            auto* innerType = dynamic_cast<RType_Nullable*>(GetType(loc->loc, rFactory));
            return innerType->innerType;
        }
    };

    return Accept(Visitor{rFactory}, loc);
}

} // namespace Citron

