export module Citron.Symbols:MNamespaceDeclContainerComponent;

import <string>;
import <vector>;
import <unordered_map>;

import :MGlobalFuncDecl;
import :MTypeDecl;

namespace Citron {

class MNamespaceDecl;

class MNamespaceDeclContainerComponent
{
    std::vector<MNamespaceDecl> namespaceDecls;    // storage
    std::unordered_map<std::string, int> namespaceDict; // indirect

public:
    MNamespaceDeclContainerComponent();
    MTypeDecl* GetType(const MName& name);

//    public ITypeDeclSymbol ? GetType(Name name, int typeParamCount)
//        = > typeComp.GetType(name, typeParamCount);
//
//    public void AddType(ITypeDeclSymbol decl)
//        = > typeComp.AddType(decl);
//
//    public IEnumerable<GlobalFuncDeclSymbol> GetFuncs(Name name, int minTypeParamCount)
//        = > funcComp.GetFuncs(name, minTypeParamCount);
//
//    public GlobalFuncDeclSymbol ? GetFunc(Name name, int typeParamCount, ImmutableArray<FuncParamId> paramIds)
//        = > funcComp.GetFunc(name, typeParamCount, paramIds);
//
//    public void AddFunc(GlobalFuncDeclSymbol decl)
//        = > funcComp.AddFunc(decl);
//
//    public void AddNamespace(NamespaceDeclSymbol decl)
//    {
//        namespaceDecls.Add(decl);
//        namespaceDict.Add(decl.GetName(), decl);
//    }
//
//    public NamespaceDeclSymbol ? GetNamespace(Name name)
//    {
//        return namespaceDict.GetValueOrDefault(name);
//    }
//
//    public IEnumerable<IDeclSymbolNode> GetMemberDeclNodes()
//    {
//        return namespaceDict.Values
//            .Concat<IDeclSymbolNode>(namespaceDecls)
//            .Concat(typeComp.GetEnumerable())
//            .Concat(funcComp.GetEnumerable());
//    }
//
//    bool ICyclicEqualityComparableStruct<TopLevelDeclSymbolComponent>.CyclicEquals(ref TopLevelDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
//    {
//        if (!namespaceDecls.CyclicEqualsClassItem(other.namespaceDecls, ref context))
//            return false;
//
//        if (!typeComp.CyclicEquals(ref other.typeComp, ref context))
//            return false;
//
//        if (!funcComp.CyclicEquals(ref other.funcComp, ref context))
//            return false;
//
//        return true;
//    }
//
//    void ISerializable.DoSerialize(ref SerializeContext context)
//    {
//        context.SerializeRefList(nameof(namespaceDecls), namespaceDecls);
//        context.SerializeValueRef(nameof(typeComp), ref typeComp);
//        context.SerializeValueRef(nameof(funcComp), ref funcComp);
//    }
//}

};


}
