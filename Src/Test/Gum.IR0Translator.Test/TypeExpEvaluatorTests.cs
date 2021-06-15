﻿using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Text;
using Xunit;
using S = Gum.Syntax;
using M = Gum.CompileTime;

using static Gum.IR0Translator.Test.SyntaxFactory;
using static Gum.Infra.Misc;

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
            S.TypeExp intTypeExp;

            var script = SScript(SVarDeclStmt(intTypeExp = IntTypeExp, "x"));

            var result = Evaluate(script);

            var typeExpInfo = result.GetTypeExpInfo(intTypeExp);
            var expected = new MTypeTypeExpInfo(new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", ImmutableArray<M.Type>.Empty));

            Assert.Equal(expected, typeExpInfo);
        }
        
        [Fact]
        public void EvaluateType_RefTypeExp_ReturnsNoMemberTypeExpInfo()
        {
            // int x;
            // ref int p;
            S.TypeExp refTypeExp;

            var script = SScript(
                SVarDeclStmt(new S.IdTypeExp("int", default), "x"),
                SVarDeclStmt(refTypeExp = new S.RefTypeExp(new S.IdTypeExp("int", default)), "p", new S.IdentifierExp("x", default)));

            var result = Evaluate(script);

            var typeExpInfo = result.GetTypeExpInfo(refTypeExp);
            var expected = new MTypeTypeExpInfo(new M.RefType(new M.GlobalType("System.Runtime", new M.NamespacePath("System"), "Int32", ImmutableArray<M.Type>.Empty)));

            Assert.Equal(expected, typeExpInfo);
        }

        // X<int>.Y<short> 라면 
        [Fact]
        public void EvaluateType_CompositionOfTypeExp_OnlyWholeTypeExpAddedToDictionary()
        {
            var innerTypeExp = SIdTypeExp("X", IntTypeExp);
            var typeExp = new S.MemberTypeExp(innerTypeExp, "Y", Arr<S.TypeExp>());

            var script = SScript(
                new S.TypeDeclScriptElement(new S.StructDecl(
                    S.AccessModifier.Public, "X", Arr("T"), Arr<S.TypeExp>(), Arr<S.StructDeclElement>(
                        new S.TypeStructDeclElement(new S.StructDecl(
                            S.AccessModifier.Public, "Y", Arr<string>(), Arr<S.TypeExp>(), Arr<S.StructDeclElement>()
                        ))
                    )
                )),
                new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                    typeExp, Arr(new S.VarDeclElement("x", null)))))
            );

            var result = Evaluate(script);

            Assert.ThrowsAny<Exception>(() => result.GetTypeExpInfo(innerTypeExp));
        }
    }
}
