﻿using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // Low-level ItemValue factory
    class ItemValueFactory
    {   
        TypeInfoRepository typeInfoRepo;
        RItemFactory ritemFactory;

        public TypeValue Void { get; }
        public TypeValue Bool { get; }
        public TypeValue Int { get; }        
        public TypeValue String { get; }
        public TypeValue List(TypeValue typeArg)
        {
            throw new NotImplementedException();
        }

        public ItemValueFactory(TypeInfoRepository typeInfoRepo, RItemFactory ritemFactory)
        {
            M.TypeInfo MakeEmptyStructInfo(M.Name name) => new M.StructInfo(name, default, null, default, default, default, default);

            this.typeInfoRepo = typeInfoRepo;
            this.ritemFactory = ritemFactory;

            Void = VoidTypeValue.Instance;
            Bool = MakeTypeValue("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("Boolean"), default);
            Int = MakeTypeValue("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("Int32"), default);

            // TODO: 일단 Struct로 만든다
            String = MakeTypeValue("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("String"), default);
        }

        public TypeValue MakeTypeValue(M.ModuleName moduleName, M.NamespacePath namespacePath, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            return MakeTypeValue(new RootItemValueOuter(moduleName, namespacePath), typeInfo, typeArgs);
        }

        public NormalTypeValue MakeTypeValue(ItemValueOuter outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo:
                    return new StructTypeValue(this, ritemFactory, outer, structInfo, typeArgs);
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
                    return MakeTypeVar(typeVar.Depth, typeVar.Index);

                case M.GlobalType externalType:
                    {
                        // typeInfo를 가져와야 한다
                        var typeInfo = typeInfoRepo.GetType(externalType.ModuleName, externalType.NamespacePath, externalType.Name, externalType.TypeArgs.Length);
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(externalType.TypeArgs);
                        return MakeTypeValue(new RootItemValueOuter(externalType.ModuleName, externalType.NamespacePath), typeInfo, typeArgs);
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

        public TypeVarTypeValue MakeTypeVar(int depth, int index)
        {
            return new TypeVarTypeValue(ritemFactory, depth, index);
        }        
        
        public FuncValue MakeMemberFunc(TypeValue outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            var itemValueOuter = new NestedItemValueOuter(outer);
            return new FuncValue(this, itemValueOuter, funcInfo, typeArgs);
        }

        // global
        public FuncValue MakeGlobalFunc(M.ModuleName moduleName, M.NamespacePath namespacePath, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            var itemValueOuter = new RootItemValueOuter(moduleName, namespacePath);
            return new FuncValue(this, itemValueOuter, funcInfo, typeArgs);
        }

        public LambdaTypeValue MakeLambdaType(R.Path.Nested lambda, TypeValue retType, ImmutableArray<TypeValue> paramTypes)
        {
            return new LambdaTypeValue(ritemFactory, lambda, retType, paramTypes);
        }

        public VarTypeValue MakeVarTypeValue()
        {
            return VarTypeValue.Instance;
        }
    }
}
