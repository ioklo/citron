#include "RMember.h"

#include "Infra/Variants.h"

#include "RGlobalFuncDecl.h"
#include "RClassFuncDecl.h"
#include "RStructFuncDecl.h"
#include "DeclWithOuterTypeArgs.h"

using namespace std;

namespace Citron {

RMember_Namespace::RMember_Namespace(RNamespaceDecl* decl)
    : decl(decl)
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items)
    : items(move(items))
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(const RMember_GlobalFuncs& member) = default;

RMember_GlobalFuncs::~RMember_GlobalFuncs() = default;

RMember_Class::RMember_Class(RTypeArguments* outerTypeArgs, RClassDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_ClassFuncs::RMember_ClassFuncs(vector<DeclWithOuterTypeArgs<RClassFuncDecl>>&& items)
    : items(move(items))
{
}

RMember_ClassFuncs::RMember_ClassFuncs(const RMember_ClassFuncs&) = default;

RMember_ClassFuncs::~RMember_ClassFuncs() = default;

RMember_ClassVar::RMember_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Struct::RMember_Struct(RTypeArguments* outerTypeArgs, RStructDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_StructFuncs::RMember_StructFuncs(vector<DeclWithOuterTypeArgs<RStructFuncDecl>>&& items)
    : items(move(items))
{

}

RMember_StructFuncs::RMember_StructFuncs(const RMember_StructFuncs&) = default;

RMember_StructFuncs::~RMember_StructFuncs() = default;

RMember_StructVar::RMember_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Enum::RMember_Enum(RTypeArguments* outerTypeArgs, REnumDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElem::RMember_EnumElem(RTypeArguments* outerTypeArgs, REnumElemDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElemVar::RMember_EnumElemVar(RTypeArguments* outerTypeArgs, REnumElemVarDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_LambdaVar::RMember_LambdaVar(RTypeArguments* outerTypeArgs, RLambdaVarDecl* decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_TupleVar::RMember_TupleVar()
{

}

template<typename TRFuncDecl>
vector<DeclWithOuterTypeArgs<RFuncDecl>> GetItems(vector<DeclWithOuterTypeArgs<TRFuncDecl>>& items)
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member)
{
    return visit(overloaded {
        [](RMember_GlobalFuncs& member) { return GetItems(member.items); },
        [](RMember_ClassFuncs& member) { return GetItems(member.items); },
        [](RMember_StructFuncs& member) { return GetItems(member.items); },
        [](auto&&) { return vector<DeclWithOuterTypeArgs<RFuncDecl>>{}; },
    }, member);
}


RMember_TypeVar::RMember_TypeVar(size_t index)
    : index(index)
{
}

RMember_LocalVar::RMember_LocalVar(RType* type, const RName& name)
    : type(type), name(name)
{

}

RMember_ThisVar::RMember_ThisVar(RType* type)
    : type(type)
{
}

} // namespace Citron
