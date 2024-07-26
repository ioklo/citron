#pragma once
#include "IR0Config.h"

#include <unordered_map>

#include "RNames.h"
#include "RGlobalFuncDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class RNamespaceDecl;

class RNamespaceDeclContainerComponent
{
    std::vector<std::shared_ptr<RNamespaceDecl>> namespaceDecls; // preserve order
    std::unordered_map<std::string, std::shared_ptr<RNamespaceDecl>> namespaceDict;

public:
    RNamespaceDeclContainerComponent();
    RTypeDecl* GetType(const RName& name);

    IR0_API void AddNamespace(std::shared_ptr<RNamespaceDecl> _namespace);
    IR0_API std::shared_ptr<RNamespaceDecl> GetNamespace(const std::string& name);

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
