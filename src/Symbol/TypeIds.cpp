module Citron.Identifiers:TypeIds;

namespace Citron {

VoidTypeId voidTypeId;
TypeId boolTypeId = SymbolTypeId(false, boolSymbolId.Copy());
TypeId intTypeId = SymbolTypeId(false, intSymbolId.Copy());
TypeId stringTypeId = SymbolTypeId(false, stringSymbolId.Copy());

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
