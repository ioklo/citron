#include "NExp.h"

#include <Infra/Unreachable.h>

#include "NLoc.h"
#include "RTypeFactory.h"

#include "RGlobalFuncDecl.h"
#include "RLambdaDecl.h"

#include "REnumElemDecl.h"

#include "RStructDecl.h"
#include "RStructMemberVarDecl.h"
#include "RStructMemberFuncDecl.h"
#include "RStructConstructorDecl.h"

#include "RClassDecl.h"
#include "RClassConstructorDecl.h"
#include "RClassMemberFuncDecl.h"
#include "RClassMemberVarDecl.h"

using namespace std;

namespace Citron {

NExp_Load::NExp_Load(NLocPtr&& loc)
    : loc(std::move(loc))
{
}

RTypePtr NExp_Load::GetType(RTypeFactory& factory)
{
    return loc->GetType(factory);
}

NExp_Assign::NExp_Assign(NLocPtr&& dest, NExpPtr&& src)
    : dest(std::move(dest)), src(std::move(src))
{
}

RTypePtr NExp_Assign::GetType(RTypeFactory& factory)
{
    return dest->GetType(factory);
}

NExp_Box::NExp_Box(NExpPtr&& innerExp)
    : innerExp(std::move(innerExp))
{
}

RTypePtr NExp_Box::GetType(RTypeFactory& factory)
{
    auto innerType = innerExp->GetType(factory);
    return factory.MakeBoxPtrType(move(innerType));
}

NExp_StaticBoxRef::NExp_StaticBoxRef(const NLocPtr& loc)
    : loc(loc)
{
}

RTypePtr NExp_StaticBoxRef::GetType(RTypeFactory& factory)
{
    return factory.MakeBoxPtrType(loc->GetType(factory));
}

NExp_ClassMemberBoxRef::NExp_ClassMemberBoxRef(const NLocPtr& holder, const shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NExp_ClassMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

NExp_StructIndirectMemberBoxRef::NExp_StructIndirectMemberBoxRef(const NLocPtr& holder, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NExp_StructIndirectMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

NExp_StructMemberBoxRef::NExp_StructMemberBoxRef(const NLocPtr& parent, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NExp_StructMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxPtrType(move(declType));
}

NExp_LocalRef::NExp_LocalRef(const NLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr NExp_LocalRef::GetType(RTypeFactory& factory)
{
    auto innerLocType = innerLoc->GetType(factory);
    return factory.MakeLocalPtrType(move(innerLocType));
}

NExp_CastBoxedLambdaToFunc::NExp_CastBoxedLambdaToFunc(const NExpPtr& exp, const shared_ptr<RType_Func>& funcType)
    : exp(exp), funcType(funcType)
{
}

RTypePtr NExp_CastBoxedLambdaToFunc::GetType(RTypeFactory& factory)
{
    return funcType;
}

NExp_BoolLiteral::NExp_BoolLiteral(bool value)
    : value(value)
{
}

RTypePtr NExp_BoolLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_IntLiteral::NExp_IntLiteral(int value)
    : value(value)
{
}

RTypePtr NExp_IntLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeIntType();
}

RTextStringExpElement::RTextStringExpElement(const string& text)
    : text(text)
{
}

RLocStringExpElement::RLocStringExpElement(NLocPtr&& loc)
    : loc(loc)
{

}

NExp_String::NExp_String(vector<RStringExpElement>&& elements)
    : elements(std::move(elements))
{
}

RTypePtr NExp_String::GetType(RTypeFactory& factory)
{
    return factory.MakeStringType();
}

NExp_List::NExp_List(vector<NExpPtr>&& elems, const RTypePtr& itemType)
    : elems(move(elems)), itemType(itemType)
{
}

RTypePtr NExp_List::GetType(RTypeFactory& factory)
{
    return factory.MakeListType(itemType);
}

NExp_ListIterator::NExp_ListIterator(const NLocPtr& listLoc, const RTypePtr& iteratorType)
    : listLoc(listLoc), type(type)
{
}


RTypePtr NExp_ListIterator::GetType(RTypeFactory& factory)
{
    return type;
}

NExp_CallInternalUnaryOperator::NExp_CallInternalUnaryOperator(RInternalUnaryOperator op, NExpPtr&& operand)
    : op(op), operand(std::move(operand))
{
}

RTypePtr NExp_CallInternalUnaryOperator::GetType(RTypeFactory& factory)
{
    switch (op)
    {
    case RInternalUnaryOperator::LogicalNot_Bool_Bool: return factory.MakeBoolType();
    case RInternalUnaryOperator::UnaryMinus_Int_Int: return factory.MakeIntType();
    case RInternalUnaryOperator::ToString_Bool_String: return factory.MakeStringType();
    case RInternalUnaryOperator::ToString_Int_String: return factory.MakeStringType();
    }

    unreachable();
}

NExp_CallInternalUnaryAssignOperator::NExp_CallInternalUnaryAssignOperator(RInternalUnaryAssignOperator op, NLocPtr&& operand)
    : op(op), operand(std::move(operand))
{
}

RTypePtr NExp_CallInternalUnaryAssignOperator::GetType(RTypeFactory& factory)
{
    switch(op)
    {
    case RInternalUnaryAssignOperator::PrefixInc_Int_Int:
    case RInternalUnaryAssignOperator::PrefixDec_Int_Int:
    case RInternalUnaryAssignOperator::PostfixInc_Int_Int:
    case RInternalUnaryAssignOperator::PostfixDec_Int_Int:
        return factory.MakeIntType();
    }

    unreachable();
}

NExp_CallInternalBinaryOperator::NExp_CallInternalBinaryOperator(RInternalBinaryOperator op, NExpPtr&& operand0, NExpPtr&& operand1)
    : op(op), operand0(std::move(operand0)), operand1(std::move(operand1))
{
}

RTypePtr NExp_CallInternalBinaryOperator::GetType(RTypeFactory& factory)
{
    switch(op)
    {
        case RInternalBinaryOperator::Multiply_Int_Int_Int:
        case RInternalBinaryOperator::Divide_Int_Int_Int:
        case RInternalBinaryOperator::Modulo_Int_Int_Int:
        case RInternalBinaryOperator::Add_Int_Int_Int:
            return factory.MakeIntType();

        case RInternalBinaryOperator::Add_String_String_String:
            return factory.MakeStringType();

        case RInternalBinaryOperator::Subtract_Int_Int_Int:
            return factory.MakeIntType();

        case RInternalBinaryOperator::LessThan_Int_Int_Bool:
        case RInternalBinaryOperator::LessThan_String_String_Bool:
        case RInternalBinaryOperator::GreaterThan_Int_Int_Bool:
        case RInternalBinaryOperator::GreaterThan_String_String_Bool:
        case RInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool:
        case RInternalBinaryOperator::LessThanOrEqual_String_String_Bool:
        case RInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool:
        case RInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool:
        case RInternalBinaryOperator::Equal_Int_Int_Bool:
        case RInternalBinaryOperator::Equal_Bool_Bool_Bool:
        case RInternalBinaryOperator::Equal_String_String_Bool:
            return factory.MakeBoolType();
        default:
            unreachable();
    }
}

NExp_CallGlobalFunc::NExp_CallGlobalFunc(const shared_ptr<RGlobalFuncDecl>& funcDecl, const RTypeArgumentsPtr& typeArgs, const vector<NArgument>& args)
    : funcDecl(funcDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr NExp_CallGlobalFunc::GetType(RTypeFactory& factory)
{
    return funcDecl->GetReturnType(*typeArgs, factory);
}

NExp_NewClass::NExp_NewClass(const shared_ptr<RClassConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const vector<NArgument>& args)
    : constructorDecl(constructorDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr NExp_NewClass::GetType(RTypeFactory& factory)
{
    auto classDecl = constructorDecl->GetClassDecl();
    assert(classDecl);

    return factory.MakeClassType(classDecl, typeArgs);
}

/////////////////////////////////////

NExp_CallClassMemberFunc::NExp_CallClassMemberFunc(shared_ptr<RClassMemberFuncDecl>&& classMemberFunc, RTypeArgumentsPtr&& typeArgs, NLocPtr&& instance, vector<NArgument>&& args)
    : classMemberFunc(std::move(classMemberFunc)), typeArgs(std::move(typeArgs)), instance(std::move(instance)), args(std::move(args))
{
}

RTypePtr NExp_CallClassMemberFunc::GetType(RTypeFactory& factory)
{
    return classMemberFunc->GetReturnType(*typeArgs, factory);
}

NExp_CastClass::NExp_CastClass(const NExpPtr& src, const RTypePtr& classType)
    : src(src), classType(classType)
{
}

RTypePtr NExp_CastClass::GetType(RTypeFactory& factory)
{
    return classType;
}

NExp_NewStruct::NExp_NewStruct(const shared_ptr<RStructConstructorDecl>& constructorDecl, RTypeArgumentsPtr&& typeArgs, vector<NArgument>&& args)
    : constructorDecl(constructorDecl), typeArgs(std::move(typeArgs)), args(std::move(args))
{
}

RTypePtr NExp_NewStruct::GetType(RTypeFactory& factory)
{
    auto structDecl = constructorDecl->GetStructDecl();
    return factory.MakeStructType(structDecl, typeArgs);
}

NExp_CallStructMemberFunc::NExp_CallStructMemberFunc(shared_ptr<RStructMemberFuncDecl>&& structMemberFuncDecl, RTypeArgumentsPtr&& typeArgs, NLocPtr&& instance, vector<NArgument>&& args)
    : structMemberFuncDecl(std::move(structMemberFuncDecl)), typeArgs(std::move(typeArgs)), instance(std::move(instance)), args(std::move(args))
{
}

RTypePtr NExp_CallStructMemberFunc::GetType(RTypeFactory& factory)
{
    return structMemberFuncDecl->GetReturnType(*typeArgs, factory);
}

NExp_NewEnumElem::NExp_NewEnumElem(const shared_ptr<REnumElemDecl>& enumElemDecl, const RTypeArgumentsPtr& typeArgs, vector<NArgument>&& args)
    : enumElemDecl(enumElemDecl), typeArgs(typeArgs), args(args)
{
}

NExp_NewEnumElem::NExp_NewEnumElem(const std::shared_ptr<REnumElemDecl>& enumElemDecl, RTypeArgumentsPtr&& typeArgs, std::vector<NArgument>&& args)
    : enumElemDecl(enumElemDecl), typeArgs(std::move(typeArgs)), args(std::move(args))
{
}

RTypePtr NExp_NewEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeEnumElemType(enumElemDecl, typeArgs);
}

NExp_CastEnumElemToEnum::NExp_CastEnumElemToEnum(const NExpPtr& src, const RTypePtr& enumType)
    : src(src), enumType(enumType)
{
}

RTypePtr NExp_CastEnumElemToEnum::GetType(RTypeFactory& factory)
{
    return enumType;
}

NExp_NullableValueNullLiteral::NExp_NullableValueNullLiteral(const RTypePtr& innerType)
    : innerType(innerType)
{
}

RTypePtr NExp_NullableValueNullLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerType);
}

NExp_NullableRefNullLiteral::NExp_NullableRefNullLiteral(const RTypePtr& innerType)
    : innerType(innerType)
{
}

RTypePtr NExp_NullableRefNullLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(innerType);
}

NExp_NewNullable::NExp_NewNullable(const NExpPtr& innerExp)
    : innerExp(innerExp)
{
}

RTypePtr NExp_NewNullable::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerExp->GetType(factory));
}

