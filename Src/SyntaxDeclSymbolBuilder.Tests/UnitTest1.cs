using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;
using Xunit;

using Citron.Analysis;
using System.Diagnostics;

namespace Citron;

public class UnitTest1
{
    // UnitOfWorkName_ScenarioName_ExpectedBehavior

    // public M.TypeId IntMType { get => new M.GlobalType("System.Runtime", new M.NamespacePath("System"), NormalName("Int32"), default); }

    SymbolFactory factory;

    ModuleDeclSymbol runtimeModuleDecl;
    StructType boolType, intType;
    ClassType stringType;
    VoidType voidType;

    M.Name NormalName(string text)
    {
        return new M.Name.Normal(text);
    }

    public UnitTest1()
    {
        factory = new SymbolFactory();

        runtimeModuleDecl = new ModuleDeclSymbol(NormalName("System.Runtime"), bReference: true);
        var systemNSDecl = new NamespaceDeclSymbol(runtimeModuleDecl, NormalName("System"));
        runtimeModuleDecl.AddNamespace(systemNSDecl);

        var boolDecl = new StructDeclSymbol(systemNSDecl, M.Accessor.Public, NormalName("Boolean"), typeParams: default);
        systemNSDecl.AddType(boolDecl);

        var intDecl = new StructDeclSymbol(systemNSDecl, M.Accessor.Public, NormalName("Int32"), typeParams: default);
        systemNSDecl.AddType(intDecl);

        var stringDecl = new ClassDeclSymbol(systemNSDecl, M.Accessor.Public, NormalName("String"), typeParams: default);
        systemNSDecl.AddType(stringDecl);

        var listDecl = new ClassDeclSymbol(systemNSDecl, M.Accessor.Public, NormalName("List"), Arr<M.Name>(NormalName("TItem")));
        systemNSDecl.AddType(listDecl);

        var runtimeModule = factory.MakeModule(runtimeModuleDecl);
        var systemNS = factory.MakeNamespace(runtimeModule, systemNSDecl);

        boolType = new StructType(factory.MakeStruct(systemNS, boolDecl, default));
        intType = new StructType(factory.MakeStruct(systemNS, intDecl, default));
        stringType = new ClassType(factory.MakeClass(systemNS, stringDecl, default));
        voidType = new VoidType();
    }

    ModuleDeclSymbol Build(M.Name moduleName, S.Script script)
    {
        var referenceModules = Arr(runtimeModuleDecl);

        // var logger = new TestLogger(true);
        return SyntaxDeclSymbolBuilder.Build(moduleName, Arr(script), referenceModules, factory);
    }

