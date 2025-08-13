#include "NExp.h"

#include <cassert>

#include "Infra/Unreachable.h"

#include "IR0Factory.h"
#include "RClassCtorDecl.h"
#include "RClassFuncDecl.h"
#include "RClassVarDecl.h"
#include "RStructCtorDecl.h"
#include "RStructFuncDecl.h"
#include "RStructVarDecl.h"
#include "RGlobalFuncDecl.h"

#include "NLoc.h"
#include "NLambdaDecl.h"

using namespace std;

namespace Citron {

NExp_Load::NExp_Load(NLoc* loc)
    : loc(loc)
{
}

RType* NExp_Load::GetType(IR0Factory& factory)
{
    return loc->GetType(factory);
}

NExp_Assign::NExp_Assign(NLoc* dest, NExp* src)
    : dest{dest}, src{src}
{
}

RType* NExp_Assign::GetType(IR0Factory& factory)
{
    return dest->GetType(factory);
}

NExp_Box::NExp_Box(NExp* innerExp)
    : innerExp{innerExp}
{
}

RType* NExp_Box::GetType(IR0Factory& factory)
{
    auto* innerType = innerExp->GetType(factory);
    return factory.MakeBoxPtrType(innerType);
}

NExp_StaticBoxRef::NExp_StaticBoxRef(NLoc* loc)
    : loc(loc)
{
}

RType* NExp_StaticBoxRef::GetType(IR0Factory& factory)
{
    return factory.MakeBoxPtrType(loc->GetType(factory));
}

NExp_ClassMemberBoxRef::NExp_ClassMemberBoxRef(NLoc* holder, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : holder(holder), decl(decl), typeArgs(typeArgs)
{
}

RType* NExp_ClassMemberBoxRef::GetType(IR0Factory& factory)
{
    auto* declType = decl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(declType);
}

NExp_StructIndirectMemberBoxRef::NExp_StructIndirectMemberBoxRef(NLoc* holder, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : holder(holder), decl(decl), typeArgs(typeArgs)
{
}

RType* NExp_StructIndirectMemberBoxRef::GetType(IR0Factory& factory)
{
    auto* declType = decl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(declType);
}

NExp_StructMemberBoxRef::NExp_StructMemberBoxRef(NLoc* parent, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RType* NExp_StructMemberBoxRef::GetType(IR0Factory& factory)
{
    auto* declType = decl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxPtrType(declType);
}

NExp_LocalRef::NExp_LocalRef(NLoc* innerLoc)
    : innerLoc(innerLoc)
{
}

RType* NExp_LocalRef::GetType(IR0Factory& factory)
{
    auto* innerLocType = innerLoc->GetType(factory);
    return factory.MakeLocalPtrType(innerLocType);
}

NExp_CastBoxedLambdaToFunc::NExp_CastBoxedLambdaToFunc(NExp* exp, RType_Func* funcType)
    : exp(exp), funcType(funcType)
{
}

RType* NExp_CastBoxedLambdaToFunc::GetType(IR0Factory& factory)
{
    return funcType;
}

NExp_BoolLiteral::NExp_BoolLiteral(bool value)
    : value(value)
{
}

RType* NExp_BoolLiteral::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_IntLiteral::NExp_IntLiteral(int value)
    : value(value)
{
}

RType* NExp_IntLiteral::GetType(IR0Factory& factory)
{
    return factory.MakeIntType();
}

NTextStringExpElement::NTextStringExpElement(const string& text)
    : text(text)
{
}

NLocStringExpElement::NLocStringExpElement(NLoc* loc)
    : loc(loc)
{

}

NExp_String::NExp_String(vector<NStringExpElement>&& elements)
    : elements(move(elements))
{
}

RType* NExp_String::GetType(IR0Factory& factory)
{
    return factory.MakeStringType();
}

NExp_List::NExp_List(vector<NExp*>&& elems, RType* itemType)
    : elems(move(elems)), itemType(itemType)
{
}

RType* NExp_List::GetType(IR0Factory& factory)
{
    return factory.MakeListType(itemType);
}

NExp_ListIterator::NExp_ListIterator(NLoc* listLoc, RType* iteratorType)
    : listLoc(listLoc), type(iteratorType)
{
}

RType* NExp_ListIterator::GetType(IR0Factory& factory)
{
    return type;
}

NExp_CallInternalUnaryOperator::NExp_CallInternalUnaryOperator(NInternalUnaryOperator op, NExp* operand)
    : op{op}, operand{operand}
{
}

RType* NExp_CallInternalUnaryOperator::GetType(IR0Factory& factory)
{
    switch (op)
    {
    case NInternalUnaryOperator::LogicalNot_Bool_Bool: return factory.MakeBoolType();
    case NInternalUnaryOperator::UnaryMinus_Int_Int: return factory.MakeIntType();
    case NInternalUnaryOperator::ToString_Bool_String: return factory.MakeStringType();
    case NInternalUnaryOperator::ToString_Int_String: return factory.MakeStringType();
    }

    unreachable();
}

NExp_CallInternalUnaryAssignOperator::NExp_CallInternalUnaryAssignOperator(NInternalUnaryAssignOperator op, NLoc* operand)
    : op{op}, operand{operand}
{
}

RType* NExp_CallInternalUnaryAssignOperator::GetType(IR0Factory& factory)
{
    switch(op)
    {
    case NInternalUnaryAssignOperator::PrefixInc_Int_Int:
    case NInternalUnaryAssignOperator::PrefixDec_Int_Int:
    case NInternalUnaryAssignOperator::PostfixInc_Int_Int:
    case NInternalUnaryAssignOperator::PostfixDec_Int_Int:
        return factory.MakeIntType();
    }

    unreachable();
}

NExp_CallInternalBinaryOperator::NExp_CallInternalBinaryOperator(NInternalBinaryOperator op, NExp* operand0, NExp* operand1)
    : op(op), operand0(operand0), operand1(operand1)
{
}

RType* NExp_CallInternalBinaryOperator::GetType(IR0Factory& factory)
{
    switch(op)
    {
        case NInternalBinaryOperator::Multiply_Int_Int_Int:
        case NInternalBinaryOperator::Divide_Int_Int_Int:
        case NInternalBinaryOperator::Modulo_Int_Int_Int:
        case NInternalBinaryOperator::Add_Int_Int_Int:
            return factory.MakeIntType();

        case NInternalBinaryOperator::Add_String_String_String:
            return factory.MakeStringType();

        case NInternalBinaryOperator::Subtract_Int_Int_Int:
            return factory.MakeIntType();

        case NInternalBinaryOperator::LessThan_Int_Int_Bool:
        case NInternalBinaryOperator::LessThan_String_String_Bool:
        case NInternalBinaryOperator::GreaterThan_Int_Int_Bool:
        case NInternalBinaryOperator::GreaterThan_String_String_Bool:
        case NInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool:
        case NInternalBinaryOperator::LessThanOrEqual_String_String_Bool:
        case NInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool:
        case NInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool:
        case NInternalBinaryOperator::Equal_Int_Int_Bool:
        case NInternalBinaryOperator::Equal_Bool_Bool_Bool:
        case NInternalBinaryOperator::Equal_String_String_Bool:
            return factory.MakeBoolType();
        default:
            unreachable();
    }
}

NExp_CallGlobalFunc::NExp_CallGlobalFunc(RGlobalFuncDecl* funcDecl, RTypeArguments* typeArgs, const vector<NArgument>& args)
    : funcDecl(funcDecl), typeArgs(typeArgs), args(args)
{
}

RType* NExp_CallGlobalFunc::GetType(IR0Factory& factory)
{
    return funcDecl->GetReturnType(*typeArgs, factory);
}

NExp_NewClass::NExp_NewClass(RClassCtorDecl* ctorDecl, RTypeArguments* typeArgs, const vector<NArgument>& args)
    : ctorDecl(ctorDecl), typeArgs(typeArgs), args(args)
{
}

RType* NExp_NewClass::GetType(IR0Factory& factory)
{
    auto classDecl = ctorDecl->GetClassDecl();
    assert(classDecl);

    return factory.MakeClassType(classDecl, typeArgs);
}

/////////////////////////////////////

NExp_CallClassFunc::NExp_CallClassFunc(RClassFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* NExp_CallClassFunc::GetType(IR0Factory& factory)
{
    return decl->GetReturnType(*typeArgs, factory);
}

NExp_CastClass::NExp_CastClass(NExp* src, RType* classType)
    : src(src), classType(classType)
{
}

RType* NExp_CastClass::GetType(IR0Factory& factory)
{
    return classType;
}

NExp_NewStruct::NExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, vector<NArgument>&& args)
    : ctor{ctor}, typeArgs{typeArgs}, args{move(args)}
{
}

RType* NExp_NewStruct::GetType(IR0Factory& factory)
{
    auto structDecl = ctor->GetStructDecl();
    return factory.MakeStructType(structDecl, typeArgs);
}

NExp_CallStructFunc::NExp_CallStructFunc(RStructFuncDecl* decl, RTypeArguments* typeArgs, NLoc* instance, vector<NArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* NExp_CallStructFunc::GetType(IR0Factory& factory)
{
    return decl->GetReturnType(*typeArgs, factory);
}

NExp_NewEnumElem::NExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, vector<NArgument>&& args)
    : enumElemDecl{enumElemDecl}, typeArgs{typeArgs}, args{move(args)}
{
}

RType* NExp_NewEnumElem::GetType(IR0Factory& factory)
{
    return factory.MakeEnumElemType(enumElemDecl, typeArgs);
}

NExp_CastEnumElemToEnum::NExp_CastEnumElemToEnum(NExp* src, RType* enumType)
    : src(src), enumType(enumType)
{
}

RType* NExp_CastEnumElemToEnum::GetType(IR0Factory& factory)
{
    return enumType;
}

NExp_NullableValueNullLiteral::NExp_NullableValueNullLiteral(RType* innerType)
    : innerType(innerType)
{
}

RType* NExp_NullableValueNullLiteral::GetType(IR0Factory& factory)
{
    return factory.MakeNullableValueType(innerType);
}

NExp_NullableRefNullLiteral::NExp_NullableRefNullLiteral(RType* innerType)
    : innerType(innerType)
{
}

RType* NExp_NullableRefNullLiteral::GetType(IR0Factory& factory)
{
    return factory.MakeNullableRefType(innerType);
}

NExp_NewNullable::NExp_NewNullable(NExp* innerExp)
    : innerExp(innerExp)
{
}

RType* NExp_NewNullable::GetType(IR0Factory& factory)
{
    return factory.MakeNullableValueType(innerExp->GetType(factory));
}

NExp_Lambda::NExp_Lambda(NLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, const vector<NArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), args(args)
{
}

RType* NExp_Lambda::GetType(IR0Factory& factory)
{
    return factory.MakeLambdaType(lambdaDecl, typeArgs);
}

NExp_CallLambda::NExp_CallLambda(RLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, NLoc* callable, const vector<NArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RType* NExp_CallLambda::GetType(IR0Factory& factory)
{
    return lambdaDecl->GetReturnType(*typeArgs, factory);
}

NExp_InlineBlock::NExp_InlineBlock(const vector<NStmt*>& stmts, RType* returnType)
    : stmts(stmts), returnType(returnType)
{
}

RType* NExp_InlineBlock::GetType(IR0Factory& factory)
{
    return returnType;
}

NExp_ClassIsClass::NExp_ClassIsClass(NExp* exp, RType* classType)
    : exp{exp}, classType{classType}
{
}

RType* NExp_ClassIsClass::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_ClassAsClass::NExp_ClassAsClass(NExp* exp, RType* classType)
    : exp(exp), classType(classType)
{
}

RType* NExp_ClassAsClass::GetType(IR0Factory& factory)
{    
    return factory.MakeNullableRefType(classType);
}

NExp_ClassIsInterface::NExp_ClassIsInterface(NExp* exp, RType* interfaceType)
    : exp{exp}, interfaceType{interfaceType}
{
}

RType* NExp_ClassIsInterface::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_ClassAsInterface::NExp_ClassAsInterface(NExp* exp, RType* interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RType* NExp_ClassAsInterface::GetType(IR0Factory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

NExp_InterfaceIsClass::NExp_InterfaceIsClass(NExp* exp, RType* classType)
    : exp{exp}, classType{classType}
{
}

RType* NExp_InterfaceIsClass::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_InterfaceAsClass::NExp_InterfaceAsClass(NExp* exp, RType* classType)
    : exp(exp), classType(classType)
{
}

RType* NExp_InterfaceAsClass::GetType(IR0Factory& factory)
{
    return factory.MakeNullableRefType(classType);
}

NExp_InterfaceIsInterface::NExp_InterfaceIsInterface(NExp* exp, RType* interfaceType)
    : exp{exp}, interfaceType{interfaceType}
{
}

RType* NExp_InterfaceIsInterface::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_InterfaceAsInterface::NExp_InterfaceAsInterface(NExp* exp, RType* interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RType* NExp_InterfaceAsInterface::GetType(IR0Factory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

NExp_EnumIsEnumElem::NExp_EnumIsEnumElem(NExp* exp, RType* enumElemType)
    : exp{exp}, enumElemType{enumElemType}
{
}

RType* NExp_EnumIsEnumElem::GetType(IR0Factory& factory)
{
    return factory.MakeBoolType();
}

NExp_EnumAsEnumElem::NExp_EnumAsEnumElem(NExp* exp, RType* enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RType* NExp_EnumAsEnumElem::GetType(IR0Factory& factory)
{
    return factory.MakeNullableValueType(enumElemType);
}

} // namespace Citron