using Citron.Collections;
using Citron.Module;
using Citron.Infra;
using System;
using static Citron.Infra.Misc;
using Citron.Symbol;

namespace Citron.Test.Misc
{
    public class ClassDeclBuilder<TOuterBuilder>
    {
        internal delegate ClassDeclSymbol OnFinish(ImmutableArray<ClassMemberFuncDeclSymbol> memberFuncDecls);

        TOuterBuilder outerBuilder;
        OnFinish onFinish;

        Holder<ClassDeclSymbol> classDeclHolder;
        ImmutableArray<ClassMemberFuncDeclSymbol>.Builder memberFuncDeclsBuilder;

        internal ClassDeclBuilder(TOuterBuilder outerBuilder, OnFinish onFinish)
        {
            this.outerBuilder = outerBuilder;
            this.onFinish = onFinish;

            this.classDeclHolder = new Holder<ClassDeclSymbol>();
            this.memberFuncDeclsBuilder = ImmutableArray.CreateBuilder<ClassMemberFuncDeclSymbol>();
        }

        public TOuterBuilder EndClass(out ClassDeclSymbol classDecl)
        {
            classDecl = onFinish.Invoke(memberFuncDeclsBuilder.ToImmutable());
            classDeclHolder.SetValue(classDecl);
            return outerBuilder;
        }

        public ClassDeclBuilder<TOuterBuilder> StaticMemberFunc(ITypeSymbol retType, string name, ITypeSymbol paramType, string paramName, out ClassMemberFuncDeclSymbol funcDecl)
        {
            funcDecl = new ClassMemberFuncDeclSymbol(classDeclHolder, AccessModifier.Public,
                new Holder<FuncReturn>(new FuncReturn(false, retType)),
                new Name.Normal(name),
                typeParams: default,
                new Holder<ImmutableArray<FuncParameter>>(Arr<FuncParameter>(
                    new FuncParameter(FuncParameterKind.Default, paramType, new Name.Normal(paramName))
                )),
                bStatic: true,
                lambdaDecls: default);

            memberFuncDeclsBuilder.Add(funcDecl);

            return this;
        }
    }
}