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
            var paramTypes1 = Arr<(M.ParamKind, M.Type)>((M.ParamKind.Normal, new M.TypeVarType(0, "T")));

            // F<U>(U u)
            var paramTypes2 = Arr<(M.ParamKind, M.Type)>((M.ParamKind.Normal, new M.TypeVarType(0, "U")));

            var paramHash1 = Misc.MakeParamHash(paramTypes1);
            var paramHash2 = Misc.MakeParamHash(paramTypes2);

            Assert.Equal(paramHash1, paramHash2);
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

            var paramHash = Misc.MakeParamHash(Arr<(M.ParamKind, M.Type)>((M.ParamKind.Ref, new M.TypeVarType(0, "T"))));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 1, paramHash));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var expected = new M.FuncInfo(
                name: "Func",
                isSequenceFunc: false,
                isRefReturn: true,
                isInstanceFunc: false, 
                typeParams: Arr("T"),
                retType: new M.TypeVarType(0, "T"),
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

            var paramHash = Misc.MakeParamHash(Arr<(M.ParamKind, M.Type)>(
                (M.ParamKind.Normal, IntMType), 
                (M.ParamKind.Params, new M.TypeVarType(1, "U")), 
                (M.ParamKind.Normal, new M.TypeVarType(0, "T"))
            ));

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry("Func", 2, paramHash));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var parameters = Arr(
                new M.Param(M.ParamKind.Normal, IntMType, "x"), 
                new M.Param(M.ParamKind.Params, new M.TypeVarType(1, "U"), "y"), 
                new M.Param(M.ParamKind.Normal, new M.TypeVarType(0, "T"), "z"));

            var expected = new M.FuncInfo(
                "Func",
                false,
                isRefReturn: false,
                false, Arr("T", "U"), M.VoidType.Instance, parameters
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
                        isSequenceFunc: false,
                        isRefReturn: false,
                        isInstanceFunc: true,
                        Arr("T", "U"),
                        new M.TypeVarType(1, "T"),
                        
                        Arr(
                            new M.Param(M.ParamKind.Normal, new M.GlobalType(moduleName, M.NamespacePath.Root, "S", ImmutableArray.Create<M.Type>(IntMType)), "s"),
                            new M.Param(M.ParamKind.Normal, new M.TypeVarType(2, "U"), "u")
                        )
                    )
                ),

                memberVars: ImmutableArray.Create(
                    new M.MemberVarInfo(false, IntMType, "x"),
                    new M.MemberVarInfo(false, IntMType, "y")
                )
            );

            Assert.Equal(expected, structInfo);
        }
    }
}
