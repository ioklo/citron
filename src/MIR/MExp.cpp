#include "MExp.h"

#include <cassert>

#include "Infra/Unreachable.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "NSymbol/NLambdaDecl.h"

#include "MLoc.h"


using namespace std;

namespace Citron {

MExp_Load::MExp_Load(MLoc* loc)
    : loc(loc)
{
}

RType* MExp_Load::GetType()
{
    return loc->GetType();
}

MExp_Assign::MExp_Assign(MLoc* dest, MExp* src)
    : dest{dest}, src{src}
{
}

RType* MExp_Assign::GetType()
{
    return dest->GetType();
}

MExp_Box::MExp_Box(MExp* innerExp, const RFactoryPtr& rFactory)
    : innerExp{innerExp}, rFactory{rFactory}
{
}

RType* MExp_Box::GetType()
{
    auto* innerType = innerExp->GetType();
    return rFactory->MakeBoxType(innerType);
}

MExp_StaticBoxRef::MExp_StaticBoxRef(MLoc* loc, const RFactoryPtr& rFactory)
    : loc{loc}, rFactory{rFactory}
{
}

RType* MExp_StaticBoxRef::GetType()
{
    return rFactory->MakeBoxType(loc->GetType());
}

MExp_ClassMemberBoxRef::MExp_ClassMemberBoxRef(MLoc* holder, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : holder(holder), decl(decl), typeArgs(typeArgs), rFactory{rFactory}
{
}

RType* MExp_ClassMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MExp_StructIndirectMemberBoxRef::MExp_StructIndirectMemberBoxRef(MLoc* holder, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : holder{holder}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MExp_StructIndirectMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MExp_StructMemberBoxRef::MExp_StructMemberBoxRef(MLoc* parent, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RType* MExp_StructMemberBoxRef::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs, factory);

    return factory.MakeBoxType(declType);
}

MExp_LocalRef::MExp_LocalRef(MLoc* innerLoc)
    : innerLoc(innerLoc)
{
}

RType* MExp_LocalRef::GetType()
{
    auto* innerLocType = innerLoc->GetType(factory);
    return factory.MakePtrType(innerLocType);
}

MExp_CastBoxedLambdaToFunc::MExp_CastBoxedLambdaToFunc(MExp* exp, RType_Func* funcType)
    : exp(exp), funcType(funcType)
{
}

RType* MExp_CastBoxedLambdaToFunc::GetType()
{
    return funcType;
}

MExp_BoolLiteral::MExp_BoolLiteral(bool value)
    : value(value)
{
}

RType* MExp_BoolLiteral::GetType()
{
    return factory.MakeBoolType();
}

MExp_IntLiteral::MExp_IntLiteral(int value)
    : value(value)
{
}

RType* MExp_IntLiteral::GetType()
{
    return factory.MakeIntType();
}

MExp_StringElem_Text::MExp_StringElem_Text(const string& text)
    : text{text}
{
}

MExp_StringElem_Exp::MExp_StringElem_Exp(MExp* mExp)
    : mExp{mExp}
{

}

MExp_String::MExp_String(vector<MExp_StringElem>&& elements)
    : elements(move(elements))
{
}

RType* MExp_String::GetType()
{
    return factory.MakeStringType();
}

MExp_List::MExp_List(vector<MExp*>&& elems, RType* itemType)
    : elems(move(elems)), itemType(itemType)
{
}

RType* MExp_List::GetType()
{
    return factory.MakeListType(itemType);
}

MExp_ListIterator::MExp_ListIterator(MLoc* listLoc, RType* iteratorType)
    : listLoc(listLoc), iteratorType{iteratorType}
{
}

RType* MExp_ListIterator::GetType()
{
    return iteratorType;
}

MExp_CallInternalUnaryOperator::MExp_CallInternalUnaryOperator(MInternalUnaryOperator op, MExp* operand)
    : op{op}, operand{operand}
{
}

RType* MExp_CallInternalUnaryOperator::GetType()
{
    switch (op)
    {
    case MInternalUnaryOperator::LogicalNot_Bool_Bool: return factory.MakeBoolType();
    case MInternalUnaryOperator::UnaryMinus_Int_Int: return factory.MakeIntType();
    case MInternalUnaryOperator::ToString_Bool_String: return factory.MakeStringType();
    case MInternalUnaryOperator::ToString_Int_String: return factory.MakeStringType();
    }

    unreachable();
}

MExp_CallInternalUnaryAssignOperator::MExp_CallInternalUnaryAssignOperator(MInternalUnaryAssignOperator op, MLoc* operand)
    : op{op}, operand{operand}
{
}

RType* MExp_CallInternalUnaryAssignOperator::GetType()
{
    switch(op)
    {
    case MInternalUnaryAssignOperator::PrefixInc_Int_Int:
    case MInternalUnaryAssignOperator::PrefixDec_Int_Int:
    case MInternalUnaryAssignOperator::PostfixInc_Int_Int:
    case MInternalUnaryAssignOperator::PostfixDec_Int_Int:
        return factory.MakeIntType();
    }

    unreachable();
}

MExp_CallInternalBinaryOperator::MExp_CallInternalBinaryOperator(MInternalBinaryOperator op, MExp* operand0, MExp* operand1)
    : op(op), operand0(operand0), operand1(operand1)
{
}

RType* MExp_CallInternalBinaryOperator::GetType()
{
    switch(op)
    {
        case MInternalBinaryOperator::Multiply_Int_Int_Int:
        case MInternalBinaryOperator::Divide_Int_Int_Int:
        case MInternalBinaryOperator::Modulo_Int_Int_Int:
        case MInternalBinaryOperator::Add_Int_Int_Int:
            return factory.MakeIntType();

        case MInternalBinaryOperator::Add_String_String_String:
            return factory.MakeStringType();

        case MInternalBinaryOperator::Subtract_Int_Int_Int:
            return factory.MakeIntType();

        case MInternalBinaryOperator::LessThan_Int_Int_Bool:
        case MInternalBinaryOperator::LessThan_String_String_Bool:
        case MInternalBinaryOperator::GreaterThan_Int_Int_Bool:
        case MInternalBinaryOperator::GreaterThan_String_String_Bool:
        case MInternalBinaryOperator::LessThanOrEqual_Int_Int_Bool:
        case MInternalBinaryOperator::LessThanOrEqual_String_String_Bool:
        case MInternalBinaryOperator::GreaterThanOrEqual_Int_Int_Bool:
        case MInternalBinaryOperator::GreaterThanOrEqual_String_String_Bool:
        case MInternalBinaryOperator::Equal_Int_Int_Bool:
        case MInternalBinaryOperator::Equal_Bool_Bool_Bool:
        case MInternalBinaryOperator::Equal_String_String_Bool:
            return factory.MakeBoolType();
        default:
            unreachable();
    }
}

MExp_CallGlobalFunc::MExp_CallGlobalFunc(RGlobalFuncDecl* rFuncDecl, RTypeArguments* rTypeArgs, const vector<MArgument>& args)
    : rFuncDecl{rFuncDecl}, rTypeArgs{rTypeArgs}, args{args}
{
}

RType* MExp_CallGlobalFunc::GetType()
{
    return rFuncDecl->GetReturnType(*rTypeArgs, factory);
}

MExp_NewClass::MExp_NewClass(RClassCtorDecl* ctorDecl, RTypeArguments* typeArgs, const vector<MArgument>& args)
    : ctorDecl(ctorDecl), typeArgs(typeArgs), args(args)
{
}

RType* MExp_NewClass::GetType()
{
    auto classDecl = ctorDecl->GetClassDecl();
    assert(classDecl);

    return factory.MakeClassType(classDecl, typeArgs);
}

/////////////////////////////////////

MExp_CallClassFunc::MExp_CallClassFunc(RClassFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* MExp_CallClassFunc::GetType()
{
    return decl->GetReturnType(*typeArgs, factory);
}

MExp_CastClass::MExp_CastClass(MExp* src, RType* classType)
    : src(src), classType(classType)
{
}

RType* MExp_CastClass::GetType()
{
    return classType;
}

MExp_NewStruct::MExp_NewStruct(RStructCtorDecl* ctor, RTypeArguments* typeArgs, vector<MArgument>&& args)
    : ctor{ctor}, typeArgs{typeArgs}, args{move(args)}
{
}

RType* MExp_NewStruct::GetType()
{
    auto structDecl = ctor->GetStructDecl();
    return factory.MakeStructType(structDecl, typeArgs);
}

MExp_CallStructFunc::MExp_CallStructFunc(RStructFuncDecl* decl, RTypeArguments* typeArgs, MLoc* instance, vector<MArgument>&& args)
    : decl{decl}, typeArgs{typeArgs}, instance{instance}, args{move(args)}
{
}

RType* MExp_CallStructFunc::GetType()
{
    return decl->GetReturnType(*typeArgs, factory);
}

MExp_NewEnumElem::MExp_NewEnumElem(REnumElemDecl* enumElemDecl, RTypeArguments* typeArgs, vector<MArgument>&& args)
    : enumElemDecl{enumElemDecl}, typeArgs{typeArgs}, args{move(args)}
{
}

RType* MExp_NewEnumElem::GetType()
{
    return factory.MakeEnumElemType(enumElemDecl, typeArgs);
}

MExp_CastEnumElemToEnum::MExp_CastEnumElemToEnum(MExp* src, RType* enumType)
    : src(src), enumType(enumType)
{
}

RType* MExp_CastEnumElemToEnum::GetType()
{
    return enumType;
}

MExp_NullableValueNullLiteral::MExp_NullableValueNullLiteral(RType* innerType)
    : innerType(innerType)
{
}

RType* MExp_NullableValueNullLiteral::GetType()
{
    return factory.MakeNullableValueType(innerType);
}

MExp_NullableRefNullLiteral::MExp_NullableRefNullLiteral(RType* innerType)
    : innerType(innerType)
{
}

RType* MExp_NullableRefNullLiteral::GetType()
{
    return factory.MakeNullableRefType(innerType);
}

MExp_NewNullable::MExp_NewNullable(MExp* innerExp)
    : innerExp(innerExp)
{
}

RType* MExp_NewNullable::GetType()
{
    return factory.MakeNullableValueType(innerExp->GetType(factory));
}

MExp_Lambda::MExp_Lambda(NLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, const vector<MArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), args(args)
{
}

RType* MExp_Lambda::GetType()
{
    return factory.MakeLambdaType(lambdaDecl, typeArgs);
}

MExp_CallLambda::MExp_CallLambda(RLambdaDecl* lambdaDecl, RTypeArguments* typeArgs, MLoc* callable, const vector<MArgument>& args)
    : lambdaDecl(lambdaDecl), typeArgs(typeArgs), callable(callable), args(args)
{
}

RType* MExp_CallLambda::GetType()
{
    return lambdaDecl->GetReturnType(*typeArgs, factory);
}

MExp_InlineBlock::MExp_InlineBlock(const vector<MStmt*>& stmts, RType* returnType)
    : stmts(stmts), returnType(returnType)
{
}

RType* MExp_InlineBlock::GetType()
{
    return returnType;
}

MExp_ClassIsClass::MExp_ClassIsClass(MExp* exp, RType* classType)
    : exp{exp}, classType{classType}
{
}

RType* MExp_ClassIsClass::GetType()
{
    return factory.MakeBoolType();
}

MExp_ClassAsClass::MExp_ClassAsClass(MExp* exp, RType* classType)
    : exp(exp), classType(classType)
{
}

RType* MExp_ClassAsClass::GetType()
{    
    return factory.MakeNullableRefType(classType);
}

MExp_ClassIsInterface::MExp_ClassIsInterface(MExp* exp, RType* interfaceType)
    : exp{exp}, interfaceType{interfaceType}
{
}

RType* MExp_ClassIsInterface::GetType()
{
    return factory.MakeBoolType();
}

MExp_ClassAsInterface::MExp_ClassAsInterface(MExp* exp, RType* interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RType* MExp_ClassAsInterface::GetType()
{
    return factory.MakeNullableRefType(interfaceType);
}

MExp_InterfaceIsClass::MExp_InterfaceIsClass(MExp* exp, RType* classType)
    : exp{exp}, classType{classType}
{
}

RType* MExp_InterfaceIsClass::GetType()
{
    return factory.MakeBoolType();
}

MExp_InterfaceAsClass::MExp_InterfaceAsClass(MExp* exp, RType* classType)
    : exp(exp), classType(classType)
{
}

RType* MExp_InterfaceAsClass::GetType()
{
    return factory.MakeNullableRefType(classType);
}

MExp_InterfaceIsInterface::MExp_InterfaceIsInterface(MExp* exp, RType* interfaceType)
    : exp{exp}, interfaceType{interfaceType}
{
}

RType* MExp_InterfaceIsInterface::GetType()
{
    return factory.MakeBoolType();
}

MExp_InterfaceAsInterface::MExp_InterfaceAsInterface(MExp* exp, RType* interfaceType)
    : exp(exp), interfaceType(interfaceType)
{
}

RType* MExp_InterfaceAsInterface::GetType()
{
    return factory.MakeNullableRefType(interfaceType);
}

MExp_EnumIsEnumElem::MExp_EnumIsEnumElem(MExp* exp, RType* enumElemType)
    : exp{exp}, enumElemType{enumElemType}
{
}

RType* MExp_EnumIsEnumElem::GetType()
{
    return factory.MakeBoolType();
}

MExp_EnumAsEnumElem::MExp_EnumAsEnumElem(MExp* exp, RType* enumElemType)
    : exp(exp), enumElemType(enumElemType)
{
}

RType* MExp_EnumAsEnumElem::GetType()
{
    return factory.MakeNullableValueType(enumElemType);
}

} // namespace Citron