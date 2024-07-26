#include "RTypeId.h"

namespace Citron {

RVoidTypeId voidTypeId;
std::shared_ptr<RTypeId> boolTypeId = std::make_shared<RSymbolTypeId>(false, boolSymbolId.Copy());
std::shared_ptr<RTypeId> intTypeId = std::make_shared<RSymbolTypeId>(false, intSymbolId.Copy());
std::shared_ptr<RTypeId> stringTypeId = std::make_shared<RSymbolTypeId>(false, stringSymbolId.Copy());

IMPLEMENT_DEFAULTS(RVoidTypeId)
IMPLEMENT_DEFAULTS(RTupleTypeId)
IMPLEMENT_DEFAULTS(RLambdaTypeId)
IMPLEMENT_DEFAULTS(RSymbolTypeId)
RSymbolTypeId::RSymbolTypeId(bool bLocal, RSymbolId&& symbolId)
    : bLocal(bLocal), symbolId(std::move(symbolId))
{
}

IMPLEMENT_DEFAULTS(RNullableTypeId)
IMPLEMENT_DEFAULTS(RFuncTypeId)
IMPLEMENT_DEFAULTS(RLocalPtrTypeId)
IMPLEMENT_DEFAULTS(RBoxPtrTypeId)


//public static class TypeIdExtensions
//{
//    public static bool IsList(this TypeId typeId, [NotNullWhen(returnValue:true)] out TypeId ? itemId)
//    {
//        var symbolId = (typeId as SymbolTypeId) ? .SymbolId;
//        if (symbolId == null)
//        {
//            itemId = null;
//            return false;
//        }
//
//        var declSymbolId = symbolId.GetDeclSymbolId();
//        if (declSymbolId.Equals(DeclSymbolIds.List))
//        {
//            Debug.Assert(symbolId.Path != null);
//
//            itemId = symbolId.Path.TypeArgs[0];
//            return true;
//        }
//
//        itemId = null;
//        return false;
//    }
//
//    // ISeq 타입인지
//    public static bool IsSeq(this TypeId typeId)
//    {
//        var symbolId = (typeId as SymbolTypeId) ? .SymbolId;
//        if (symbolId == null) return false;
//
//        var declSymbolId = symbolId.GetDeclSymbolId();
//        return declSymbolId.Equals(DeclSymbolIds.Seq);
//    }
//}



}
