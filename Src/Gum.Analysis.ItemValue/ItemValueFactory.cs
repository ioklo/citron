using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.Infra.Misc;
using static Gum.CompileTime.ItemPathExtensions;

using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Analysis;

namespace Gum.Analysis
{
    // Low-level ItemValue factory
    public class ItemValueFactory
    {
        TypeInfoRepository typeInfoRepo;
        RItemFactory ritemFactory;

        public TypeSymbol Void { get; }
        public TypeSymbol Bool { get; }
        public TypeSymbol Int { get; }        
        public TypeSymbol String { get; }

        // temporary
        record DummyStructInfo : IModuleStructDecl
        {
            IModuleNamespaceInfo outer;
            string name;

            public DummyStructInfo(IModuleNamespaceInfo outer, string name)
            {
                this.outer = outer;
                this.name = name;

                outer.AddItem(this);
            }

            public IStructTypeValue? GetBaseStruct() => null;
            public ImmutableArray<IModuleConstructorDecl> GetConstructors() => default;
            public M.ItemPathEntry GetEntry() => new M.ItemPathEntry(new M.Name.Normal(Name));
            public IModuleFuncDecl? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes) => null;
            public ImmutableArray<IModuleFuncDecl> GetFuncs(M.Name name, int minTypeParamCount) => default;
            public ImmutableArray<IModuleFuncDecl> GetMemberFuncs() => default;
            public ImmutableArray<IModuleTypeDecl> GetMemberTypes() => default;
            public ImmutableArray<IModuleMemberVarInfo> GetMemberVars() => default;
            public IModuleNamespaceInfo? GetNamespace(M.Name name) => null;
            public IModuleConstructorDecl? GetTrivialConstructor() => null;
            public IModuleTypeDecl? GetType(M.Name name, int typeParamCount) => null;
        }

        record DummySystemNamespaceInfo : IModuleNamespaceInfo
        {
            record DummyRuntimeModuleInfo : IModuleDecl
            {
                DummySystemNamespaceInfo ns;

                public DummyRuntimeModuleInfo(DummySystemNamespaceInfo ns)
                {
                    this.ns = ns;
                }
                
                M.ItemPathEntry IModuleItemDecl.GetEntry() => new M.ItemPathEntry(new M.Name.Normal("System.Runtime"));
                IModuleFuncDecl? IModuleItemDecl.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes) => null;
                ImmutableArray<IModuleFuncDecl> IModuleItemDecl.GetFuncs(M.Name name, int minTypeParamCount) => default;
                IModuleItemDecl? IModuleItemDecl.GetOuter() => null;
                IModuleTypeDecl? IModuleItemDecl.GetType(M.Name name, int typeParamCount) => null;

                IModuleNamespaceInfo? IModuleItemDecl.GetNamespace(M.Name name)
                {
                    if (name.Equals(new M.Name.Normal("System")))
                        return ns;

                    return null;
                }
            }

            DummyRuntimeModuleInfo outer;

            public DummySystemNamespaceInfo()
            {
                outer = new DummyRuntimeModuleInfo(this);
            }

