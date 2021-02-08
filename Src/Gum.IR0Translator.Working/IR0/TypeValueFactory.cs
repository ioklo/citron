using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0
{
    // Low-level ItemValue factory
    internal class TypeValueFactory
    {
        public TypeValueFactory()
        {
        }

        public TypeValue MakeGlobalType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo: return new StructTypeValue(moduleName, namespacePath, typeInfo, typeArgs);
            }

            throw new UnreachableCodeException();
        }

        public NormalTypeValue MakeMemberType(NormalTypeValue outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo:
                    return new StructTypeValue(this, outer, structInfo, typeArgs, typeEnv);
            }

            throw new UnreachableCodeException();
        }

        public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, M.MemberVarInfo info)
        {
            // class X<T> { T x; }
            // MakeMemberVarValue(X<int>, X<>.x)
            // X<>.x.Type == TV(0, 0)            

            // X<int>.GetTypeEnv() == [TV(0,0)=>int]

            var memberType = MakeTypeValue(info.Type);         // TV(0, 0)
            var typeEnv = outer.GetTypeEnv();                  // [TV(0,0) => int]
            var appliedMemberType = memberType.Apply(typeEnv);

            return new MemberVarValue(info.Name, info.IsStatic, appliedMemberType);
        }

        ImmutableArray<TypeValue> MakeTypeValues(ImmutableArray<M.Type> mtypes)
        {
            var builder = ImmutableArray.CreateBuilder<TypeValue>();
            foreach (var mtype in mtypes)
            {
                var type = MakeTypeValue(mtype);
                builder.Add(type);
            }

            return builder.ToImmutable();
        }

        public TypeValue MakeTypeValue(M.Type mtype)
        {
            switch (mtype)
            {
                case M.TypeVarType typeVar:
                    return new TypeVarTypeValue(typeVar.Depth, typeVar.Index, typeVar.Name);

                case M.ExternalType externalType:
                    {   
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(externalType.TypeArgs);
                        return MakeGlobalType(externalType.ModuleName, externalType.NamespacePath, externalType.Name, typeArgs);
                    }

                case M.MemberType memberType:
                    {
                        var outerType = ToTypeValue(memberType.Outer) as NormalTypeValue;
                        Debug.Assert(outerType != null);

                        var typeArgs = MakeTypeValues(memberType.TypeArgs);

                        var memberTypeValue = outerType.GetMemberType(memberType.Name, typeArgs);
                        Debug.Assert(memberTypeValue != null);

                        return memberTypeValue;
                    }

                case VoidType _:
                    return VoidTypeValue.Instance;

                default:
                    throw new UnreachableCodeException();
            }
        }

        public TypeVarTypeValue MakeTypeVar(int depth, int index, string name)
        {
            return new TypeVarTypeValue(depth, index, name);
        }        

        // internal
        public FuncValue MakeMemberFunc(TypeValue outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            return new FuncValue(outer, funcInfo, typeArgs);
        }

        // global
        public FuncValue MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo func, ImmutableArray<TypeValue> typeArgs)
        {
            return new FuncValue(moduleName, namespacePath, func, typeArgs);
        }
    }
}
