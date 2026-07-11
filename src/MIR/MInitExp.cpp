#include "MInitExp.h"

#include "Infra/Unreachable.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RLambdaDecl.h"

#include "MSharedExp.h"

using namespace std;

namespace Citron {

void MInitExp_Shared::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_SharedRef::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_Stmt::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_String::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_List::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_CallIntrinsic::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_NewClass::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_StructCtor::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_Call::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_NewEnumElem::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_Nullable::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_NullableNullLiteral::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_NullableInplaceNullLiteral::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_Cast::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_Lambda::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_InlineBlock::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }
void MInitExp_As::Accept(MInitExpVisitor& visitor) { visitor.Visit(this); }

RType* GetType(MInitExp_StructCtorKind& ctorKind, RFactory* rFactory)
{
    return visit([rFactory](auto& ctorKind) -> RType* {
        using T = remove_cvref_t<decltype(ctorKind)>;

        if constexpr (same_as<T, MInitExp_StructCtorKind_Copy>)
            return ctorKind.structType;
        else if constexpr (same_as<T, MInitExp_StructCtorKind_Move>)
            return ctorKind.structType;
        else if constexpr (same_as<T, MInitExp_StructCtorKind_General>)
            return rFactory->MakeStructType(ctorKind.decl->GetStructDecl(), ctorKind.typeArgs);
        else static_assert(false);
        
    }, ctorKind);
}

RType* GetType(MInitExp* initExp, RFactory* rFactory)
{
    struct Visitor {
        using ResultType = RType*;
        RFactory* rFactory;

        ResultType Visit(MInitExp_Shared* initExp) { return GetType(initExp->create, rFactory); }
        ResultType Visit(MInitExp_SharedRef* initExp) { return GetType(initExp->sharedExp, rFactory); }
        ResultType Visit(MInitExp_Stmt* initExp) { return GetType(initExp->finalExp, rFactory); }
        ResultType Visit(MInitExp_String* initExp) { return rFactory->MakeStringType(); }
        ResultType Visit(MInitExp_List* initExp) { return rFactory->MakeListType(initExp->itemType); }
        ResultType Visit(MInitExp_CallIntrinsic* initExp) 
        {
            using enum MInitExp_CallIntrinsicKind;

            switch (initExp->kind)
            {
            case ToString_String_Bool: return rFactory->MakeStringType();
            case ToString_String_Int: return rFactory->MakeStringType();
            case Add_String_StringInRef_StringInRef: return rFactory->MakeStringType();
            case Max: unreachable();
            }

            unreachable();
        }

        ResultType Visit(MInitExp_NewClass* initExp) { return rFactory->MakeClassType(initExp->ctorDecl->GetClassDecl(), initExp->typeArgs); }
        ResultType Visit(MInitExp_StructCtor* initExp) { return GetType(initExp->kind, rFactory); }
        ResultType Visit(MInitExp_Call* initExp) { return initExp->callable.decl->GetReturnType(initExp->callable.typeArgs); }
        ResultType Visit(MInitExp_NewEnumElem* initExp) { return rFactory->MakeEnumElemType(initExp->enumElemDecl, initExp->typeArgs); }
        ResultType Visit(MInitExp_Nullable* initExp) 
        {
            auto* innerType = GetType(initExp->inner.initExp, rFactory);
            return rFactory->MakeNullableType(innerType);
        }
        ResultType Visit(MInitExp_NullableNullLiteral* initExp) { return rFactory->MakeNullableType(initExp->innerType); }
        ResultType Visit(MInitExp_NullableInplaceNullLiteral* initExp) { return rFactory->MakeNullableInplaceType(initExp->innerType); }
        ResultType Visit(MInitExp_Cast* initExp) { return initExp->targetType; }
        ResultType Visit(MInitExp_Lambda* initExp) { return initExp->lambdaDecl->GetReturnType(initExp->typeArgs); }
        ResultType Visit(MInitExp_InlineBlock* initExp) { return initExp->returnType; }
        ResultType Visit(MInitExp_As* initExp) { return initExp->type; }
    };

    return Accept(Visitor{rFactory}, initExp);
}

} // namespace Citron
