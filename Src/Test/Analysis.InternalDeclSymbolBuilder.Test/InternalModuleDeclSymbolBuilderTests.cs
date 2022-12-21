using System.Diagnostics;
using Citron.Collections;

using Xunit;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;

using S = Citron.Syntax;
using M = Citron.Module;
using Citron.Test.Misc;
using System;
using System.IO;
using Citron.Analysis;
using Citron.Symbol;
using Citron.Infra;

namespace Citron.IR0Translator.Test
{    
    public class InternalModuleDeclSymbolBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        // public M.TypeId IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), NormalName("Int32"), default); }

        SymbolFactory factory;
        ModuleDeclSymbol runtimeModuleDecl;
        StructSymbol boolType, intType;
        ClassSymbol stringType;
        VoidSymbol voidType;

        M.Name.Normal NormalName(string text)
        {
            return new M.Name.Normal(text);
        }

        public InternalModuleDeclSymbolBuilderTests()
        {
            factory = new SymbolFactory();

            runtimeModuleDecl = new ModuleDeclBuilder(factory, NormalName("System.Runtime"))
                    .BeginNamespace("System")
                        .Struct("Boolean", out var boolDecl)
                        .Struct("Int32", out var intDecl)
                        .Class("String", out var stringDecl)
                        .BeginClass("List")
                            .TypeParam("TItem", out var _)
                        .EndClass(out var listTypeDecl)
                    .EndNamespace(out var systemNSDecl)
                    .Make();


            var runtimeModule = factory.MakeModule(runtimeModuleDecl);
            var systemNS = factory.MakeNamespace(runtimeModule, systemNSDecl);

            boolType = factory.MakeStruct(systemNS, boolDecl, default);
            intType = factory.MakeStruct(systemNS, intDecl, default);
            stringType = factory.MakeClass(systemNS, stringDecl, default);
            voidType = factory.MakeVoid();
        }

        ModuleDeclSymbol Build(M.Name moduleName, S.Script script)
        {   
            var referenceModules = Arr(runtimeModuleDecl);

            var logger = new TestLogger(true);
            TypeExpEvaluator.Evaluate(moduleName, script, referenceModules, logger);
            
            var result = InternalModuleDeclSymbolBuilder.Build(moduleName, script, factory, referenceModules);

            Assert.NotNull(result);
            return result!.Value.ModuleDecl;
        }

