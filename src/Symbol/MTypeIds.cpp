#include "MTypeIds.h"

namespace Citron {

MVoidTypeId voidTypeId;
MTypeId boolTypeId = MSymbolTypeId(false, boolSymbolId.Copy());
MTypeId intTypeId = MSymbolTypeId(false, intSymbolId.Copy());
MTypeId stringTypeId = MSymbolTypeId(false, stringSymbolId.Copy());

IMPLEMENT_DEFAULTS(MVoidTypeId)
IMPLEMENT_DEFAULTS(MTupleTypeId)
IMPLEMENT_DEFAULTS(MLambdaTypeId)
IMPLEMENT_DEFAULTS(MSymbolTypeId)
MSymbolTypeId::MSymbolTypeId(bool bLocal, MSymbolId&& symbolId)
    : bLocal(bLocal), symbolId(std::move(symbolId))
{
}

IMPLEMENT_DEFAULTS(MNullableTypeId)
IMPLEMENT_DEFAULTS(MFuncTypeId)
IMPLEMENT_DEFAULTS(MLocalPtrTypeId)
IMPLEMENT_DEFAULTS(MBoxPtrTypeId)


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
