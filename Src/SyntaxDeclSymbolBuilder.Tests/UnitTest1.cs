using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;
using M = Citron.Module;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;
using Xunit;
using Citron.Test;
using Citron.Analysis;

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

    M.Name.Normal NormalName(string text)
    {
        return new M.Name.Normal(text);
    }

    public UnitTest1()
    {
        factory = new SymbolFactory();

        runtimeModuleDecl = new ModuleDeclBuilder(factory, NormalName("System.Runtime"), bReference: false)
            .BeginNamespace("System")
                .Struct("Boolean", out var boolDecl)
                .Struct("Int32", out var intDecl)
                .Class("String", out var stringDecl)
                .BeginClass("List")
                    .TypeParam("TItem", out var _)
                .EndClass(out var listTypeDecl)
            .EndNamespace(out var systemNSDecl)
            .Make();

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
        var result = Build(moduleName, script);
        Assert.NotNull(result);
        Debug.Assert(result != null);

        // var paramTypes = new M.ParamTypes(Arr(new M.ParamKindAndType(M.ParamKind.Ref, new M.TypeVarTypeId(0, "T"))));
        var paramIds = Arr(new FuncParamId(M.FuncParameterKind.Ref, new TypeVarSymbolId(0)));
        var path = new DeclSymbolPath(null, NormalName("Func"), 1, paramIds);

        var resultDecl = result.GetDeclSymbol(path) as GlobalFuncDeclSymbol;
        Debug.Assert(resultDecl != null);

        // declSymbol을 얻어낼 수 있는 방법
        var _ = new ModuleDeclBuilder(factory, moduleName, bReference: false)
            .BeginGlobalFunc(M.Accessor.Private, NormalName("Func"), bInternal: true)
                .TypeParam("T", out var typeVarDecl)
                .FuncReturnHolder(out var funcRetHolder)
                .FuncParametersHolder(paramIds, out var funcParamsHolder)
            .EndGlobalFunc(out var globalFuncDecl)
            .Make();

        var typeVar = factory.MakeTypeVar(typeVarDecl);

        funcRetHolder.SetValue(new FuncReturn(isRef: false, typeVar));
        funcParamsHolder.SetValue(Arr(new FuncParameter(M.FuncParameterKind.Ref, typeVar, NormalName("t"))));

        Assert.Equal(globalFuncDecl, resultDecl);
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

        var result = Build(NormalName("TestModule"), script);

        var paramIds = Arr(
            new FuncParamId(M.FuncParameterKind.Default, SymbolId.Int),
            new FuncParamId(M.FuncParameterKind.Params, new TypeVarSymbolId(1)),
            new FuncParamId(M.FuncParameterKind.Ref, new TypeVarSymbolId(0))
        );

        var path = new DeclSymbolPath(
            null,
            NormalName("Func"), 2,
            paramIds
        );

        var resultDecl = result.GetDeclSymbol(path) as GlobalFuncDeclSymbol;
        Debug.Assert(resultDecl != null);

        // expected 만들기
        var _ = new ModuleDeclBuilder(factory, NormalName("TestModule"), bReference: false)
            .BeginGlobalFunc(M.Accessor.Private, NormalName("Func"), true)
                .TypeParam("T", out var tDecl)
                .TypeParam("U", out var uDecl)
                .FuncReturn(isRef: false, voidType)
                .FuncParametersHolder(paramIds, out var paramsHolder)
            .EndGlobalFunc(out var expectedGlobalFuncDecl)
            .Make();

        var t = factory.MakeTypeVar(tDecl);
        var u = factory.MakeTypeVar(uDecl);

        paramsHolder.SetValue(Arr(
            new FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x")),
            new FuncParameter(M.FuncParameterKind.Params, u, NormalName("y")),
            new FuncParameter(M.FuncParameterKind.Ref, t, NormalName("z"))
        ));

        Assert.Equal(expectedGlobalFuncDecl, resultDecl);
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
        var result = Build(moduleName, script);

        var s_intId = new ModuleSymbolId(moduleName, new SymbolPath(null, NormalName("S"), Arr(SymbolId.Int)));
        var uId = new TypeVarSymbolId(2);

        var funcParamIds = Arr(
            new FuncParamId(M.FuncParameterKind.Default, s_intId),
            new FuncParamId(M.FuncParameterKind.Default, uId)
        );

        // var trivialConstructor = new InternalModuleConstructorInfo(M.Accessor.Public, Arr(new M.Param(M.ParamKind.Default, IntMType, NormalName("x")), new M.Param(M.ParamKind.Default, IntMType, NormalName("y"))));

        var expected = new ModuleDeclBuilder(factory, moduleName, bReference: false)
            .BeginStruct(M.Accessor.Public, "S")
                .TypeParam("T", out var _)
                .BaseHolder(out var baseHolder) // B<int>

                // private static T Func<T, U>(S<int> s, U u) { }
                .BeginFunc(M.Accessor.Private, bStatic: false, NormalName("Func"))
                    .TypeParam("T", out var typeVarFuncTDecl)
                    .TypeParam("U", out var typeVarFuncUDecl)
                    .FuncReturnHolder(out var funcRetHolder)
                    .FuncParametersHolder(funcParamIds, out var funcParamsHolder)
                .EndFunc(out var _)
                .Var(M.Accessor.Protected, bStatic: false, intType.ToHolder(), NormalName("x"))
                .Var(M.Accessor.Protected, bStatic: false, intType.ToHolder(), NormalName("y"))
                .BeginConstructor(M.Accessor.Public, bTrivial: true)
                    .FuncParameter(M.FuncParameterKind.Default, intType, NormalName("x"))
                    .FuncParameter(M.FuncParameterKind.Default, intType, NormalName("y"))
                .EndConstructor(out var _)
            .EndStruct(out var expectedSDecl)
            .BeginStruct(M.Accessor.Private, "B")
                .TypeParam("T", out var _)
            .EndStruct(out var expectedBDecl)
            .Make();

        var expectedModule = factory.MakeModule(expected);
        var expectedB_int = factory.MakeStruct(expectedModule, expectedBDecl, Arr<ITypeSymbol>(intType));
        baseHolder.SetValue(expectedB_int);

        var expectedS_int = factory.MakeStruct(expectedModule, expectedSDecl, Arr<ITypeSymbol>(intType));

        var typeVarFuncT = factory.MakeTypeVar(typeVarFuncTDecl);
        var typeVarFuncU = factory.MakeTypeVar(typeVarFuncUDecl);
        funcRetHolder.SetValue(new FuncReturn(isRef: false, typeVarFuncT));
        funcParamsHolder.SetValue(Arr(
            new FuncParameter(M.FuncParameterKind.Default, expectedS_int, NormalName("s")),
            new FuncParameter(M.FuncParameterKind.Default, typeVarFuncU, NormalName("u"))
        ));

        // TODO: expected에 trivialConstructor를 강제로 꽂을 방법이 없어서 equals체크를 하면 실패한다
        //       1:1로 비교하면 되는데, 그건 그거대로 고통스럽다

        Assert.Equal(expected, result);
    }

    // From TypeEvaluator tests
    [Fact]
    public void EvaluateType_TopLevelStmt_WorksProperly()
    {
        TypeExp intTypeExp;

        var script = SScript(SVarDeclStmt(intTypeExp = SIntTypeExp(), "x"));

        Evaluate(script);

        var info = intTypeExp.GetTypeExpInfo();

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

        Assert.ThrowsAny<Exception>(() => innerTypeExp.GetType());
    }
}