        [Fact]
        public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
        {
            // F<T>(T t)
            var paramTypes1 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Default, new M.TypeVarTypeId(0, "T"))));

            // F<U>(U u)
            var paramTypes2 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Default, new M.TypeVarTypeId(0, "U"))));

            Assert.Equal(paramTypes1, paramTypes2);
        }

        [Fact]
        public void Build_FuncDecl_RefTypes()
        {
            // ref T Func<T>(ref T t) { return ref t; }
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                null,
                isSequence: false,
                isRefReturn: true,
                retType: new S.IdTypeExp("T", default),
                name: "Func",
                typeParams: Arr(new S.TypeParam("T")),
                parameters: Arr(new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "t")),
                body: Arr<S.Stmt>(new S.ReturnStmt(new S.ReturnValueInfo(true, new S.IdentifierExp("t", default))))
            )));

            var moduleName = NormalName("TestModule");
            var result = Build(moduleName, script);
            Assert.NotNull(result);
            Debug.Assert(result != null);

            // var paramTypes = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Ref, new M.TypeVarTypeId(0, "T"))));
            var paramIds = Arr(new FuncParamId(M.FuncParameterKind.Ref, new TypeVarSymbolId(0)));
            var path = new DeclSymbolPath(null, NormalName("Func"), 1, paramIds);

            var resultDecl = result.GetDeclSymbol(path) as GlobalFuncDeclSymbol;
            Debug.Assert(resultDecl != null);

            // declSymbol을 얻어낼 수 있는 방법
            var _ = new ModuleDeclBuilder(factory, moduleName)
                .BeginGlobalFunc(M.Accessor.Private, NormalName("Func"), bInternal: true)
                    .TypeParam("T", out var typeVarDecl)
                    .FuncReturnHolder(out var funcRetHolder)
                    .FuncParametersHolder(paramIds, out var funcParamsHolder)
                .EndGlobalFunc(out var globalFuncDecl)
                .Make();

            var typeVar = factory.MakeTypeVar(typeVarDecl);

            funcRetHolder.SetValue(new FuncReturn(isRef: false, typeVar));
            funcParamsHolder.SetValue(Arr(new FuncParameter(M.FuncParameterKind.Ref, typeVar, NormalName("t"))));

            Assert.Equal(globalFuncDecl, resultDecl);
        }
        
        [Fact]
        public void Build_FuncDecl_ModuleInfoHasFuncInfo()
        {
            // void Func<T, U>(int x, params U y, ref T z)
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                null,
                isSequence: false,
                isRefReturn: false,
                SVoidTypeExp(),
                "Func",
                Arr(new S.TypeParam("T"), new S.TypeParam("U")),
                Arr(
                    new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x"),
                    new S.FuncParam(S.FuncParamKind.Params, new S.IdTypeExp("U", default), "y"),
                    new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "z")
                ),
                Arr<S.Stmt>()
            )));

            var result = Build(NormalName("TestModule"), script);

            var paramIds = Arr(
                new FuncParamId(M.FuncParameterKind.Default, SymbolId.Int),
                new FuncParamId(M.FuncParameterKind.Params, new TypeVarSymbolId(1)),
                new FuncParamId(M.FuncParameterKind.Ref, new TypeVarSymbolId(0))
            );

            var path = new DeclSymbolPath(
                null, 
                NormalName("Func"), 2, 
                paramIds
            );

            var resultDecl = result.GetDeclSymbol(path) as GlobalFuncDeclSymbol;
            Debug.Assert(resultDecl != null);

            // expected 만들기
            var _ = new ModuleDeclBuilder(factory, NormalName("TestModule"))
                .BeginGlobalFunc(M.Accessor.Private, NormalName("Func"), true)
                    .TypeParam("T", out var tDecl)
                    .TypeParam("U", out var uDecl)
                    .FuncReturn(isRef: false, voidType)
                    .FuncParametersHolder(paramIds, out var paramsHolder)
                .EndGlobalFunc(out var expectedGlobalFuncDecl)
                .Make();

            var t = factory.MakeTypeVar(tDecl);
            var u = factory.MakeTypeVar(uDecl);

            paramsHolder.SetValue(Arr(
                new FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x")),
                new FuncParameter(M.FuncParameterKind.Params, u, NormalName("y")),
                new FuncParameter(M.FuncParameterKind.Ref, t, NormalName("z"))
            ));

            Assert.Equal(expectedGlobalFuncDecl, resultDecl);
        }
        
        // public struct S<T> : B<int>
        // {
        //     private static T Func<T, U>(S<int> s, U u) { }
        //     protected int x, y;
        // }
        // private struct B<T> { }
        [Fact]
        public void Build_StructDecl_ModuleInfoHasStructInfo()
        {
            var script = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(
                    S.AccessModifier.Public,
                    "S",
                    Arr(new S.TypeParam("T")),
                    Arr<S.TypeExp>(new S.IdTypeExp("B", Arr(SIntTypeExp()))),
                    Arr<S.StructMemberDecl>(
                        new S.StructMemberFuncDecl(
                            S.AccessModifier.Private,
                            IsStatic: true,
                            IsSequence: false,
                            IsRefReturn: false,
                            new S.IdTypeExp("T", default),
                            "Func",
                            Arr(new S.TypeParam("T"), new S.TypeParam("U")),
                            Arr(
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("S", SIntTypeExp()), "s"), 
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("U"), "u")
                            ),
                            default
                        ),

                        new S.StructMemberVarDecl(
                            S.AccessModifier.Protected,
                            SIntTypeExp(),
                            Arr("x", "y")
                        )
                    )
                )),

                new S.TypeDeclScriptElement(new S.StructDecl(
                    S.AccessModifier.Private,
                    "B",
                    Arr(new S.TypeParam("T")),
                    default,
                    default
                ))
            );

            var moduleName = NormalName("TestModule");
            var result = Build(moduleName, script);

            var s_intId = new ModuleSymbolId(moduleName, new SymbolPath(null, NormalName("S"), Arr(SymbolId.Int)));
            var uId = new TypeVarSymbolId(2);

            var funcParamIds = Arr(
                new FuncParamId(M.FuncParameterKind.Default, s_intId),
                new FuncParamId(M.FuncParameterKind.Default, uId)
            );

            // var trivialConstructor = new InternalModuleConstructorInfo(M.Accessor.Public, Arr(new M.Param(M.ParamKind.Default, IntMType, NormalName("x")), new M.Param(M.ParamKind.Default, IntMType, NormalName("y"))));

            var expected = new ModuleDeclBuilder(factory, moduleName)
                .BeginStruct(M.Accessor.Public, "S")
                    .TypeParam("T", out var _)
                    .BaseHolder(out var baseHolder) // B<int>

                    // private static T Func<T, U>(S<int> s, U u) { }
                    .BeginFunc(M.Accessor.Private, bStatic: false, NormalName("Func"))
                        .TypeParam("T", out var typeVarFuncTDecl)
                        .TypeParam("U", out var typeVarFuncUDecl)
                        .FuncReturnHolder(out var funcRetHolder)
                        .FuncParametersHolder(funcParamIds, out var funcParamsHolder)
                    .EndFunc(out var _)
                    .Var(M.Accessor.Protected, bStatic: false, intType.ToHolder(), NormalName("x"))
                    .Var(M.Accessor.Protected, bStatic: false, intType.ToHolder(), NormalName("y"))
                    .BeginConstructor(M.Accessor.Public, bTrivial: true)
                        .FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x"))
                        .FuncParameter(M.FuncParameterKind.Default, intType, NormalName("y"))
                    .EndConstructor(out var _)
                .EndStruct(out var expectedSDecl)
                .BeginStruct(M.Accessor.Private, "B")
                    .TypeParam("T", out var _)
                .EndStruct(out var expectedBDecl)
                .Make();

            var expectedModule = factory.MakeModule(expected);
            var expectedB_int = factory.MakeStruct(expectedModule, expectedBDecl, Arr<ITypeSymbol>(intType));
            baseHolder.SetValue(expectedB_int);

            var expectedS_int = factory.MakeStruct(expectedModule, expectedSDecl, Arr<ITypeSymbol>(intType));

            var typeVarFuncT = factory.MakeTypeVar(typeVarFuncTDecl);
            var typeVarFuncU = factory.MakeTypeVar(typeVarFuncUDecl);
            funcRetHolder.SetValue(new FuncReturn(isRef: false, typeVarFuncT));
            funcParamsHolder.SetValue(Arr(
                new FuncParameter(M.FuncParameterKind.Default, expectedS_int, NormalName("s")),
                new FuncParameter(M.FuncParameterKind.Default, typeVarFuncU, NormalName("u"))
            ));

            // TODO: expected에 trivialConstructor를 강제로 꽂을 방법이 없어서 equals체크를 하면 실패한다
            //       1:1로 비교하면 되는데, 그건 그거대로 고통스럽다

            Assert.Equal(expected, result);
        }
    }
}
