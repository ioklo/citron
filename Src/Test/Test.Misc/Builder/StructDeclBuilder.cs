using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using static Citron.Infra.Misc;
using Citron.Symbol;
using System.Diagnostics;

namespace Citron.Test.Misc
{
    public class StructDeclBuilder<TOuterBuilder>
    {
        // from constructors
        TOuterBuilder outerBuilder;
        SymbolFactory factory;
        ImmutableArray<ITypeDeclSymbol>.Builder typeDeclsBuilder;

        IHolder<IDeclSymbolNode> outerHolder;
        AccessModifier accessModifier;
        string name;        
        
        // allocated inside
        Holder<StructDeclSymbol> structDeclHolder;

        IHolder<StructSymbol?>? baseHolder;
        ImmutableArray<StructConstructorDeclSymbol>.Builder constructorDeclsBuilder;
        ImmutableArray<StructMemberFuncDeclSymbol>.Builder memberFuncDeclsBuilder;
        ImmutableArray<StructMemberVarDeclSymbol>.Builder memberVarDeclsBuilder;

        // components
        TypeParamBuilderComponent<StructDeclBuilder<TOuterBuilder>> typeParamComponent;

        internal StructDeclBuilder(
            TOuterBuilder outerBuilder, 
            SymbolFactory factory,
            ImmutableArray<ITypeDeclSymbol>.Builder typeDeclsBuilder, 
            IHolder<IDeclSymbolNode> outerHolder, AccessModifier accessModifier, string name)
        {
            this.outerBuilder = outerBuilder;
            this.factory = factory;
            this.typeDeclsBuilder = typeDeclsBuilder;

            this.outerHolder = outerHolder;
            this.accessModifier = accessModifier;
            this.name = name;

            this.structDeclHolder = new Holder<StructDeclSymbol>();
            this.baseHolder = null;
            this.constructorDeclsBuilder = ImmutableArray.CreateBuilder<StructConstructorDeclSymbol>();
            this.memberFuncDeclsBuilder = ImmutableArray.CreateBuilder<StructMemberFuncDeclSymbol>();
            this.memberVarDeclsBuilder = ImmutableArray.CreateBuilder<StructMemberVarDeclSymbol>();
            this.typeParamComponent = new TypeParamBuilderComponent<StructDeclBuilder<TOuterBuilder>>(this, structDeclHolder);
        }

        public StructDeclBuilder<TOuterBuilder> TypeParam(string name, out TypeVarDeclSymbol typeVarDecl)
            => typeParamComponent.TypeParam(name, out typeVarDecl);

        public StructDeclBuilder<TOuterBuilder> BaseHolder(out Holder<StructSymbol?> baseHolder)
        {
            Debug.Assert(this.baseHolder == null);
            baseHolder = new Holder<StructSymbol?>();
            this.baseHolder = baseHolder;

            return this;
        }

        public StructConstructorDeclBuilder<StructDeclBuilder<TOuterBuilder>> BeginConstructor(AccessModifier accessModifier, bool bTrivial)
        {
            return new StructConstructorDeclBuilder<StructDeclBuilder<TOuterBuilder>>(
                this, constructorDeclsBuilder, factory, structDeclHolder,
                accessModifier, bTrivial);
        }

        public StructMemberFuncDeclBuilder<StructDeclBuilder<TOuterBuilder>> BeginFunc(AccessModifier accessModifier, bool bStatic, Name name)
        {
            return new StructMemberFuncDeclBuilder<StructDeclBuilder<TOuterBuilder>>(
                this, memberFuncDeclsBuilder, factory, structDeclHolder,
                accessModifier, bStatic, name);
        }
        

        public StructDeclBuilder<TOuterBuilder> StaticMemberFunc(ITypeSymbol retType, string name, ITypeSymbol paramType, string paramName, out StructMemberFuncDeclSymbol funcDecl)
        {
            funcDecl = new StructMemberFuncDeclSymbol(
                structDeclHolder, AccessModifier.Public,
                bStatic: true,
                new Holder<FuncReturn>(new FuncReturn(false, retType)),
                new Name.Normal(name),
                typeParams: default,
                new Holder<ImmutableArray<FuncParameter>>(Arr<FuncParameter>(
                    new FuncParameter(FuncParameterKind.Default, paramType, new Name.Normal(paramName))
                )),                
                lambdaDecls: default);

            memberFuncDeclsBuilder.Add(funcDecl);

            return this;
        }

        public StructDeclBuilder<TOuterBuilder> Var(AccessModifier accessModifier, bool bStatic, IHolder<ITypeSymbol> typeHolder, Name name)
        {
            var varDecl = new StructMemberVarDeclSymbol(structDeclHolder, accessModifier, bStatic, typeHolder, name);
            memberVarDeclsBuilder.Add(varDecl);
            return this;
        }

        public TOuterBuilder EndStruct(out StructDeclSymbol structDecl)
        {
            var typeParams = typeParamComponent.MakeTypeParams();
            var baseHolder = this.baseHolder ?? new Holder<StructSymbol?>(null);

            structDecl = new StructDeclSymbol(
                    outerHolder,
                    accessModifier,
                    new Name.Normal(name),
                    typeParams,
                    baseHolder,
                    memberTypesExceptTypeVars: default,
                    memberFuncs: memberFuncDeclsBuilder.ToImmutable(),
                    memberVars: memberVarDeclsBuilder.ToImmutable(),
                    new Holder<ImmutableArray<StructConstructorDeclSymbol>>(default),
                    new Holder<StructConstructorDeclSymbol?>(null)
                );

            structDeclHolder.SetValue(structDecl);
            typeDeclsBuilder.Add(structDecl);
            return outerBuilder;
        }
    }
}