            public M.ItemPathEntry GetEntry() => new M.ItemPathEntry(new M.Name.Normal("System"));
            public IModuleFuncDecl? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes) => null;
            public ImmutableArray<IModuleFuncDecl> GetFuncs(M.Name name, int minTypeParamCount) => default;
            public IModuleNamespaceInfo? GetNamespace(M.Name name) => null;
            public IModuleTypeDecl? GetType(M.Name name, int typeParamCount);
        }

        public ItemValueFactory(TypeInfoRepository typeInfoRepo, RItemFactory ritemFactory)
        {
            this.typeInfoRepo = typeInfoRepo;
            this.ritemFactory = ritemFactory;

            Void = VoidTypeValue.Instance;

            // outer를 우선 만들고, 자식들은 outer에 본인을 등록할 책임이 있다            
            var systemNS = new DummySystemNamespaceInfo();

            Bool = MakeDummyStructTypeValue(systemNS, "Boolean");
            Int = MakeDummyStructTypeValue(systemNS, "Int32");

            // TODO: 일단 Struct로 만든다
            String = MakeDummyStructTypeValue(systemNS, "String");
        }

        // typeInfo의 outer가 ItemValue류가 아닐때 가능하다
        TypeSymbol MakeDummyStructTypeValue(DummySystemNamespaceInfo namespaceInfo, string name)
        {
            var structInfo = new DummyStructInfo(namespaceInfo, name);
            return MakeTypeValue(new RootItemValueOuter(namespaceInfo), structInfo, default);
        }

        public TypeSymbol MakeTypeValue(TypeSymbol outer, IModuleTypeDecl typeInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return MakeTypeValue(new NestedItemValueOuter(outer), typeInfo, typeArgs);
        }

        public NormalTypeValue MakeTypeValue(ItemValueOuter outer, IModuleTypeDecl typeInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            switch (typeInfo)
            {
                case IModuleStructDecl structInfo:
                    return MakeStructValue(outer, structInfo, typeArgs);

                case IModuleClassDecl classInfo:
                    return MakeClassSymbol(outer, classInfo, typeArgs);

                case IModuleEnumDecl enumInfo:
                    return new EnumSymbol(this, outer, enumInfo, typeArgs);

                case IModuleEnumElemDecl enumElemInfo:
                    Debug.Assert(outer is NestedItemValueOuter);
                    Debug.Assert(((NestedItemValueOuter)outer).ItemValue is EnumSymbol);
                    return new EnumElemSymbol(ritemFactory, this, (EnumTypeValue)((NestedItemValueOuter)outer).ItemValue, enumElemInfo);
            }

            throw new UnreachableCodeException();
        }        

        public MemberVarValue MakeMemberVarValue(NormalTypeValue outer, IModuleMemberVarInfo info)
        {
            return new MemberVarValue(this, outer, info);
        }        
        
        ImmutableArray<TypeSymbol> MakeTypeValues(ImmutableArray<M.TypeId> mtypes)
        {
            var builder = ImmutableArray.CreateBuilder<TypeSymbol>();
            foreach (var mtype in mtypes)
            {
                var type = MakeTypeValueByMType(mtype);
                builder.Add(type);
            }

            return builder.ToImmutable();
        }

        public EnumSymbol MakeEnumTypeValue(ItemValueOuter outer, IModuleEnumDecl enumInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new EnumSymbol(this, outer, enumInfo, typeArgs);
        }

        public EnumElemSymbol MakeEnumElemTypeValue(EnumSymbol outer, IModuleEnumElemDecl elemInfo)
        {
            return new EnumElemSymbol(ritemFactory, this, outer, elemInfo);
        }

        // Gum.Analysis.ModuleInfo.Abstraction에서
        // [Gum.Analysis.TypeExpEvaluator] TypeExpInfo를 참조해야 한다
        // TypeExpInfo는 TypeExpEvaluator를 수행한 결과이다
        // [Gum.Analysis.TypeExpEvaluator]에서 [Gum.Analysis.TypeExpInfo]를 분리해 보자
        public TypeSymbol MakeTypeValue(TypeExpInfo typeExpInfo)
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
        
        // ItemValueOuter를 만듭니다
        ItemValueOuter MakeItemValueOuter(M.ItemPath itemPath)
        {
            if (itemPath.Outer != null)
            {
                var outerItemValueOuter = MakeItemValueOuter(itemPath.Outer);
            }

            if (itemPath.Outer == null)
            {
                Debug.Assert(itemPath.TypeParamCount == 0 && itemPath.ParamTypes.IsEmpty);
                return new RootItemValueOuter(itemPath.Name);
            }

            var outer = MakeItemValueOuter(itemPath.Outer);
            if( outer is RootItemValueOuter rootItemValueOuter)
            {

            }
        }
        
        public TypeSymbol MakeTypeValueByMType(M.TypeId mtype)
        {
            switch (mtype)
            {
                case M.TypeVarTypeId typeVar:
                    return MakeTypeVar(typeVar.Index);

                case M.RootTypeId rootType:
                    {
                        var typeInfo = typeInfoRepo.GetType(rootType.Outer.Child(rootType.Name, rootType.TypeArgs.Length));
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(rootType.TypeArgs);
                        var outer = MakeItemValueOuter(rootType.Outer);

                        return MakeTypeValue(outer, typeInfo, typeArgs);
                    }

                case M.NormalTypeId normalType:
                    {
                        normalType.Path

                        // typeInfo를 가져와야 한다
                        var typeInfo = typeInfoRepo.GetType(rootType.ModuleName, rootType.Name, rootType.TypeArgs.Length);
                        Debug.Assert(typeInfo != null);

                        var typeArgs = MakeTypeValues(rootType.TypeArgs);
                        return MakeTypeValue(new RootItemValueOuter(rootType.ModuleName), typeInfo, typeArgs);
                    }

                case M.NestedType nestedType:
                    {
                        var outerType = MakeTypeValueByMType(nestedType.Outer);
                        var typeArgs = MakeTypeValues(nestedType.TypeArgs);

                        var memberTypeValue = outerType.GetMemberType(nestedType.Name, typeArgs);
                        Debug.Assert(memberTypeValue != null);

                        return memberTypeValue;
                    }

                case M.VoidTypeId:
                    return VoidTypeValue.Instance;

                case M.NullableTypeId nullableType:
                    var innerTypeValue = MakeTypeValueByMType(nullableType.InnerType);
                    return MakeNullableTypeValue(innerTypeValue);

                default:
                    throw new UnreachableCodeException();
            }
        }

        public FuncValue MakeFunc(ItemValueOuter outer, IModuleFuncDecl funcInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new FuncValue(this, outer, funcInfo, typeArgs);
        }

        public TypeVarTypeValue MakeTypeVar(int index)
        {
            return new TypeVarTypeValue(ritemFactory, index);
        }        
        
        public FuncValue MakeMemberFunc(TypeSymbol outer, IModuleFuncDecl funcInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            var itemValueOuter = new NestedItemValueOuter(outer);
            return new FuncValue(this, itemValueOuter, funcInfo, typeArgs);
        }

        // global
        public FuncValue MakeGlobalFunc(M.ItemPath outerPath, IModuleFuncDecl funcInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            var itemValueOuter = MakeItemValueOuter(outerPath);
            return new FuncValue(this, itemValueOuter, funcInfo, typeArgs);
        }

        public SeqTypeValue MakeSeqType(R.Path.Nested seq, TypeSymbol yieldType)
        {
            return new SeqTypeValue(ritemFactory, seq, yieldType);
        }

        public LambdaTypeValue MakeLambdaType(R.Path.Nested lambda, TypeSymbol retType, ImmutableArray<ParamInfo> paramInfos)
        {
            return new LambdaTypeValue(ritemFactory, lambda, retType, paramInfos);
        }

        public VarTypeValue MakeVarTypeValue()
        {
            return VarTypeValue.Instance;
        }

        public TupleTypeValue MakeTupleType(ImmutableArray<(TypeSymbol Type, string? Name)> elems)
        {
            return new TupleTypeValue(ritemFactory, elems);
        }

        public RuntimeListTypeValue MakeListType(TypeSymbol elemType)
        {
            return new RuntimeListTypeValue(this, elemType);
        }

        public ConstructorValue MakeConstructorValue(NormalTypeValue outer, IModuleConstructorDecl info)
        {
            return new ConstructorValue(this, outer, info);
        }

        public StructSymbol MakeStructValue(ItemValueOuter outer, IModuleStructDecl structInfo, ImmutableArray<ITypeSymbolNode> typeArgs)
        {
            return new StructSymbol(this, ritemFactory, outer, structInfo, typeArgs);
        }        

        public NullableTypeValue MakeNullableTypeValue(TypeSymbol innerTypeValue)
        {
            return new NullableTypeValue(this, innerTypeValue);
        }
    }
}
