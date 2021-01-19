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
        TypeExpTypeValueService Evaluate(S.Script script)
        {
            var errorCollector = new TestErrorCollector(true);

            var skelRepo = TypeSkeletonCollector.Collect(script);            

            var moduleInfoRepo = new ModuleInfoRepository(Array.Empty<IModuleInfo>());
            var result = TypeExpEvaluator.Evaluate(script, moduleInfoRepo, skelRepo, errorCollector);

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
                new S.Script.StmtElement(new S.VarDeclStmt(new S.VarDecl(
                    intTypeExp, new S.VarDeclElement("x", null)
                )))
            );

            var result = Evaluate(script);

            var typeValue = result.GetTypeValue(intTypeExp);

            Assert.Equal(TypeValues.Int, typeValue);
        }

        // 먼저 해서 좋은 이유는?
        // 1. 다른거 할때 같이 해도 되니 코드가 비슷해지지 않을까
        // 2. 
        // 안좋은 이유는
        // 2. x.F 처럼 Analyzer단계에서 함수 검색은 또 해야한다, 근데 어차피 모든 함수는 오버로딩 선택을 해야 하므로
        
        // IdExp, MemberExp가 {Type, Func, Var}가 될 수 있다.
        // 1. Type  (GlobalType(X), MemberType(X<>.Y.Z<>), TypeVar T) // 두개는 구분할 필요가 없다 Type(TypeValue.Normal)
        // 2. Funcs (GlobalFunc(F), MemberFunc(T.F))       // 두개는 구분할 필요가 없다 Func(FuncValue) // 인데 ParamHash가 없는 상태
        // 3. Var   (GlobalVar(N.x), LocalVar(x), LocalVarOutsideLambda(x), StaticVar(N.T.x), MemberVar(x))
        //         VarValue
        // 4. x.F는 처리를 안하는 것으로

        // IdentifierExp가 Type을 지칭하는 경우, IdExp -> TypeValue매핑
        [Fact]
        public void TypeEvaluate_IdExpIsTypeExp_MakesIdExpToTypeValueMapping()
        {
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

            var result = Evaluate(script);

            var typeValue = result.GetTypeValue(exp);
            var expected = new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, 
                new AppliedItemPathEntry(
                    "S", string.Empty, new[] { new TypeValue.Normal(ModuleName.Internal, NamespacePath.Root, new AppliedItemPathEntry("X")) }));

            Debug.Assert(typeValue != null);
            Assert.Equal(expected, typeValue, ModuleInfoEqualityComparer.Instance);
        }        
    }
}
