using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // Low-level ItemValue factory
    internal class TypeValueFactory
    {
        TypeInfoRepository typeInfoRepo;

        public TypeValueFactory(TypeInfoRepository typeInfoRepo)
        {
            this.typeInfoRepo = typeInfoRepo;
        }

        public TypeValue MakeGlobalType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo: return new StructTypeValue(this, moduleName, namespacePath, structInfo, typeArgs);
            }

            throw new UnreachableCodeException();
        }

        public NormalTypeValue MakeMemberType(TypeValue outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo:
                    return new StructTypeValue(this, outer, structInfo, typeArgs);
            }

            throw new UnreachableCodeException();
        }

        public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, M.MemberVarInfo info)
        {
            return new MemberVarValue(this, outer, info);
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

                case M.GlobalType externalType:
                    {
                        // typeInfo를 가져와야 한다
                        var typeInfo = typeInfoRepo.GetType(externalType.ModuleName, externalType.NamespacePath, externalType.Name, externalType.TypeArgs.Length);
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(externalType.TypeArgs);
                        return MakeGlobalType(externalType.ModuleName, externalType.NamespacePath, typeInfo, typeArgs);
                    }

                case M.MemberType memberType:
                    {
                        var outerType = MakeTypeValue(memberType.Outer);
                        var typeArgs = MakeTypeValues(memberType.TypeArgs);

                        var memberTypeValue = outerType.GetMemberType(memberType.Name, typeArgs);
                        Debug.Assert(memberTypeValue != null);

                        return memberTypeValue;
                    }

                case M.VoidType _:
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
