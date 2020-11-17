using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using S = Gum.Syntax;

namespace Gum.IR0
{
    public class TypeExpEvaluatorTests
    {
        // UnitOfWorkName_ScenarioName_ExpectedBehavior

        // IdentifierExp가 Type을 지칭하는 경우, 
        [Fact]
        public void TypeEvaluate_IdExpIsTypeExp_MakesIdExpToTypeValueMapping()
        {
            var skeletonCollector = new TypeSkeletonCollector();
            var evaluator = new TypeExpEvaluator(skeletonCollector);

            S.Exp exp;
            // struct S<T> { }
            // struct X {}
            // int x = S<X>.x;
            var script = new S.Script(
                
                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Public, "S", new[] {"T"}, Array.Empty<S.TypeExp>(), Array.Empty<S.StructDecl.Element>()
                )),

                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Public, "X", Array.Empty<string>(), Array.Empty<S.TypeExp>(), Array.Empty<S.StructDecl.Element>()
                )),

                new S.Script.StmtElement(new S.VarDeclStmt(new S.VarDecl(
                    new S.IdTypeExp("int"), 
                    new S.VarDeclElement("x", new S.MemberExp(exp = new S.IdentifierExp("S", new S.IdTypeExp("X")), "x", Array.Empty<S.TypeExp>()))
                )))
            );

            var errorCollector = new TestErrorCollector(true);
            var result = evaluator.EvaluateScript(script, Array.Empty<IModuleInfo>(), errorCollector);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var typeValue = result.Value.TypeExpTypeValueService.GetTypeValue(exp);
            var expected = new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, 
                new AppliedItemPathEntry(
                    "S", string.Empty, new[] { new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, new AppliedItemPathEntry("X")) }));

            Debug.Assert(typeValue != null);
            Assert.Equal(expected, typeValue, ModuleInfoEqualityComparer.Instance);
        }

        // struct A<T> { struct B { } }
        // struct X {}
        // int x = A<X>.B.x; 
        // MemberExp(IdExp("A", ["X"]), "B") => TypeValue(A<X>.B)
        // IdExp("A", ["X"]) => TypeValue(A<X>) // TODO: 사실 이건 필요없을거 같다
        [Fact]
        public void TypeEvaluate_MemberExpIsTypeExp_MakesMemberExpToTypeValueMapping()
        {
            var skeletonCollector = new TypeSkeletonCollector();
            var evaluator = new TypeExpEvaluator(skeletonCollector);

            S.Exp exp1, exp2;          
            var script = new S.Script(

                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Public, "A", new[] { "T" }, Array.Empty<S.TypeExp>(), new S.StructDecl.Element[] {
                        new S.StructDecl.TypeDeclElement(new S.StructDecl(
                            S.AccessModifier.Public, "B", Array.Empty<string>(), Array.Empty<S.TypeExp>(), Array.Empty<S.StructDecl.Element>()
                        ))
                    }
                )),

                new S.Script.TypeDeclElement(new S.StructDecl(
                    S.AccessModifier.Public, "X", Array.Empty<string>(), Array.Empty<S.TypeExp>(), Array.Empty<S.StructDecl.Element>()
                )),

                new S.Script.StmtElement(new S.VarDeclStmt(new S.VarDecl(
                    new S.IdTypeExp("int"),
                    new S.VarDeclElement(
                        "x",
                        new S.MemberExp(
                            exp1 = new S.MemberExp(exp2 = new S.IdentifierExp("A", new S.IdTypeExp("X")), "B", 
                            Array.Empty<S.TypeExp>()), "x", Array.Empty<S.TypeExp>()
                        )
                    )
                )))
            );

            var errorCollector = new TestErrorCollector(false);
            var result = evaluator.EvaluateScript(script, Array.Empty<IModuleInfo>(), errorCollector);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            var typeValue1 = result.Value.TypeExpTypeValueService.GetTypeValue(exp1);
            var typeValue2 = result.Value.TypeExpTypeValueService.GetTypeValue(exp2);

            var expected1 = new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root,
                new AppliedItemPathEntry[]
                {
                    new AppliedItemPathEntry("A", string.Empty, 
                        new[] { new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, new AppliedItemPathEntry("X")) } 
                    )
                },

                new AppliedItemPathEntry("B")
            );

            Debug.Assert(typeValue1 != null);
            Assert.Equal(expected1, typeValue1, ModuleInfoEqualityComparer.Instance);
            Assert.Null(typeValue2); // 최외각만 TypeValue 매핑을 준다
        }
    }
}
