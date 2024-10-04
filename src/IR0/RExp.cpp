#include "RExp.h"

#include <Infra/Unreachable.h>

#include "RLoc.h"
#include "RTypeFactory.h"

#include "RGlobalFuncDecl.h"
#include "RLambdaDecl.h"

#include "RStructMemberVarDecl.h"
#include "RStructMemberFuncDecl.h"
#include "RStructConstructorDecl.h"

#include "RClassConstructorDecl.h"
#include "RClassMemberFuncDecl.h"
#include "RClassMemberVarDecl.h"

using namespace std;

namespace Citron {

RLoadExp::RLoadExp(const RLocPtr& loc)
    : loc(loc)
{
}

RTypePtr RLoadExp::GetType(RTypeFactory& factory)
{
    return loc->GetType(factory);
}

RAssignExp::RAssignExp(const RLocPtr& dest, const RExpPtr& src)
    : dest(dest), src(src)
{
}

RTypePtr RAssignExp::GetType(RTypeFactory& factory)
{
    return dest->GetType(factory);
}

RBoxExp::RBoxExp(const RExpPtr& innerExp)
    : innerExp(innerExp)
{
}

RTypePtr RBoxExp::GetType(RTypeFactory& factory)
{
    auto innerType = innerExp->GetType(factory);
    return factory.MakeBoxPtrType(move(innerType));
}

RStaticBoxRefExp::RStaticBoxRefExp(const RLocPtr& loc)
    : loc(loc)
{
}

RTypePtr RStaticBoxRefExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoxPtrType(loc->GetType(factory));
}

RClassMemberBoxRefExp::RClassMemberBoxRefExp(const RLocPtr& holder, const shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RClassMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

RStructIndirectMemberBoxRefExp::RStructIndirectMemberBoxRefExp(const RLocPtr& holder, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : holder(holder), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RStructIndirectMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);
    return factory.MakeBoxPtrType(move(declType));
}

