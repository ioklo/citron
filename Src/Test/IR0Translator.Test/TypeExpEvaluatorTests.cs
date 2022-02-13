using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Text;
using Xunit;
using S = Citron.Syntax;
using M = Citron.CompileTime;

using static Citron.Syntax.SyntaxFactory;
using static Citron.Infra.Misc;
using Citron.Test.Misc;

namespace Citron.IR0Translator.Test
{
    public class TypeExpEvaluatorTests
    {
        TypeExpInfoService Evaluate(S.Script script)
        {
            M.Name moduleName = new M.Name.Normal("TestModule");
            var testLogger = new TestLogger(true);

            var skelRepo = TypeSkeletonCollector.Collect(script);            

            var moduleInfoRepo = new ExternalModuleDeclRepository(ImmutableArray<M.ModuleDecl>.Empty);
            var result = TypeExpEvaluator.Evaluate(moduleName, script, moduleInfoRepo, skelRepo, testLogger);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            return result;
        }

        // UnitOfWorkName_ScenarioName_ExpectedBehavior                
        
        [Fact]
        public void EvaluateType_TopLevelStmt_WorksProperly()
        {
            S.TypeExp intTypeExp;

            var script = SScript(SVarDeclStmt(intTypeExp = SIntTypeExp(), "x"));

            var result = Evaluate(script);

            var typeExpInfo = result.GetTypeExpInfo(intTypeExp);
            var expected = new MTypeTypeExpInfo(new M.GlobalType("System.Runtime", new M.NamespacePath("System"), new M.Name.Normal("Int32"), ImmutableArray<M.TypeId>.Empty), TypeExpInfoKind.Struct, false);

            Assert.Equal(expected, typeExpInfo);
        }

        // X<int>.Y<short> 라면 
        [Fact]
        public void EvaluateType_CompositionOfTypeExp_OnlyWholeTypeExpAddedToDictionary()
        {
            var innerTypeExp = SIdTypeExp("X", SIntTypeExp());
            var typeExp = new S.MemberTypeExp(innerTypeExp, "Y", Arr<S.TypeExp>());

            var script = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(
                    S.AccessModifier.Public, "X", Arr("T"), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(
                        new S.StructMemberTypeDecl(new S.StructDecl(
                            S.AccessModifier.Public, "Y", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>()
                        ))
                    )
                )),
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    false, typeExp, Arr(new S.VarDeclElement("x", null)))))
            );

            var result = Evaluate(script);

            Assert.ThrowsAny<Exception>(() => result.GetTypeExpInfo(innerTypeExp));
        }
    }
}
