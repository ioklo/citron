module Citron.Symbols:MTypeIds;

namespace Citron {

MVoidTypeId voidTypeId;
MTypeId boolTypeId = MSymbolTypeId(false, boolSymbolId.Copy());
MTypeId intTypeId = MSymbolTypeId(false, intSymbolId.Copy());
MTypeId stringTypeId = MSymbolTypeId(false, stringSymbolId.Copy());

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
