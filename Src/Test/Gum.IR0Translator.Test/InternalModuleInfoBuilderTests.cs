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
        public M.Type IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), new M.Name.Normal("Int32"), default); }
        
        InternalModuleInfo Build(M.ModuleName moduleName, S.Script script)
        {
            var typeSkelRepo = TypeSkeletonCollector.Collect(script);

            var externalModuleInfoRepo = new ModuleInfoRepository(default);
            var logger = new TestLogger(true);

            var typeExpTypeValueService = TypeExpEvaluator.Evaluate(moduleName, script, externalModuleInfoRepo, typeSkelRepo, logger);

            var (info, _, _) = InternalModuleInfoBuilder.Build(moduleName, script, typeExpTypeValueService, externalModuleInfoRepo);
            return info;
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
                null,
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

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry(new M.Name.Normal("Func"), 1, paramTypes));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var expected = new InternalModuleFuncInfo(
                M.AccessModifier.Private,
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: true,
                retType: new M.TypeVarType(0, "T"),
                name: new M.Name.Normal("Func"),                                
                typeParams: Arr("T"),
                parameters: Arr(new M.Param(M.ParamKind.Ref, new M.TypeVarType(0, "T"), new M.Name.Normal("t")))
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

            var funcInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry(new M.Name.Normal("Func"), 2, paramTypes));
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var parameters = Arr(
                new M.Param(M.ParamKind.Normal, IntMType, new M.Name.Normal("x")), 
                new M.Param(M.ParamKind.Params, new M.TypeVarType(1, "U"), new M.Name.Normal("y")), 
                new M.Param(M.ParamKind.Normal, new M.TypeVarType(0, "T"), new M.Name.Normal("z")));

            var expected = new InternalModuleFuncInfo(
                M.AccessModifier.Private,
                bInstanceFunc: false,
                bSeqFunc: false,
                bRefReturn: false,
                M.VoidType.Instance,
                new M.Name.Normal("Func"),
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

            var structInfo = GlobalItemQueryService.GetGlobalItem(result, M.NamespacePath.Root, new ItemPathEntry(new M.Name.Normal("S"), 1)) as IModuleStructInfo;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var trivialConstructor = new InternalModuleConstructorInfo(M.AccessModifier.Public, Arr(new M.Param(M.ParamKind.Normal, IntMType, new M.Name.Normal("x")), new M.Param(M.ParamKind.Normal, IntMType, new M.Name.Normal("y"))));

            var expected = new InternalModuleStructInfo(
                new M.Name.Normal("S"),
                Arr("T"), 
                mbaseStruct: null, //baseType: new M.GlobalType(moduleName, M.NamespacePath.Root, "B", Arr(IntMType)),
                default,
                Arr<IModuleFuncInfo>(
                    new InternalModuleFuncInfo(
                        M.AccessModifier.Private,
                        bInstanceFunc: true,
                        bSeqFunc: false,
                        bRefReturn: false,
                        new M.TypeVarType(1, "T"),
                        new M.Name.Normal("Func"),                        
                        Arr("T", "U"),
                        Arr(
                            new M.Param(M.ParamKind.Normal, new M.GlobalType(moduleName, M.NamespacePath.Root, new M.Name.Normal("S"), ImmutableArray.Create<M.Type>(IntMType)), new M.Name.Normal("s")),
                            new M.Param(M.ParamKind.Normal, new M.TypeVarType(2, "U"), new M.Name.Normal("u"))
                        )
                    )
                ),

                Arr<IModuleConstructorInfo>(trivialConstructor),
                
                Arr<IModuleMemberVarInfo>(
                    new InternalModuleMemberVarInfo(M.AccessModifier.Protected, false, IntMType, new M.Name.Normal("x")),
                    new InternalModuleMemberVarInfo(M.AccessModifier.Protected, false, IntMType, new M.Name.Normal("y"))
                )
            );

            // TODO: expected에 trivialConstructor를 강제로 꽂을 방법이 없어서 equals체크를 하면 실패한다
            //       1:1로 비교하면 되는데, 그건 그거대로 고통스럽다

            expected.Equals(structInfo);
            Assert.Equal(expected, structInfo);
        }
    }
}
