using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;
using Gum.Infra;

namespace Gum.Analysis
{
    // ItemPath를 다시 만들어 봅시다. 거의 IR0와 비슷하게 나올겁니다
    public abstract record ItemPath
    {
        
    }

    public record RootItemPath(M.ModuleName ModuleName) : ItemPath; // 최상위
    public record NestedItemPath(ItemPath Outer, M.Name Name, int TypeParamCount, M.ParamTypes ParamTypes): ItemPath;

    public static class ItemPathExtensions
    {
        public static ItemPath Child(this ItemPath outer, M.Name name, int typeParamCount = 0, M.ParamTypes paramTypes = default)
        {
            return new NestedItemPath(outer, name, typeParamCount, paramTypes);
        }

        public static ItemPath ToItemPath(this M.Type type)
        {   
            switch (type)
            {
                case M.GlobalType globalType:
                    ItemPath curPath = new RootItemPath(globalType.ModuleName);

                    foreach (var ns in globalType.NamespacePath.Entries)
                        curPath = curPath.Child(ns);

                    return curPath.Child(globalType.Name, globalType.TypeArgs.Length);

                case M.MemberType memberType:
                    return ToItemPath(memberType.Outer).Child(memberType.Name, memberType.TypeArgs.Length);

                case M.TypeVarType:
                case M.VoidType:
                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}