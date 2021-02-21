using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Xunit;
using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Gum.IR0Translator.Test
{
    public class TypeExpEvaluatorTests
    {
        TypeExpInfoService Evaluate(S.Script script)
        {
            M.ModuleName moduleName = "TestModule";
            var errorCollector = new TestErrorCollector(true);

            var skelRepo = TypeSkeletonCollector.Collect(script);            

            var moduleInfoRepo = new ModuleInfoRepository(ImmutableArray<M.ModuleInfo>.Empty);
            var result = TypeExpEvaluator.Evaluate(moduleName, script, moduleInfoRepo, skelRepo, errorCollector);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            return result;
        }

        // UnitOfWorkName_ScenarioName_ExpectedBehavior                
        
        [Fact]
        public void EvaluateType_TopLevelStmt_WorksProperly()
        {
            S.IdTypeExp intTypeExp = new S.IdTypeExp("int");

            var script = new S.Script(
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    intTypeExp, new S.VarDeclElement("x", null)
                )))
            );

            var result = Evaluate(script);

            var typeExpInfo = result.GetTypeExpInfo(intTypeExp);
            var expected = new MTypeTypeExpInfo(new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", ImmutableArray<M.Type>.Empty));

            Assert.Equal(expected, typeExpInfo);
        }

        // X<int>.Y<short> 라면 
        [Fact]
        public void EvaluateType_CompositionOfTypeExp_OnlyWholeTypeExpAddedToDictionary()
        {
            var innerTypeExp = new S.IdTypeExp("X", new S.IdTypeExp("int"));
            var typeExp = new S.MemberTypeExp(innerTypeExp, "Y", Array.Empty<S.TypeExp>());

            var script = new S.Script(
                new S.TypeDeclScriptElement(new S.StructDecl(
                    S.AccessModifier.Public, "X", new[] { "T" }, Array.Empty<S.TypeExp>(), new S.StructDeclElement[] {
                        new S.TypeStructDeclElement(new S.StructDecl(
                            S.AccessModifier.Public, "Y", Array.Empty<string>(), Array.Empty<S.TypeExp>(), Array.Empty<S.StructDeclElement>()
                        ))
                    }
                )),
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    typeExp, new S.VarDeclElement("x", null)
                )))
            );

            var result = Evaluate(script);

            Assert.ThrowsAny<Exception>(() => result.GetTypeExpInfo(innerTypeExp));
        }
    }
}
