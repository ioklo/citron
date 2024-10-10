#include "RExp.h"

#include <Infra/Unreachable.h>

#include "RLoc.h"
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

RExp_Load::RExp_Load(RLocPtr&& loc)
    : loc(std::move(loc))
{
}

RTypePtr RExp_Load::GetType(RTypeFactory& factory)
{
    return loc->GetType(factory);
}

RExp_Assign::RExp_Assign(RLocPtr&& dest, RExpPtr&& src)
    : dest(std::move(dest)), src(std::move(src))
{
}

RTypePtr RExp_Assign::GetType(RTypeFactory& factory)
{
    return dest->GetType(factory);
}

RExp_Box::RExp_Box(RExpPtr&& innerExp)
    : innerExp(std::move(innerExp))
{
}

RTypePtr RExp_Box::GetType(RTypeFactory& factory)
{
    auto innerType = innerExp->GetType(factory);
    return factory.MakeBoxPtrType(move(innerType));
}

RExp_StaticBoxRef::RExp_StaticBoxRef(const RLocPtr& loc)
    : loc(loc)
{
}

RTypePtr RExp_StaticBoxRef::GetType(RTypeFactory& factory)
{
    return factory.MakeBoxPtrType(loc->GetType(factory));
}

RExp_ClassMemberBoxRef::RExp_ClassMemberBoxRef(const RLocPtr& holder, const shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RExp_ClassMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

RExp_StructIndirectMemberBoxRef::RExp_StructIndirectMemberBoxRef(const RLocPtr& holder, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RExp_StructIndirectMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

RExp_StructMemberBoxRef::RExp_StructMemberBoxRef(const RLocPtr& parent, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RExp_StructMemberBoxRef::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxPtrType(move(declType));
}

RExp_LocalRef::RExp_LocalRef(const RLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr RExp_LocalRef::GetType(RTypeFactory& factory)
{
    auto innerLocType = innerLoc->GetType(factory);
    return factory.MakeLocalPtrType(move(innerLocType));
}

RExp_CastBoxedLambdaToFunc::RExp_CastBoxedLambdaToFunc(const RExpPtr& exp, const shared_ptr<RType_Func>& funcType)
    : exp(exp), funcType(funcType)
{
}

RTypePtr RExp_CastBoxedLambdaToFunc::GetType(RTypeFactory& factory)
{
    return funcType;
}

RExp_BoolLiteral::RExp_BoolLiteral(bool value)
    : value(value)
{
}

RTypePtr RExp_BoolLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_IntLiteral::RExp_IntLiteral(int value)
    : value(value)
{
}

RTypePtr RExp_IntLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeIntType();
}

RTextStringExpElement::RTextStringExpElement(const string& text)
    : text(text)
{
}

RLocStringExpElement::RLocStringExpElement(RLocPtr&& loc)
    : loc(loc)
{

}

RExp_String::RExp_String(vector<RStringExpElement>&& elements)
    : elements(std::move(elements))
{
}

RTypePtr RExp_String::GetType(RTypeFactory& factory)
{
    return factory.MakeStringType();
}

RExp_List::RExp_List(vector<RExpPtr>&& elems, const RTypePtr& itemType)
    : elems(move(elems)), itemType(itemType)
{
}

RTypePtr RExp_List::GetType(RTypeFactory& factory)
{
    return factory.MakeListType(itemType);
}

RExp_ListIterator::RExp_ListIterator(const RLocPtr& listLoc, const RTypePtr& iteratorType)
    : listLoc(listLoc), type(type)
{
}


RTypePtr RExp_ListIterator::GetType(RTypeFactory& factory)
{
    return type;
}

RExp_CallInternalUnaryOperator::RExp_CallInternalUnaryOperator(RInternalUnaryOperator op, RExpPtr&& operand)
    : op(op), operand(std::move(operand))
{
}

RTypePtr RExp_CallInternalUnaryOperator::GetType(RTypeFactory& factory)
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

RExp_CallInternalUnaryAssignOperator::RExp_CallInternalUnaryAssignOperator(RInternalUnaryAssignOperator op, RLocPtr&& operand)
    : op(op), operand(std::move(operand))
{
}

RTypePtr RExp_CallInternalUnaryAssignOperator::GetType(RTypeFactory& factory)
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

RExp_CallInternalBinaryOperator::RExp_CallInternalBinaryOperator(RInternalBinaryOperator op, RExpPtr&& operand0, RExpPtr&& operand1)
    : op(op), operand0(std::move(operand0)), operand1(std::move(operand1))
{
}

RTypePtr RExp_CallInternalBinaryOperator::GetType(RTypeFactory& factory)
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
    }
}

