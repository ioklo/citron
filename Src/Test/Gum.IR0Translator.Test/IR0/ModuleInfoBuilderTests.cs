using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;
using Xunit;
using Gum.CompileTime;
using System.Linq;
using System.Diagnostics;

namespace Gum.IR0
{    
    public class ModuleInfoBuilderTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        public S.TypeExp IntTypeExp { get => new S.IdTypeExp("int"); }
        public S.TypeExp VoidTypeExp { get => new S.IdTypeExp("void"); }
        ModuleInfoBuilder.Result? Build(S.Script script)
        {
            var moduleInfoBuilder = new ModuleInfoBuilder(new TypeExpEvaluator());
            var errorCollector = new TestErrorCollector(true);

            return moduleInfoBuilder.Build(Array.Empty<IModuleInfo>(), script, errorCollector);
        }

        [Fact]
        public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
        {
            // F<T>(T t)
            var paramTypes1 = new TypeValue[] { new TypeValue.TypeVar(0, 0, "T")};

            // F<U>(U u)
            var paramTypes2 = new TypeValue[] { new TypeValue.TypeVar(0, 0, "U")};

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

            var result = Build(script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var funcInfo = result.ModuleInfo.GetFuncs(NamespacePath.Root, "Func").SingleOrDefault();
            Assert.NotNull(funcInfo);
            Debug.Assert(funcInfo != null);

            var paramTypes = new TypeValue[] { TypeValues.Int, new TypeValue.TypeVar(0, 1, "U"), new TypeValue.TypeVar(0, 0, "T") };
            var funcId = new ItemId(ModuleName.Internal, NamespacePath.Root, new ItemPathEntry("Func", 2, Misc.MakeParamHash(paramTypes)));

            var expected = new FuncInfo(
                funcId,
                false, false, new[] { "T", "U" }, TypeValue.Void.Instance, paramTypes
            );

            Assert.Equal(expected, funcInfo, ModuleInfoEqualityComparer.Instance);
        }
        
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
                        new S.StructDecl.FuncDeclElement(
                            S.AccessModifier.Private,
                            true,
                            false,
                            new S.IdTypeExp("T"),
                            "Func",
                            new[] {"T", "U"},
                            new S.FuncParamInfo(new[] { new S.TypeAndName(new S.IdTypeExp("S", IntTypeExp), "s"), new S.TypeAndName(new S.IdTypeExp("U"), "u") }, null),
                            new S.BlockStmt()
                        ),

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

            var result = Build(script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var structInfo = result.ModuleInfo.GetItem(NamespacePath.Root, new ItemPathEntry("S", 1)) as StructInfo;
            Assert.NotNull(structInfo);
            Debug.Assert(structInfo != null);

            var sId = new ItemId(ModuleName.Internal, new ItemPath(NamespacePath.Root, new ItemPathEntry("S", 1)));

            var paramTypeValues = new TypeValue[] {
                new TypeValue.Normal(sId, new[] { TypeValues.Int }),
                new TypeValue.TypeVar(1, 1, "U")
            };
            var paramHash = Misc.MakeParamHash(paramTypeValues);
            var sFuncId = sId.Append("Func", 2, paramHash);

            var sxId = sId.Append("x");
            var syId = sId.Append("y");

            var expected = new StructInfo(
                sId,
                new[] { "T", "U" },
                new TypeValue.Normal(ModuleName.Internal, new AppliedItemPath(NamespacePath.Root, new AppliedItemPathEntry("B", string.Empty, new[] { TypeValues.Int }))),
                new ItemInfo[] {
                    new FuncInfo(
                        sFuncId,
                        bSeqCall: false, bThisCall: true,
                        new[] { "T" },
                        new TypeValue.TypeVar(1, 0, "T"),
                        paramTypeValues
                    ),

                    new VarInfo(sxId, false, TypeValues.Int),
                    new VarInfo(syId, false, TypeValues.Int),
                }
            );

            Assert.Equal(expected, structInfo, ModuleInfoEqualityComparer.Instance);
        }


    }
}