RStructMemberBoxRefExp::RStructMemberBoxRefExp(const RLocPtr& parent, const shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RStructMemberBoxRefExp::GetType(RTypeFactory& factory)
{
    auto declType = memberVarDecl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxPtrType(move(declType));
}

RLocalRefExp::RLocalRefExp(const RLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr RLocalRefExp::GetType(RTypeFactory& factory)
{
    auto innerLocType = innerLoc->GetType(factory);
    return factory.MakeLocalPtrType(move(innerLocType));
}

RCastBoxedLambdaToFuncExp::RCastBoxedLambdaToFuncExp(const RExpPtr& exp, const shared_ptr<RFuncType>& funcType)
    : exp(exp), funcType(funcType)
{
}

RTypePtr RCastBoxedLambdaToFuncExp::GetType(RTypeFactory& factory)
{
    return funcType;
}

RBoolLiteralExp::RBoolLiteralExp(bool value)
    : value(value)
{
}

RTypePtr RBoolLiteralExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RIntLiteralExp::RIntLiteralExp(int value)
    : value(value)
{
}

RTypePtr RIntLiteralExp::GetType(RTypeFactory& factory)
{
    return factory.MakeIntType();
}

RTextStringExpElement::RTextStringExpElement(const string& text)
    : text(text)
{
}

RLocStringExpElement::RLocStringExpElement(const RLocPtr& loc)
    : loc(loc)
{

}

RStringExp::RStringExp(const vector<RStringExpElement>& elements)
    : elements(elements)
{
}

RTypePtr RStringExp::GetType(RTypeFactory& factory)
{
    return factory.MakeStringType();
}

RListExp::RListExp(vector<RExpPtr>&& elems, const RTypePtr& itemType)
    : elems(move(elems)), itemType(itemType)
{
}

RTypePtr RListExp::GetType(RTypeFactory& factory)
{
    return factory.MakeListType(itemType);
}

RListIteratorExp::RListIteratorExp(const RLocPtr& listLoc, const RTypePtr& iteratorType)
    : listLoc(listLoc), type(type)
{
}


RTypePtr RListIteratorExp::GetType(RTypeFactory& factory)
{
    return type;
}

RCallInternalUnaryOperatorExp::RCallInternalUnaryOperatorExp(RInternalUnaryOperator op, const RExpPtr& operand)
    : op(op), operand(operand)
{
}

RTypePtr RCallInternalUnaryOperatorExp::GetType(RTypeFactory& factory)
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

RCallInternalUnaryAssignOperatorExp::RCallInternalUnaryAssignOperatorExp(RInternalUnaryAssignOperator op, const RLocPtr& operand)
    : op(op), operand(operand)
{
}

RTypePtr RCallInternalUnaryAssignOperatorExp::GetType(RTypeFactory& factory)
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

RCallInternalBinaryOperatorExp::RCallInternalBinaryOperatorExp(RInternalBinaryOperator op, const RExpPtr& operand0, const RExpPtr& operand1)
    : op(op), operand0(operand0), operand1(operand1)
{
}

RTypePtr RCallInternalBinaryOperatorExp::GetType(RTypeFactory& factory)
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

RCallGlobalFuncExp::RCallGlobalFuncExp(const shared_ptr<RGlobalFuncDecl>& funcDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : funcDecl(funcDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RCallGlobalFuncExp::GetType(RTypeFactory& factory)
{
    return funcDecl->GetReturnType(*typeArgs, factory);
}

RNewClassExp::RNewClassExp(const shared_ptr<RClassConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : constructorDecl(constructorDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RNewClassExp::GetType(RTypeFactory& factory)
{
    return constructorDecl->GetClassType(typeArgs, factory);

    /*auto classDecl = constructorDecl->_class.lock();
    assert(classDecl);

    return factory.MakeInstanceType(classDecl->GetDeclId(), typeArgs);*/
}

/////////////////////////////////////

RCallClassMemberFuncExp::RCallClassMemberFuncExp(const shared_ptr<RClassMemberFuncDecl>& classMemberFunc, const RTypeArgumentsPtr& typeArgs, const RLocPtr& instance, const vector<RArgument>& args)
    : classMemberFunc(classMemberFunc), typeArgs(typeArgs), instance(instance), args(args)
{
}

RTypePtr RCallClassMemberFuncExp::GetType(RTypeFactory& factory)
{
    return classMemberFunc->GetReturnType(*typeArgs, factory);
}

RCastClassExp::RCastClassExp(const RExpPtr& src, const RTypePtr& classType)
    : src(src), classType(classType)
{
}

RTypePtr RCastClassExp::GetType(RTypeFactory& factory)
{
    return classType;
}

RNewStructExp::RNewStructExp(const shared_ptr<RStructConstructorDecl>& constructorDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : constructorDecl(constructorDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RNewStructExp::GetType(RTypeFactory& factory)
{
    auto structDecl = constructorDecl->_struct.lock();
    return factory.MakeInstanceType(structDecl->GetDeclId(), typeArgs);
}

RCallStructMemberFuncExp::RCallStructMemberFuncExp(const shared_ptr<RStructMemberFuncDecl>& structMemberFuncDecl, const RTypeArgumentsPtr& typeArgs, const RLocPtr& instance, const vector<RArgument>& args)
    : structMemberFuncDecl(structMemberFuncDecl), typeArgs(typeArgs), instance(instance), args(args)
{
}

RTypePtr RCallStructMemberFuncExp::GetType(RTypeFactory& factory)
{
    return structMemberFuncDecl->GetReturnType(*typeArgs, factory);
}

RNewEnumElemExp::RNewEnumElemExp(const shared_ptr<REnumElemDecl>& enumElemDecl, const RTypeArgumentsPtr& typeArgs, vector<RArgument>&& args)
    : enumElemDecl(enumElemDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RNewEnumElemExp::GetType(RTypeFactory& factory)
{
    return factory.MakeInstanceType(enumElemDecl->GetDeclId(), typeArgs);
}

RCastEnumElemToEnumExp::RCastEnumElemToEnumExp(const RExpPtr& src, const RTypePtr& enumType)
    : src(src), enumType(enumType)
{
}

RTypePtr RCastEnumElemToEnumExp::GetType(RTypeFactory& factory)
{
    return enumType;
}

RNullableNullLiteralExp::RNullableNullLiteralExp(const RTypePtr& innerType)
    : innerType(innerType)
{

}

RTypePtr RNullableNullLiteralExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerType);
}

RNewNullableExp::RNewNullableExp(const RExpPtr& innerExp)
    : innerExp(innerExp)
{
}

RTypePtr RNewNullableExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(innerExp->GetType(factory));
}

RLambdaExp::RLambdaExp(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const vector<RArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), args(args)
{
}

RTypePtr RLambdaExp::GetType(RTypeFactory& factory)
{
    static_assert(false); // 람다는 struct로 인코딩 되어야 한다 (module private 공간에 만들어진다)
}

RCallLambdaExp::RCallLambdaExp(const shared_ptr<RLambdaDecl>& lambdaDecl, const RTypeArgumentsPtr& typeArgs, const RLocPtr& callable, const vector<RArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RTypePtr RCallLambdaExp::GetType(RTypeFactory& factory)
{
    return lambdaDecl->GetReturnType(*typeArgs, factory);
}

RInlineBlockExp::RInlineBlockExp(const vector<RStmtPtr>& stmts, const RTypePtr& returnType)
    : stmts(stmts), returnType(returnType)
{
}

RTypePtr RInlineBlockExp::GetType(RTypeFactory& factory)
{
    return returnType;
}

RClassIsClassExp::RClassIsClassExp(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RClassIsClassExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RClassAsClassExp::RClassAsClassExp(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RClassAsClassExp::GetType(RTypeFactory& factory)
{    
    return factory.MakeNullableRefType(classType);
}

RClassIsInterfaceExp::RClassIsInterfaceExp(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RClassIsInterfaceExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RClassAsInterfaceExp::RClassAsInterfaceExp(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RClassAsInterfaceExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

RInterfaceIsClassExp::RInterfaceIsClassExp(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RInterfaceIsClassExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RInterfaceAsClassExp::RInterfaceAsClassExp(const RExpPtr& exp, const RTypePtr& classType)
    : exp(exp), classType(classType)
{
}

RTypePtr RInterfaceAsClassExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(classType);
}

RInterfaceIsInterfaceExp::RInterfaceIsInterfaceExp(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RInterfaceIsInterfaceExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

RInterfaceAsInterfaceExp::RInterfaceAsInterfaceExp(const RExpPtr& exp, const RTypePtr& interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RTypePtr RInterfaceAsInterfaceExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableRefType(interfaceType);
}

REnumIsEnumElemExp::REnumIsEnumElemExp(const RExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr REnumIsEnumElemExp::GetType(RTypeFactory& factory)
{
    return factory.MakeBoolType();
}

REnumAsEnumElemExp::REnumAsEnumElemExp(const RExpPtr& exp, const RTypePtr& enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RTypePtr REnumAsEnumElemExp::GetType(RTypeFactory& factory)
{
    return factory.MakeNullableValueType(enumElemType);
}

} // namespace Citron