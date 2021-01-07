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

            var skeletonCollector = new TypeSkeletonCollector(errorCollector);            
            var skelRepo = skeletonCollector.Collect(script);
            Debug.Assert(skelRepo != null);

            var moduleInfoRepo = new ModuleInfoRepository(Array.Empty<IModuleInfo>());
            var itemInfoRepo = new ItemInfoRepository(moduleInfoRepo);

            var evaluator = new TypeExpEvaluator(itemInfoRepo, skelRepo, errorCollector);

            var result = evaluator.EvaluateScript(script);

            Assert.NotNull(result);
            Debug.Assert(result != null);

            return result;
        }

        // UnitOfWorkName_ScenarioName_ExpectedBehavior

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

        // IdExp가 함수였을때 IdExp -> FuncValue매핑
        [Fact]
        public void TypeEvaluate_IdExpIsFunc_MakesIdExpToFuncValueMapping()
        {

        }

        [Fact]
        public void 

        // struct A<T> { struct B { } }
        // struct X {}
        // int x = A<X>.B.x; 
        // MemberExp(IdExp("A", ["X"]), "B") => TypeValue(A<X>.B)
        // IdExp("A", ["X"]) => TypeValue(A<X>) // TODO: 사실 이건 필요없을거 같다
        [Fact]
        public void TypeEvaluate_MemberExpIsTypeExp_MakesMemberExpToTypeValueMapping()
        {
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

            var result = Evaluate(script);

            var typeValue1 = result.GetTypeValue(exp1);
            var typeValue2 = result.GetTypeValue(exp2);

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