    [Fact]
    public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
    {
        // F<T>(T t)
        var paramTypes1 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Default, new M.TypeVarTypeId(0, "T"))));

        // F<U>(U u)
        var paramTypes2 = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Default, new M.TypeVarTypeId(0, "U"))));

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
            typeParams: Arr(new S.TypeParam("T")),
            parameters: Arr(new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "t")),
            body: Arr<S.Stmt>(new S.ReturnStmt(new S.ReturnValueInfo(true, new S.IdentifierExp("t", default))))
        )));

        var moduleName = NormalName("TestModule");
        var moduleDecl = Build(moduleName, script);
        Assert.NotNull(moduleDecl);
        Debug.Assert(moduleDecl != null);

        // expected
        var expectedModuleDecl = new ModuleDeclSymbol(moduleName, bReference: false);
        var funcDecl = new GlobalFuncDeclSymbol(expectedModuleDecl, M.Accessor.Private, NormalName("Func"), Arr(NormalName("T")));
        expectedModuleDecl.AddFunc(funcDecl);

        var typeVar = new TypeVarType(0);
        funcDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: false, typeVar),
            Arr(new FuncParameter(M.FuncParameterKind.Ref, typeVar, NormalName("t"))));
        
        Assert.Equal(expectedModuleDecl, moduleDecl);
    }

    [Fact]
    public void Build_FuncDecl_ModuleInfoHasFuncInfo()
    {
        // void Func<T, U>(int x, params U y, ref T z)
        var script = SScript(new S.GlobalFuncDeclScriptElement(new S.GlobalFuncDecl(
            null,
            isSequence: false,
            isRefReturn: false,
            SVoidTypeExp(),
            "Func",
            Arr(new S.TypeParam("T"), new S.TypeParam("U")),
            Arr(
                new S.FuncParam(S.FuncParamKind.Normal, SIntTypeExp(), "x"),
                new S.FuncParam(S.FuncParamKind.Params, new S.IdTypeExp("U", default), "y"),
                new S.FuncParam(S.FuncParamKind.Ref, new S.IdTypeExp("T", default), "z")
            ),
            Arr<S.Stmt>()
        )));

        var resultModuleDecl = Build(NormalName("TestModule"), script);

        var expectedModuleDecl = new ModuleDeclSymbol(NormalName("TestModule"), bReference: false);
        var funcDecl = new GlobalFuncDeclSymbol(expectedModuleDecl, M.Accessor.Private, NormalName("Func"), Arr(NormalName("T"), NormalName("U")));

        var t = new TypeVarType(0);
        var u = new TypeVarType(1);

        funcDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: false, voidType),
            Arr(
                new FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x")),
                new FuncParameter(M.FuncParameterKind.Params, u, NormalName("y")),
                new FuncParameter(M.FuncParameterKind.Ref, t, NormalName("z"))
            )
        );
        expectedModuleDecl.AddFunc(funcDecl);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedModuleDecl, resultModuleDecl));
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
                Arr(new S.TypeParam("T")),
                Arr<S.TypeExp>(new S.IdTypeExp("B", Arr(SIntTypeExp()))),
                Arr<S.StructMemberDecl>(
                    new S.StructMemberFuncDecl(
                        S.AccessModifier.Private,
                        IsStatic: true,
                        IsSequence: false,
                        IsRefReturn: false,
                        new S.IdTypeExp("T", default),
                        "Func",
                        Arr(new S.TypeParam("T"), new S.TypeParam("U")),
                        Arr(
                            new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("S", SIntTypeExp()), "s"),
                            new S.FuncParam(S.FuncParamKind.Normal, SIdTypeExp("U"), "u")
                        ),
                        default
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
                Arr(new S.TypeParam("T")),
                default,
                default
            ))
        );

        var moduleName = NormalName("TestModule");
        var resultModuleDecl = Build(moduleName, script);

        var expectedModuleDecl = new ModuleDeclSymbol(moduleName, bReference: false);
        var sDecl = new StructDeclSymbol(expectedModuleDecl, M.Accessor.Public, NormalName("S"), Arr(NormalName("T")));
        expectedModuleDecl.AddType(sDecl);        

        var sFuncDecl = new StructMemberFuncDeclSymbol(sDecl, M.Accessor.Private, bStatic: false, NormalName("Func"), Arr(NormalName("T"), NormalName("U")));
        sDecl.AddFunc(sFuncDecl);

        var sFuncXDecl = new StructMemberVarDeclSymbol(sDecl, M.Accessor.Protected, bStatic: false, intType, NormalName("x"));
        sDecl.AddMemberVar(sFuncXDecl);

        var sFuncYDecl = new StructMemberVarDeclSymbol(sDecl, M.Accessor.Protected, bStatic: false, intType, NormalName("y"));
        sDecl.AddMemberVar(sFuncYDecl);

        var sFuncConstructorDecl = new StructConstructorDeclSymbol(
            sDecl,
            M.Accessor.Public,
            Arr(
                new FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x")),
                new FuncParameter(M.FuncParameterKind.Default, intType, NormalName("y"))
            ),
            bTrivial: true
        );
        sDecl.AddConstructor(sFuncConstructorDecl);

        var bDecl = new StructDeclSymbol(expectedModuleDecl, M.Accessor.Private, NormalName("B"), Arr(NormalName("T")));
        expectedModuleDecl.AddType(bDecl);

        var sFuncT = new TypeVarType(0);
        var sFuncU = new TypeVarType(1);

        var module = factory.MakeModule(expectedModuleDecl);
        var s_Int = new StructType(factory.MakeStruct(module, sDecl, Arr<IType>(intType)));

        sFuncDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: false, sFuncT),
            Arr(
                new FuncParameter(M.FuncParameterKind.Default, s_Int, NormalName("s")),
                new FuncParameter(M.FuncParameterKind.Default, sFuncU, NormalName("u"))
            )
        );

        var b_Int = new StructType(factory.MakeStruct(module, bDecl, Arr<IType>(intType)));
        sDecl.InitBaseTypes(b_Int, interfaces: default);

        Assert.Equal(expectedModuleDecl, resultModuleDecl);
    }

    // From TypeEvaluator tests
    [Fact]
    public void EvaluateType_TopLevelStmt_WorksProperly()
    {
        S.TypeExp intTypeExp;

        var script = SScript(SVarDeclStmt(intTypeExp = SIntTypeExp(), "x"));
        var _ = Build(NormalName("TestModule"), script);

        var resultType = intTypeExp.GetType() as IType;
        Assert.Equal(intType, resultType);        
    }

    // 중첩된 TypeExp는 가장 최상위에만 Info를 붙인다
    // TypeExp 자체는 Type이 아닐 수 있기 때문이다. TypeExpInfo는 타입에만 유효하다
    // 예) X<int>.Y<short> 라면, X<int>에는 TypeExpInfo를 만들지 않는다.
    [Fact]
    public void EvaluateType_CompositionOfTypeExp_OnlyWholeTypeExpAddedToDictionary()
    {
        var innerTypeExp = SIdTypeExp("X", SIntTypeExp());
        var typeExp = new S.MemberTypeExp(innerTypeExp, "Y", Arr<S.TypeExp>());

        var script = SScript(
            new S.TypeDeclScriptElement(new S.StructDecl(
                S.AccessModifier.Public, "X", Arr(new S.TypeParam("T")), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>(
                    new S.StructMemberTypeDecl(new S.StructDecl(
                        S.AccessModifier.Public, "Y", Arr<S.TypeParam>(), Arr<S.TypeExp>(), Arr<S.StructMemberDecl>()
                    ))
                )
            )),
            new S.StmtScriptElement(new S.VarDeclStmt(new S.VarDecl(
                false, typeExp, Arr(new S.VarDeclElement("x", null)))))
        );

        Build(NormalName("TestModule"), script);
        Assert.ThrowsAny<Exception>(() => innerTypeExp.GetType());
    }
}