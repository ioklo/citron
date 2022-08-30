using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Text;
using Xunit;

using Citron.Module;

using static Citron.Syntax.SyntaxFactory;
using static Citron.Infra.Misc;
using Citron.Test.Misc;
using Citron.Analysis;
using Citron.Symbol;

namespace Citron.Syntax
{
    public class TypeExpEvaluatorTests
    {
        void Evaluate(Script script)
        {
            Name moduleName = new Name.Normal("TestModule");
            var testLogger = new TestLogger(true);
            
            TypeExpEvaluator.Evaluate(moduleName, script, default, testLogger);
        }

        // UnitOfWorkName_ScenarioName_ExpectedBehavior
        
        [Fact]
        public void EvaluateType_TopLevelStmt_WorksProperly()
        {
            TypeExp intTypeExp;

            var script = SScript(SVarDeclStmt(intTypeExp = SIntTypeExp(), "x"));

            Evaluate(script);

            var info = intTypeExp.Info;            

            // 겉보기만 제대로 되면 된다
            Assert.Equal(TypeExpInfoKind.Struct, info.GetKind());
            Assert.Equal(
                new ModuleSymbolId(new Name.Normal("System.Runtime"), new SymbolPath(new SymbolPath(null, new Name.Normal("System")), new Name.Normal("Int32"))),
                info.GetSymbolId()
            );
            Assert.Equal(intTypeExp, info.GetTypeExp());
        }

        // 중첩된 TypeExp는 가장 최상위에만 Info를 붙인다
        // TypeExp 자체는 Type이 아닐 수 있기 때문이다. TypeExpInfo는 타입에만 유효하다
        // 예) X<int>.Y<short> 라면, X<int>에는 TypeExpInfo를 만들지 않는다.
        [Fact]
        public void EvaluateType_CompositionOfTypeExp_OnlyWholeTypeExpAddedToDictionary()
        {
            var innerTypeExp = SIdTypeExp("X", SIntTypeExp());
            var typeExp = new MemberTypeExp(innerTypeExp, "Y", Arr<TypeExp>());

            var script = SScript(
                new TypeDeclScriptElement(new StructDecl(
                    AccessModifier.Public, "X", Arr(new TypeParam("T")), Arr<TypeExp>(), Arr<StructMemberDecl>(
                        new StructMemberTypeDecl(new StructDecl(
                            AccessModifier.Public, "Y", Arr<TypeParam>(), Arr<TypeExp>(), Arr<StructMemberDecl>()
                        ))
                    )
                )),
                new StmtScriptElement(new VarDeclStmt(new VarDecl(
                    false, typeExp, Arr(new VarDeclElement("x", null)))))
            );

            Evaluate(script);

            Assert.ThrowsAny<Exception>(() => innerTypeExp.Info);
        }
    }
}
