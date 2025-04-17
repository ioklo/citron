module Citron.SyntaxIR0Translator:IrExp;

import Citron.Ptr;
import Citron.RDecls;

using namespace std;

namespace Citron::SyntaxIR0Translator {

IrExp_Namespace::IrExp_Namespace(const shared_ptr<RNamespaceDecl>& decl)
    : decl(decl)
{
}

IrExp_TypeVar::IrExp_TypeVar(const shared_ptr<RType_TypeVar>& type)
    : type(type)
{

}

IrExp_Class::IrExp_Class(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Struct::IrExp_Struct(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Enum::IrExp_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_ThisVar::IrExp_ThisVar(const RTypePtr& type)
    : type(type)
{
}

IrExp_StaticRef::IrExp_StaticRef(const NLocPtr& loc)
    : loc(loc)
{
}

IrExp_BoxRef_ClassMember::IrExp_BoxRef_ClassMember(const NLocPtr& loc, const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_ClassMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLocPtr IrExp_BoxRef_ClassMember::MakeLoc()
{
    return MakePtr<NLoc_ClassVar>(NLocPtr{loc}, decl, typeArgs);
}

IrExp_BoxRef_StructIndirectMember::IrExp_BoxRef_StructIndirectMember(const NLocPtr& loc, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_StructIndirectMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLocPtr IrExp_BoxRef_StructIndirectMember::MakeLoc()
{
    return MakePtr<NLoc_StructVar>(MakePtr<NLoc_BoxDeref>(NLocPtr{loc}), decl, typeArgs);
}

IrExp_BoxRef_StructMember::IrExp_BoxRef_StructMember(const std::shared_ptr<IrExp_BoxRef>& parent, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_StructMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLocPtr IrExp_BoxRef_StructMember::MakeLoc()
{
    return MakePtr<NLoc_StructVar>(parent->MakeLoc(), decl, typeArgs);
}

IrExp_LocalRef::IrExp_LocalRef(const NLocPtr& loc)
    : loc(loc)
{
}

IrExp_LocalValue::IrExp_LocalValue(NExpPtr&& exp)
    : exp(move(exp))
{

}

IrExp_DerefedBoxValue::IrExp_DerefedBoxValue(NLocPtr&& innerLoc)
    : innerLoc(move(innerLoc))
{
}

} // Citron::SyntaxIR0Translator