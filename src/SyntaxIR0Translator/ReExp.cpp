#include "pch.h"
#include "ReExp.h"
#include <IR0/RLambdaMemberVarDecl.h>

namespace Citron::SyntaxIR0Translator {

Citron::RTypePtr ReLambdaMemberVarExp::GetType()
{
    return decl->GetDeclType(typeArgs);
}

//RTypePtr ReClassMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}
//
//RTypePtr ReStructMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}
//
//RTypePtr ReEnumElemMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}
//
//RTypePtr ReLocalDerefExp::GetType()
//{
//    return ((RLocalPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}
//
//RTypePtr ReBoxDerefExp::GetType()
//{
//    return ((RBoxPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}
//
//RTypePtr ReElseExp::GetType()
//{
//    return rExp->GetType();
//}



}