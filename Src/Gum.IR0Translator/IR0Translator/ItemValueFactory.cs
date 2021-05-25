using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.Infra.Misc;

using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // Low-level ItemValue factory
    class ItemValueFactory : IPure
    {   
        TypeInfoRepository typeInfoRepo;
        RItemFactory ritemFactory;

        M.StructInfo listInfo;

        public TypeValue Void { get; }
        public TypeValue Bool { get; }
        public TypeValue Int { get; }        
        public TypeValue String { get; }
        public TypeValue List(TypeValue typeArg)
        {
            return MakeTypeValue("System.Runtime", new M.NamespacePath("System"), listInfo, Arr(typeArg));
        }

        public void EnsurePure()
        {
            Infra.Misc.EnsurePure(typeInfoRepo);
            Infra.Misc.EnsurePure(ritemFactory);
        }

        public ItemValueFactory(TypeInfoRepository typeInfoRepo, RItemFactory ritemFactory)
        {
            M.StructInfo MakeEmptyStructInfo(M.Name name) => new M.StructInfo(name, default, null, default, default, default, default);

            this.typeInfoRepo = typeInfoRepo;
            this.ritemFactory = ritemFactory;

            listInfo = MakeEmptyStructInfo("List");

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

        public TypeValue MakeTypeValue(TypeValue outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            return MakeTypeValue(new NestedItemValueOuter(outer), typeInfo, typeArgs);
        }

        public NormalTypeValue MakeTypeValue(ItemValueOuter outer, M.TypeInfo typeInfo, ImmutableArray<TypeValue> typeArgs)
        {
            switch (typeInfo)
            {
                case M.StructInfo structInfo:
                    return new StructTypeValue(this, ritemFactory, outer, structInfo, typeArgs);

                case M.EnumInfo enumInfo:
                    Debug.Assert(typeArgs.IsEmpty);
                    return new EnumTypeValue(this, outer, enumInfo, default);

                case M.EnumElemInfo enumElemInfo:
                    Debug.Assert(outer is NestedItemValueOuter);
                    Debug.Assert(((NestedItemValueOuter)outer).ItemValue is EnumTypeValue);
                    return new EnumElemTypeValue(ritemFactory, this, (EnumTypeValue)((NestedItemValueOuter)outer).ItemValue, enumElemInfo);
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
                var type = MakeTypeValueByMType(mtype);
                builder.Add(type);
            }

            return builder.ToImmutable();
        }

        public EnumTypeValue MakeEnumTypeValue(ItemValueOuter outer, M.EnumInfo enumInfo, ImmutableArray<TypeValue> typeArgs)
        {
            return new EnumTypeValue(this, outer, enumInfo, typeArgs);
        }

        public EnumElemTypeValue MakeEnumElemTypeValue(EnumTypeValue outer, M.EnumElemInfo elemInfo)
        {
            return new EnumElemTypeValue(ritemFactory, this, outer, elemInfo);
        }

        public TypeValue MakeTypeValue(TypeExpInfo typeExpInfo)
        {
            switch (typeExpInfo)
            {
                case MTypeTypeExpInfo mtypeInfo:
                    return MakeTypeValueByMType(mtypeInfo.Type);

                case VarTypeExpInfo:
                    return MakeVarTypeValue();

                default:
                    throw new UnreachableCodeException();
            }
        }

        public TypeValue MakeTypeValueByMType(M.Type mtype)
        {
            switch (mtype)
            {
                case M.TypeVarType typeVar:
                    return MakeTypeVar(typeVar.Index);                

                case M.GlobalType globalType:
                    {
                        // typeInfo를 가져와야 한다
                        var typeInfo = typeInfoRepo.GetType(globalType.ModuleName, globalType.NamespacePath, globalType.Name, globalType.TypeArgs.Length);
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(globalType.TypeArgs);
                        return MakeTypeValue(new RootItemValueOuter(globalType.ModuleName, globalType.NamespacePath), typeInfo, typeArgs);
                    }

                case M.MemberType memberType:
                    {
                        var outerType = MakeTypeValueByMType(memberType.Outer);
                        var typeArgs = MakeTypeValues(memberType.TypeArgs);

                        var memberTypeValue = outerType.GetMemberType(memberType.Name, typeArgs);
                        Debug.Assert(memberTypeValue != null);

                        return memberTypeValue;
                    }

                case M.VoidType:
                    return VoidTypeValue.Instance;

                default:
                    throw new UnreachableCodeException();
            }
        }

        public FuncValue MakeFunc(ItemValueOuter outer, M.FuncInfo funcInfo, ImmutableArray<TypeValue> typeArgs)
        {
            return new FuncValue(this, outer, funcInfo, typeArgs);
        }

        public TypeVarTypeValue MakeTypeVar(int index)
        {
            return new TypeVarTypeValue(ritemFactory, index);
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

        public SeqTypeValue MakeSeqType(R.Path.Nested seq, TypeValue yieldType)
        {
            return new SeqTypeValue(ritemFactory, seq, yieldType);
        }

        public LambdaTypeValue MakeLambdaType(R.Path.Nested lambda, TypeValue retType, ImmutableArray<TypeValue> paramTypes)
        {
            return new LambdaTypeValue(ritemFactory, lambda, retType, paramTypes);
        }

        public VarTypeValue MakeVarTypeValue()
        {
            return VarTypeValue.Instance;
        }

        public TupleTypeValue MakeTupleType(ImmutableArray<(TypeValue Type, string? Name)> elems)
        {
            return new TupleTypeValue(ritemFactory, elems);
        }
    }
}
