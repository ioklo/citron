using System.Diagnostics;
using Gum.Collections;

using Xunit;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using Gum.Test.Misc;
using System;
using System.IO;

namespace Gum.IR0Translator.Test
{    
    public class InternalModuleInfoBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior
        public M.Type IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", default); }
        
        InternalModuleInfo Build(M.ModuleName moduleName, S.Script script)
        {
            var typeSkelRepo = TypeSkeletonCollector.Collect(script);

            var externalModuleInfoRepo = new ModuleInfoRepository(default);
            var errorCollector = new TestErrorCollector(true);

            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(moduleName, script, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            return InternalModuleInfoBuilder.Build(moduleName, script, typeExpTypeValueService);
        }

        [Fact]
        public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
        {
            // F<T>(T t)
            var paramTypes1 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Normal, new M.TypeVarType(0, "T"))));

            // F<U>(U u)
            var paramTypes2 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Normal, new M.TypeVarType(0, "U"))));

            Assert.Equal(paramTypes1, paramTypes2);
        }

        [Fact]
        public void Build_FuncDecl_RefTypes()
        {
            // ref T Func<T>(ref T t) { return ref t; }
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                isSequence: false,
                isRefReturn: true,
                retType: new S.IdTypeExp("T", default),
                name: "Func",
                typeParams: Arr("T"),
                parameters: Arr(new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "t")),
                body: new S.BlockStmt(Arr<S.Stmt>(new S.ReturnStmt(new S.ReturnValueInfo(true, new S.IdentifierExp("t", default)))))
            )));

            var result = Build("TestModule", script);
            Assert.NotNull(result);
            Debug.Assert(result != null);

            var paramTypes = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Ref, new M.TypeVarType(0, "T"))));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 1, paramTypes));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var expected = new InternalModuleFuncInfo(
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: true,
                retType: new M.TypeVarType(0, "T"),
                name: "Func",                                
                typeParams: Arr("T"),
                parameters: Arr(new M.Param(M.ParamKind.Ref, new M.TypeVarType(0, "T"), "t"))
            );

            Assert.Equal(expected, funcInfo);
        }

        [Fact]
        public void Build_FuncDecl_ModuleInfoHasFuncInfo()
        {
            // void Func<T, U>(int x, params U y, T z)
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                isSequence: false,
                isRefReturn: false,
                VoidTypeExp,
                "Func",
                Arr("T", "U"),
                Arr(
                    new S.FuncParam(S.FuncParamKind.Normal, IntTypeExp, "x"),
                    new S.FuncParam(S.FuncParamKind.Params, new S.IdTypeExp("U", default), "y"),
                    new S.FuncParam(S.FuncParamKind.Normal, new S.IdTypeExp("T", default), "z")
                ),
                new S.BlockStmt(Arr<S.Stmt>())
            )));

            var result = Build("TestModule", script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var paramTypes = new M.ParamTypes(Arr(
                new M.ParamKindAndType(M.ParamKind.Normal, IntMType), 
                new M.ParamKindAndType(M.ParamKind.Params, new M.TypeVarType(1, "U")), 
                new M.ParamKindAndType(M.ParamKind.Normal, new M.TypeVarType(0, "T"))
            ));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 2, paramTypes));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var parameters = Arr(
                new M.Param(M.ParamKind.Normal, IntMType, "x"), 
                new M.Param(M.ParamKind.Params, new M.TypeVarType(1, "U"), "y"), 
                new M.Param(M.ParamKind.Normal, new M.TypeVarType(0, "T"), "z"));

            var expected = new InternalModuleFuncInfo(
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: false,
                M.VoidType.Instance,
                "Func",
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
                    default, // Arr<S.TypeExp>(new S.IdTypeExp("B", Arr(IntTypeExp))),
                    Arr<S.StructDeclElement>(
                        new S.FuncStructDeclElement(new S.StructFuncDecl(
                            S.AccessModifier.Private,
                            isStatic: true,
                            isSequence: false,
                            isRefReturn: false,
                            new S.IdTypeExp("T", default),
                            "Func",
                            Arr("T", "U"),
                            Arr(
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("S", IntTypeExp), "s"), 
                                new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("U"), "u")
                            ),
                            new S.BlockStmt(Arr<S.Stmt>())
                        )),

                        new S.VarStructDeclElement(
                            S.AccessModifier.Protected,
                            IntTypeExp,
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

            var structInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("S", 1)) as IModuleStructInfo;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var autoConstructor = new InternalModuleConstructorInfo("S", Arr(new M.Param(M.ParamKind.Normal, IntMType, "x"), new M.Param(M.ParamKind.Normal, IntMType, "y")));

            var expected = new InternalModuleStructInfo(
                "S",
                Arr("T"), 
                baseType: null, //baseType: new M.GlobalType(moduleName, M.NamespacePath.Root, "B", Arr(IntMType)),
                Array.Empty<IModuleTypeInfo>(),
                Arr<IModuleFuncInfo>(
                    new InternalModuleFuncInfo(
                        bInstanceFunc: true,
                        bSeqFunc: false,
                        bRefReturn: false,
                        new M.TypeVarType(1, "T"),
                        "Func",                        
                        Arr("T", "U"),
                        Arr(
                            new M.Param(M.ParamKind.Normal, new M.GlobalType(moduleName, M.NamespacePath.Root, "S", ImmutableArray.Create<M.Type>(IntMType)), "s"),
                            new M.Param(M.ParamKind.Normal, new M.TypeVarType(2, "U"), "u")
                        )
                    )
                ).AsEnumerable(),

                Arr<IModuleConstructorInfo>(autoConstructor),
                autoConstructor,
                Arr<IModuleMemberVarInfo>(
                    new InternalModuleMemberVarInfo(false, IntMType, "x"),
                    new InternalModuleMemberVarInfo(false, IntMType, "y")
                )
            );

            var writer0 = new StringWriter();
            Dumper.Dump(writer0, expected);
            var text0 = writer0.ToString();

            var writer1 = new StringWriter();
            Dumper.Dump(writer1, structInfo);
            var text1 = writer1.ToString();

            expected.Equals(structInfo);

            Assert.Equal(expected, structInfo);
        }
    }
}
