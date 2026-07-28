#include "SmTypeUtil.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/REnumDecl.h"
#include "RSymbol/REnumElemDecl.h"
#include "SmDeclRes.h"

using namespace std;

namespace Citron {

namespace {

template<typename TRDecl>
concept CanHandleDeclType = requires(TRDecl* type, InRef<RName> name) {
    { type->decl->GetMember(name) } -> std::same_as<std::optional<RMember>>;
};

struct GetMemberVisitor
{
    using ResultType = optional<SmDeclRes>;

    template<typename TRDecl> requires CanHandleDeclType<TRDecl>
    ResultType HandleDeclType(TRDecl* type, InRef<RName> name)
    {
        auto o_member = type->decl->GetMember(name);
        if (!o_member) return nullopt;

        return ToSmDeclRes(type->typeArgs, *o_member);
    }

    ResultType Visit(RType_Nullable* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_NullableInplace* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_TypeVar* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Void* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Primitive* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Tuple* type, InRef<RName> name) { throw NotImplementedException(); }
    ResultType Visit(RType_Func* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Ptr* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Shared* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Box* type, InRef<RName> name) { return nullopt; }
    ResultType Visit(RType_Class* type, InRef<RName> name) { return HandleDeclType(type, name); }    
    ResultType Visit(RType_Struct* type, InRef<RName> name) { return HandleDeclType(type, name); }
    ResultType Visit(RType_Enum* type, InRef<RName> name) { return HandleDeclType(type, name); }
    ResultType Visit(RType_EnumElem* type, InRef<RName> name) { return HandleDeclType(type, name); }
    ResultType Visit(RType_Interface* type, InRef<RName> name) { throw NotImplementedException(); }
    ResultType Visit(RType_Lambda* type, InRef<RName> name) { throw NotImplementedException(); }
};
} // namespace

optional<SmDeclRes> GetMember(RType* type, InRef<RName> name)
{   
    return Accept(GetMemberVisitor{}, type, name);
}

} // namespace Citron