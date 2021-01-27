using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;
using Xunit;
using M = Gum.CompileTime;
using System.Linq;
using System.Diagnostics;
using System.Collections.Immutable;

namespace Gum.IR0
{    
    public class ModuleInfoBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        public M.Type IntType { get => new M.ExternalType("System.Runtime", new M.NamespacePath("System"), "Int32", ImmutableArray<M.Type>.Empty); }

        public S.TypeExp IntTypeExp { get => new S.IdTypeExp("int"); }
        public S.TypeExp VoidTypeExp { get => new S.IdTypeExp("void"); }
        M.ModuleInfo Build(M.ModuleName moduleName, S.Script script)
        {
            var typeSkelRepo = TypeSkeletonCollector.Collect(script);

            var externalModuleInfoRepo = new ModuleInfoRepository(ImmutableArray<M.ModuleInfo>.Empty);
            var errorCollector = new TestErrorCollector(true);

            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(moduleName, script, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            return ModuleInfoBuilder.Build(moduleName, script, typeExpTypeValueService);
        }

        [Fact]
        public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
        {
            // F<T>(T t)
            var paramTypes1 = ImmutableArray.Create<M.Type>(new M.TypeVarType(0, 0, "T"));

            // F<U>(U u)
            var paramTypes2 = ImmutableArray.Create<M.Type>(new M.TypeVarType(0, 0, "U"));

            var paramHash1 = Misc.MakeParamHash(paramTypes1);
            var paramHash2 = Misc.MakeParamHash(paramTypes2);

            Assert.True(paramHash1 == paramHash2);
        }

        [Fact]
        public void Build_FuncDecl_ModuleInfoHasFuncInfo()
        {
            var script = new S.Script(new S.Script.GlobalFuncDeclElement(new S.GlobalFuncDecl(
                bSequence: false,
                VoidTypeExp,
                "Func",
                new[] { "T", "U" },
                new S.FuncParamInfo(new[] {
                    new S.TypeAndName(IntTypeExp, "x"),
                    new S.TypeAndName(new S.IdTypeExp("U"), "y"),
                    new S.TypeAndName(new S.IdTypeExp("T"), "z"),
                }, 1),
                new S.BlockStmt()
            )));

            var result = Build("TestModule", script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var paramHash = Misc.MakeParamHash(ImmutableArray.Create<M.Type>(IntType, new M.TypeVarType(0, 1, "U"), new M.TypeVarType(0, 0, "T")));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 2, paramHash));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var paramTypes = new M.Type[] { IntType, new M.TypeVarType(0, 1, "U"), new M.TypeVarType(0, 0, "T") };

            var expected = new M.FuncInfo(
                "Func",
                false, false, new[] { "T", "U" }, M.VoidType.Instance, paramTypes
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
            var script = new S.Script(
                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Public,
                    "S",
                    new[] { "T" },
                    new[] { new S.IdTypeExp("B", IntTypeExp) },
                    new S.StructDecl.Element[] {
                        new S.StructDecl.FuncDeclElement(new S.StructFuncDecl(
                            S.AccessModifier.Private,
                            bStatic: true,
                            bSequence: false,
                            new S.IdTypeExp("T"),
                            "Func",
                            new[] {"T", "U"},
                            new S.FuncParamInfo(new[] { new S.TypeAndName(new S.IdTypeExp("S", IntTypeExp), "s"), new S.TypeAndName(new S.IdTypeExp("U"), "u") }, null),
                            new S.BlockStmt()
                        )),

                        new S.StructDecl.VarDeclElement(
                            S.AccessModifier.Protected,
                            IntTypeExp,
                            new[] {"x", "y"}                        
                        )
                    }
                )),

                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Private,
                    "B",
                    new[] { "T" },
                    Array.Empty<S.TypeExp>(),
                    Array.Empty<S.StructDecl.Element>()
                ))
            );
            var moduleName = "TestModule";
            var result = Build(moduleName, script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var structInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("S", 1)) as M.StructInfo;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var expected = new M.StructInfo(
                "S",
                ImmutableArray.Create("T"),
                ImmutableArray.Create<M.Type>(new M.ExternalType(moduleName, M.NamespacePath.Root, "B", ImmutableArray.Create(IntType))),

                memberTypes: ImmutableArray<M.TypeInfo>.Empty,
                memberFuncs: ImmutableArray.Create<M.FuncInfo>(
                    new M.FuncInfo(
                        "Func",
                        bSeqCall: false, 
                        bThisCall: true,
                        new[] { "T", "U" },
                        new M.TypeVarType(1, 0, "T"),
                        ImmutableArray.Create<M.Type>(
                            new M.ExternalType(moduleName, M.NamespacePath.Root, "S", ImmutableArray.Create<M.Type>(IntType)),
                            new M.TypeVarType(1, 1, "U")
                        )
                    )
                ),

                memberVars: ImmutableArray.Create<M.MemberVarInfo>(
                    new M.MemberVarInfo(false, IntType, "x"),
                    new M.MemberVarInfo(false, IntType, "y")
                )
            );

            Assert.Equal(expected, structInfo);
        }
    }
}
