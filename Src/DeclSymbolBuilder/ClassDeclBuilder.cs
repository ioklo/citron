using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test
{
    public class ClassDeclBuilder<TOuterBuilder>
    {
        internal delegate ClassDeclSymbol OnFinish(ImmutableArray<Name> typeParams, ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncDecls);

        TOuterBuilder outerBuilder;
        OnFinish onFinish;

        ClassDeclSymbol classDecl;
        ImmutableArray<ClassMemberFuncDeclSymbol>.Builder memberFuncDeclsBuilder;

        TypeParamBuilderComponent<ClassDeclBuilder<TOuterBuilder>> typeParamComponent;

        internal ClassDeclBuilder(TOuterBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.classDecl = new ClassDeclSymbol();
            this.memberFuncDeclsBuilder = ImmutableArray.CreateBuilder<ClassMemberFuncDeclSymbol>();
            this.typeParamComponent = new TypeParamBuilderComponent<ClassDeclBuilder<TOuterBuilder>>(this, classDeclHolder);
        }

        public ClassDeclBuilder<TOuterBuilder> TypeParam(string name, out Name typeVarDecl)
            => typeParamComponent.TypeParam(name, out typeVarDecl);

        public TOuterBuilder EndClass(out ClassDeclSymbol classDecl)
        {
            classDecl = onFinish.Invoke(typeParamComponent.MakeTypeParams(), memberFuncDeclsBuilder.ToImmutable());
            classDeclHolder.SetValue(classDecl);
            return outerBuilder;
        }

        public ClassDeclBuilder<TOuterBuilder> StaticMemberFunc(ITypeSymbol retType, string name, ITypeSymbol paramType, string paramName, out ClassMemberFuncDeclSymbol funcDecl)
        {
            funcDecl = new ClassMemberFuncDeclSymbol(classDecl, Accessor.Public,                
                new Name.Normal(name),
                typeParams: default,
                bStatic: true);

            funcDecl.InitFuncReturnAndParams(
                new FuncReturn(false, retType),
                Arr<FuncParameter>(
                    new FuncParameter(FuncParameterKind.Default, paramType, new Name.Normal(paramName))
                )
            );

            classDecl.AddFunc(funcDecl);
            memberFuncDeclsBuilder.Add(funcDecl);

            return this;
        }
    }
}