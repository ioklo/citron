﻿using System.Diagnostics;
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

namespace Citron.IR0Translator.Test
{    
    public class InternalModuleDeclSymbolBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        // public M.TypeId IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), NormalName("Int32"), default); }

        M.Name.Normal NormalName(string text)
        {
            return NormalName(text);
        }
        
        ModuleDeclSymbol Build(M.Name moduleName, S.Script script)
        {   
            ImmutableArray<ModuleDeclSymbol> referenceModules = default;

            var logger = new TestLogger(true);
            TypeExpEvaluator.Evaluate(moduleName, script, referenceModules, logger);

            var symbolFactory = new SymbolFactory();
            var result = InternalModuleDeclSymbolBuilder.Build(moduleName, script, symbolFactory, referenceModules);

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
                typeParams: Arr("T"),
                parameters: Arr(new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "t")),
                body: Arr<S.Stmt>(new S.ReturnStmt(new S.ReturnValueInfo(true, new S.IdentifierExp("t", default))))
            )));

            var moduleName = NormalName("TestModule");
            var result = Build(moduleName, script);
            Assert.NotNull(result);
            Debug.Assert(result != null);

            // var paramTypes = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Ref, new M.TypeVarTypeId(0, "T"))));

            var paramId = new FuncParamId(M.FuncParameterKind.Ref, new TypeVarSymbolId(0));
            var path = new DeclSymbolPath(null, NormalName("Func"), 1, Arr(paramId));

            var resultDecl = result.GetDeclSymbol(path) as GlobalFuncDeclSymbol;
            Debug.Assert(resultDecl != null);

            // declSymbol을 얻어낼 수 있는 방법
            var factory = new SymbolFactory();
            var builder = new ModuleDeclBuilder(factory, moduleName);
            builder.BeginGlobalFunc(
                new FuncReturn(isRef: true),
                NormalName("Func"),
                Arr<FuncParameter>(new FuncParameter(M.FuncParameterKind.Ref, ))

            var expected = new GlobalFuncDeclSymbol(
                M.AccessModifier.Private,
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: true,
                retType: new M.TypeVarTypeId(0, "T"),
                name: NormalName("Func"),                                
                typeParams: Arr("T"),
                parameters: Arr(new M.Param(M.ParamKind.Ref, new M.TypeVarTypeId(0, "T"), NormalName("t")))
            );

            Assert.Equal(expected, funcInfo);
        }

        [Fact]
        public void Build_FuncDecl_ModuleInfoHasFuncInfo()
        {
            // void Func<T, U>(int x, params U y, T z)
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                null,
                isSequence: false,
                isRefReturn: false,
                SVoidTypeExp(),
                "Func",
                Arr("T", "U"),
                Arr(
                    new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x"),
                    new S.FuncParam(S.FuncParamKind.Params, new S.IdTypeExp("U", default), "y"),
                    new S.FuncParam(S.FuncParamKind.Normal, new S.IdTypeExp("T", default), "z")
                ),
                Arr<S.Stmt>()
            )));

            var result = Build(NormalName("TestModule"), script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var paramTypes = new M.ParamTypes(Arr(
                new M.ParamKindAndType(M.ParamKind.Default, IntMType), 
                new M.ParamKindAndType(M.ParamKind.Params, new M.TypeVarTypeId(1, "U")), 
                new M.ParamKindAndType(M.ParamKind.Default, new M.TypeVarTypeId(0, "T"))
            ));


            var funcInfo = GlobalItemQueryService.GetGlobalDeclSymbol(result, M.NamespacePath.Root, new ItemPathEntry(NormalName("Func"), 2, paramTypes));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var parameters = Arr(
                new M.Param(M.ParamKind.Default, IntMType, NormalName("x")), 
                new M.Param(M.ParamKind.Params, new M.TypeVarTypeId(1, "U"), NormalName("y")), 
                new M.Param(M.ParamKind.Default, new M.TypeVarTypeId(0, "T"), NormalName("z")));

            var expected = new InternalModuleFuncInfo(
                M.AccessModifier.Private,
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: false,
                M.VoidTypeId.Instance,
                NormalName("Func"),
                Arr("T", "U"),
                parameters
            );

            Assert.Equal(expected, funcInfo);
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
                    Arr("T"),
                    default, // Arr<S.TypeExp>(new S.IdTypeExp("B", Arr(SIntTypeExp()))),
                    Arr<S.StructMemberDecl>(
                        new S.StructMemberFuncDecl(
                            S.AccessModifier.Private,
                            IsStatic: true,
                            IsSequence: false,
                            IsRefReturn: false,
                            new S.IdTypeExp("T", default),
                            "Func",
                            Arr("T", "U"),
                            Arr(
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("S", SIntTypeExp()), "s"), 
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("U"), "u")
                            ),
                            new S.BlockStmt(Arr<S.Stmt>())
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
                    Arr("T"),
                    default,
                    default
                ))
            );
            var moduleName = "TestModule";
            var result = Build(moduleName, script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var structInfo = GlobalItemQueryService.GetGlobalDeclSymbol(result, M.NamespacePath.Root, new ItemPathEntry(NormalName("S"), 1)) as IModuleStructDecl;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var trivialConstructor = new InternalModuleConstructorInfo(M.AccessModifier.Public, Arr(new M.Param(M.ParamKind.Default, IntMType, NormalName("x")), new M.Param(M.ParamKind.Default, IntMType, NormalName("y"))));

            var expected = new StructDeclSymbol(
                NormalName("S"),
                Arr("T"), 
                mbaseStruct: null, //baseType: new M.GlobalType(moduleName, M.NamespacePath.Root, "B", Arr(IntMType)),
                default,
                Arr<IModuleFuncDecl>(
                    new InternalModuleFuncInfo(
                        M.AccessModifier.Private,
                        bInstanceFunc: true,
                        bSeqFunc: false,
                        bRefReturn: false,
                        new M.TypeVarTypeId(1, "T"),
                        NormalName("Func"),                        
                        Arr("T", "U"),
                        Arr(
                            new M.Param(M.ParamKind.Default, new M.GlobalType(moduleName, M.NamespacePath.Root, NormalName("S"), ImmutableArray.Create<M.TypeId>(IntMType)), NormalName("s")),
                            new M.Param(M.ParamKind.Default, new M.TypeVarTypeId(2, "U"), NormalName("u"))
                        )
                    )
                ),

                Arr<IModuleConstructorDecl>(trivialConstructor),
                
                Arr<IModuleMemberVarInfo>(
                    new InternalModuleMemberVarInfo(M.AccessModifier.Protected, false, IntMType, NormalName("x")),
                    new InternalModuleMemberVarInfo(M.AccessModifier.Protected, false, IntMType, NormalName("y"))
                )
            );

            // TODO: expected에 trivialConstructor를 강제로 꽂을 방법이 없어서 equals체크를 하면 실패한다
            //       1:1로 비교하면 되는데, 그건 그거대로 고통스럽다

            expected.Equals(structInfo);
            Assert.Equal(expected, structInfo);
        }
    }
}