RExp_CallGlobalFunc::RExp_CallGlobalFunc(const shared_ptr<RGlobalFuncDecl>& funcDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : funcDecl(funcDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RExp_CallGlobalFunc::GetType(RTypeFactory& factory)
{
    return funcDecl->GetReturnType(*typeArgs, factory);
}

RExp_NewClass::RExp_NewClass(const shared_ptr<RClassConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : constructorDecl(constructorDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RExp_NewClass::GetType(RTypeFactory& factory)
{
    auto classDecl = constructorDecl->_class.lock();
    assert(classDecl);

    return factory.MakeClassType(classDecl, typeArgs);
}

/////////////////////////////////////

RExp_CallClassMemberFunc::RExp_CallClassMemberFunc(const shared_ptr<RClassMemberFuncDecl>& classMemberFunc, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, vector<RArgument>&& args)
    : classMemberFunc(classMemberFunc), typeArgs(typeArgs), instance(std::move(instance)), args(std::move(args))
{
}

RTypePtr RExp_CallClassMemberFunc::GetType(RTypeFactory& factory)
{
    return classMemberFunc->GetReturnType(*typeArgs, factory);
}

RExp_CastClass::RExp_CastClass(const RExpPtr& src, const RTypePtr& classType)
    : src(src), classType(classType)
{
}

RTypePtr RExp_CastClass::GetType(RTypeFactory& factory)
{
    return classType;
}

RExp_NewStruct::RExp_NewStruct(const shared_ptr<RStructConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : constructorDecl(constructorDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RExp_NewStruct::GetType(RTypeFactory& factory)
{
    auto structDecl = constructorDecl->_struct.lock();
    return factory.MakeStructType(structDecl, typeArgs);
}

RExp_CallStructMemberFunc::RExp_CallStructMemberFunc(const shared_ptr<RStructMemberFuncDecl>& structMemberFuncDecl, const RTypeArgumentsPtr& typeArgs, RLocPtr&& instance, vector<RArgument>&& args)
    : structMemberFuncDecl(structMemberFuncDecl), typeArgs(typeArgs), instance(std::move(instance)), args(std::move(args))
{
}

RTypePtr RExp_CallStructMemberFunc::GetType(RTypeFactory& factory)
{
    return structMemberFuncDecl->GetReturnType(*typeArgs, factory);
}

RExp_NewEnumElem::RExp_NewEnumElem(const shared_ptr<REnumElemDecl>& enumElemDecl, const RTypeArgumentsPtr& typeArgs, vector<RArgument>&& args)
    : enumElemDecl(enumElemDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RExp_NewEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeEnumElemType(enumElemDecl, typeArgs);
}

RExp_CastEnumElemToEnum::RExp_CastEnumElemToEnum(const RExpPtr& src, const RTypePtr& enumType)
    : src(src), enumType(enumType)
{
}

RTypePtr RExp_CastEnumElemToEnum::GetType(RTypeFactory& factory)
{
    return enumType;
}

RExp_NullableValueNullLiteral::RExp_NullableValueNullLiteral(const RTypePtr& innerType)
    : innerType(innerType)
{
}

RTypePtr RExp_NullableValueNullLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerType);
}

RExp_NullableRefNullLiteral::RExp_NullableRefNullLiteral(const RTypePtr& innerType)
    : innerType(innerType)
{
}

RTypePtr RExp_NullableRefNullLiteral::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(innerType);
}

RExp_NewNullable::RExp_NewNullable(const RExpPtr& innerExp)
    : innerExp(innerExp)
{
}

RTypePtr RExp_NewNullable::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerExp->GetType(factory));
}

RExp_Lambda::RExp_Lambda(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RExp_Lambda::GetType(RTypeFactory& factory)
{
    static_assert(false); // 람다는 struct로 인코딩 되어야 한다 (module private 공간에 만들어진다)
}

RExp_CallLambda::RExp_CallLambda(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const RLocPtr& callable, const vector<RArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RTypePtr RExp_CallLambda::GetType(RTypeFactory& factory)
{
    return lambdaDecl->GetReturnType(*typeArgs, factory);
}

RExp_InlineBlock::RExp_InlineBlock(const vector<RStmtPtr>& stmts, const RTypePtr& returnType)
    : stmts(stmts), returnType(returnType)
{
}

RTypePtr RExp_InlineBlock::GetType(RTypeFactory& factory)
{
    return returnType;
}

RExp_ClassIsClass::RExp_ClassIsClass(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RExp_ClassIsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_ClassAsClass::RExp_ClassAsClass(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RExp_ClassAsClass::GetType(RTypeFactory& factory)
{    
    return factory.MakeNullableRefType(classType);
}

RExp_ClassIsInterface::RExp_ClassIsInterface(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RExp_ClassIsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_ClassAsInterface::RExp_ClassAsInterface(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RExp_ClassAsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

RExp_InterfaceIsClass::RExp_InterfaceIsClass(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RExp_InterfaceIsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_InterfaceAsClass::RExp_InterfaceAsClass(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RExp_InterfaceAsClass::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(classType);
}

RExp_InterfaceIsInterface::RExp_InterfaceIsInterface(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RExp_InterfaceIsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_InterfaceAsInterface::RExp_InterfaceAsInterface(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RExp_InterfaceAsInterface::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

RExp_EnumIsEnumElem::RExp_EnumIsEnumElem(const RExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr RExp_EnumIsEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RExp_EnumAsEnumElem::RExp_EnumAsEnumElem(const RExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr RExp_EnumAsEnumElem::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(enumElemType);
}

} // namespace Citron