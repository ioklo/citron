using System.Diagnostics;
using Gum.Collections;

using Xunit;

using static Gum.Infra.Misc;
using static Gum.IR0Translator.Test.SyntaxFactory;

using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0Translator.Test
{    
    public class ModuleInfoBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior
        public M.Type IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", default); }
        
        M.ModuleInfo Build(M.ModuleName moduleName, S.Script script)
        {
            var typeSkelRepo = TypeSkeletonCollector.Collect(script);

            var externalModuleInfoRepo = new ModuleInfoRepository(default);
            var errorCollector = new TestErrorCollector(true);

            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(moduleName, script, externalModuleInfoRepo, typeSkelRepo, errorCollector);

            return ModuleInfoBuilder.Build(moduleName, script, typeExpTypeValueService);
        }

        [Fact]
        public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
        {
            // F<T>(T t)
            var paramTypes1 = Arr<M.Type>(new M.TypeVarType(0, "T"));

            // F<U>(U u)
            var paramTypes2 = Arr<M.Type>(new M.TypeVarType(0, "U"));

            var paramHash1 = Misc.MakeParamHash(paramTypes1);
            var paramHash2 = Misc.MakeParamHash(paramTypes2);

            Assert.True(paramHash1 == paramHash2);
        }

        [Fact]
        public void Build_FuncDecl_ModuleInfoHasFuncInfo()
        {
            var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
                bSequence: false,
                VoidTypeExp,
                "Func",
                Arr("T", "U"),
                new S.FuncParamInfo(Arr(
                    new S.TypeAndName(IntTypeExp, "x"),
                    new S.TypeAndName(new S.IdTypeExp("U", default), "y"),
                    new S.TypeAndName(new S.IdTypeExp("T", default), "z")
                ), 1),
                new S.BlockStmt(Arr<S.Stmt>())
            )));

            var result = Build("TestModule", script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var paramHash = Misc.MakeParamHash(Arr<M.Type>(IntMType, new M.TypeVarType(1, "U"), new M.TypeVarType(0, "T")));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 2, paramHash));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var paramTypes = Arr<M.Type>(IntMType, new M.TypeVarType(1, "U"), new M.TypeVarType(0, "T"));

            var expected = new M.FuncInfo(
                "Func",
                false, false, Arr("T", "U"), M.VoidType.Instance, paramTypes
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
                    Arr<S.TypeExp>(new S.IdTypeExp("B", Arr(IntTypeExp))),
                    Arr<S.StructDeclElement>(
                        new S.FuncStructDeclElement(new S.StructFuncDecl(
                            S.AccessModifier.Private,
                            bStatic: true,
                            bSequence: false,
                            new S.IdTypeExp("T", default),
                            "Func",
                            Arr("T", "U"),
                            new S.FuncParamInfo(Arr(new S.TypeAndName(SIdTypeExp("S", IntTypeExp), "s"), new S.TypeAndName(SIdTypeExp("U"), "u")), null),
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

            var structInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("S", 1)) as M.StructInfo;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var expected = new M.StructInfo(
                "S",
                Arr("T"), 
                baseType: new M.GlobalType(moduleName, M.NamespacePath.Root, "B", Arr(IntMType)), 
                interfaces: default,
                memberTypes: default,
                memberFuncs: Arr<M.FuncInfo>(
                    new M.FuncInfo(
                        "Func",
                        bSeqCall: false, 
                        bThisCall: true,
                        Arr("T", "U"),
                        new M.TypeVarType(1, "T"),
                        Arr<M.Type>(
                            new M.GlobalType(moduleName, M.NamespacePath.Root, "S", ImmutableArray.Create<M.Type>(IntMType)),
                            new M.TypeVarType(2, "U")
                        )
                    )
                ),

                memberVars: ImmutableArray.Create<M.MemberVarInfo>(
                    new M.MemberVarInfo(false, IntMType, "x"),
                    new M.MemberVarInfo(false, IntMType, "y")
                )
            );

            Assert.Equal(expected, structInfo);
        }
    }
}