NExp_Lambda::NExp_Lambda(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const vector<NArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr NExp_Lambda::GetType(RTypeFactory& factory)
{
    return factory.MakeLambdaType(lambdaDecl, typeArgs);
}

NExp_CallLambda::NExp_CallLambda(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const NLocPtr& callable, const vector<NArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RTypePtr NExp_CallLambda::GetType(RTypeFactory& factory)
{
    return lambdaDecl->GetReturnType(*typeArgs, factory);
}

NExp_InlineBlock::NExp_InlineBlock(const vector<NStmtPtr>& stmts, const RTypePtr& returnType)
    : stmts(stmts), returnType(returnType)
{
}

RTypePtr NExp_InlineBlock::GetType(RTypeFactory& factory)
{
    return returnType;
}

NExp_ClassIsClass::NExp_ClassIsClass(const NExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr NExp_ClassIsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_ClassAsClass::NExp_ClassAsClass(const NExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr NExp_ClassAsClass::GetType(RTypeFactory& factory)
{    
    return factory.MakeNullableRefType(classType);
}

NExp_ClassIsInterface::NExp_ClassIsInterface(const NExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr NExp_ClassIsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_ClassAsInterface::NExp_ClassAsInterface(const NExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr NExp_ClassAsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

NExp_InterfaceIsClass::NExp_InterfaceIsClass(const NExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr NExp_InterfaceIsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_InterfaceAsClass::NExp_InterfaceAsClass(const NExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr NExp_InterfaceAsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(classType);
}

NExp_InterfaceIsInterface::NExp_InterfaceIsInterface(const NExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr NExp_InterfaceIsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_InterfaceAsInterface::NExp_InterfaceAsInterface(const NExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr NExp_InterfaceAsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

NExp_EnumIsEnumElem::NExp_EnumIsEnumElem(const NExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr NExp_EnumIsEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

NExp_EnumAsEnumElem::NExp_EnumAsEnumElem(const NExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr NExp_EnumAsEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(enumElemType);
}

} // namespace Citron