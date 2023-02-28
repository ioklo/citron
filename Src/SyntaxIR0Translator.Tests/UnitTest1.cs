using Citron.Infra;
using Citron.Symbol;
using Citron.Collections;

using S = Citron.Syntax;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;
using Xunit;

using Citron.Analysis;
using System.Diagnostics;
using Citron.IR0;
using Citron.Test.Misc;

namespace Citron;

public class UnitTest1
{
    // UnitOfWorkName_ScenarioName_ExpectedBehavior

    // public TypeId IntMType { get => new GlobalType("System.Runtime", new NamespacePath("System"), NormalName("Int32"), default); }

    SymbolFactory factory;

    ModuleDeclSymbol runtimeModuleDecl;
    StructType boolType, intType;
    ClassType stringType;
    VoidType voidType;

    Name NormalName(string text)
    {
        return new Name.Normal(text);
    }

    public UnitTest1()
    {
        factory = new SymbolFactory();

        runtimeModuleDecl = new ModuleDeclSymbol(NormalName("System.Runtime"), bReference: true);
        var systemNSDecl = new NamespaceDeclSymbol(runtimeModuleDecl, NormalName("System"));
        runtimeModuleDecl.AddNamespace(systemNSDecl);

        var boolDecl = new StructDeclSymbol(systemNSDecl, Accessor.Public, NormalName("Boolean"), typeParams: default);
        systemNSDecl.AddType(boolDecl);

        var intDecl = new StructDeclSymbol(systemNSDecl, Accessor.Public, NormalName("Int32"), typeParams: default);
        systemNSDecl.AddType(intDecl);

        var stringDecl = new ClassDeclSymbol(systemNSDecl, Accessor.Public, NormalName("String"), typeParams: default);
        systemNSDecl.AddType(stringDecl);

        var listDecl = new ClassDeclSymbol(systemNSDecl, Accessor.Public, NormalName("List"), Arr<Name>(NormalName("TItem")));
        systemNSDecl.AddType(listDecl);

        var runtimeModule = factory.MakeModule(runtimeModuleDecl);
        var systemNS = factory.MakeNamespace(runtimeModule, systemNSDecl);

        boolType = new StructType(factory.MakeStruct(systemNS, boolDecl, default));
        intType = new StructType(factory.MakeStruct(systemNS, intDecl, default));
        stringType = new ClassType(factory.MakeClass(systemNS, stringDecl, default));
        voidType = new VoidType();
    }

    (ModuleDeclSymbol, ImmutableArray<StmtBody>) Build(Name moduleName, S.Script script)
    {
        var referenceModules = Arr(runtimeModuleDecl);

        var logger = new TestLogger(true);
        return SyntaxIR0Translator.Build(moduleName, Arr(script), referenceModules, factory, logger);
    }

    //[Fact]
    //public void ParamHash_TypeVarDifferentNameSameLocation_SameParamHash()
    //{
    //    // F<T>(T t)
    //    var paramTypes1 = new ParamTypes(Arr(new ParamKindAndType(ParamKind.Default, new TypeVarTypeId(0, "T"))));

    //    // F<U>(U u)
    //    var paramTypes2 = new ParamTypes(Arr(new ParamKindAndType(ParamKind.Default, new TypeVarTypeId(0, "U"))));

    //    Assert.Equal(paramTypes1, paramTypes2);
    //}

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
        var (moduleDecl, _) = Build(moduleName, script);

        // expected
        var expectedModuleDecl = new ModuleDeclSymbol(moduleName, bReference: false);
        var funcDecl = new GlobalFuncDeclSymbol(expectedModuleDecl, Accessor.Private, NormalName("Func"), Arr(NormalName("T")));

        var typeVar = new TypeVarType(0, NormalName("T"));
        funcDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: true, typeVar),
            Arr(new FuncParameter(FuncParameterKind.Ref, typeVar, NormalName("t"))));

        expectedModuleDecl.AddFunc(funcDecl);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedModuleDecl, moduleDecl));
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

        var (resultModuleDecl, _) = Build(NormalName("TestModule"), script);

        var expectedModuleDecl = new ModuleDeclSymbol(NormalName("TestModule"), bReference: false);
        var funcDecl = new GlobalFuncDeclSymbol(expectedModuleDecl, Accessor.Private, NormalName("Func"), Arr(NormalName("T"), NormalName("U")));

        var t = new TypeVarType(0, NormalName("T"));
        var u = new TypeVarType(1, NormalName("U"));

        funcDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: false, voidType),
            Arr(
                new FuncParameter(FuncParameterKind.Default, intType, NormalName("x")),
                new FuncParameter(FuncParameterKind.Params, u, NormalName("y")),
                new FuncParameter(FuncParameterKind.Ref, t, NormalName("z"))
            )
        );
        expectedModuleDecl.AddFunc(funcDecl);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedModuleDecl, resultModuleDecl));
    }

    // public struct S<T> : B<int>
    // {
    //     private static T Func<T, U>(S<int> s, U u) { }
    //     int x, y;
    // }
    // struct B<T> { }
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
                        null,
                        SIntTypeExp(),
                        Arr("x", "y")
                    )
                )
            )),

            new S.TypeDeclScriptElement(new S.StructDecl(
                null,
                "B",
                Arr(new S.TypeParam("T")),
                default,
                default
            ))
        );

        var moduleName = NormalName("TestModule");
        var (resultModuleDecl, _) = Build(moduleName, script);

        var expectedModuleDecl = new ModuleDeclSymbol(moduleName, bReference: false);
        var sDecl = new StructDeclSymbol(expectedModuleDecl, Accessor.Public, NormalName("S"), Arr(NormalName("T")));
        expectedModuleDecl.AddType(sDecl);        

        var sFuncDecl = new StructMemberFuncDeclSymbol(sDecl, Accessor.Private, bStatic: true, NormalName("Func"), Arr(NormalName("T"), NormalName("U")));

        var sFuncXDecl = new StructMemberVarDeclSymbol(sDecl, Accessor.Public, bStatic: false, intType, NormalName("x"));
        sDecl.AddMemberVar(sFuncXDecl);

        var sFuncYDecl = new StructMemberVarDeclSymbol(sDecl, Accessor.Public, bStatic: false, intType, NormalName("y"));
        sDecl.AddMemberVar(sFuncYDecl);

        var sConstructorDecl = new StructConstructorDeclSymbol(
            sDecl,
            Accessor.Public,
            Arr(
                new FuncParameter(FuncParameterKind.Default, intType, NormalName("x")),
                new FuncParameter(FuncParameterKind.Default, intType, NormalName("y"))
            ),
            bTrivial: true
        );
        sDecl.AddConstructor(sConstructorDecl);

        var bDecl = new StructDeclSymbol(expectedModuleDecl, Accessor.Private, NormalName("B"), Arr(NormalName("T")));
        bDecl.InitBaseTypes(null, interfaces: default);
        expectedModuleDecl.AddType(bDecl);

        var bConstructorDecl = new StructConstructorDeclSymbol(bDecl, Accessor.Public, parameters: default, bTrivial: true);
        bDecl.AddConstructor(bConstructorDecl);

        var sFuncT = new TypeVarType(1, NormalName("T"));
        var sFuncU = new TypeVarType(2, NormalName("U"));

        var module = factory.MakeModule(expectedModuleDecl);
        var s_Int = new StructType(factory.MakeStruct(module, sDecl, Arr<IType>(intType)));

        sFuncDecl.InitFuncReturnAndParams(
            new FuncReturn(IsRef: false, sFuncT),
            Arr(
                new FuncParameter(FuncParameterKind.Default, s_Int, NormalName("s")),
                new FuncParameter(FuncParameterKind.Default, sFuncU, NormalName("u"))
            )
        );

        sDecl.AddFunc(sFuncDecl);

        var b_Int = new StructType(factory.MakeStruct(module, bDecl, Arr<IType>(intType)));
        sDecl.InitBaseTypes(b_Int, interfaces: default);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedModuleDecl, resultModuleDecl));
    }

    [Fact]
    public void EvaluateType_TypeVarTypeExpInDeclSpace_MakeOpenSymbol()
    {
        // class X<T> { class Y { T t; } } // t의 타입은
        var syntax = SScript(new S.TypeDeclScriptElement(new S.ClassDecl(null, "X", Arr(new S.TypeParam("T")), baseTypes: default,
            Arr<S.ClassMemberDecl>(
                new S.ClassMemberTypeDecl(new S.ClassDecl(null, "Y", typeParams: default, baseTypes: default,
                    Arr<S.ClassMemberDecl>(
                        new S.ClassMemberVarDecl(null, new S.IdTypeExp("T", TypeArgs: default), Arr("t"))
                    )
                ))
            )
        )));

        var (resultDeclSymbol, _) = Build(NormalName("TestModule"), syntax);

        var expectedDeclSymbol = new ModuleDeclSymbol(NormalName("TestModule"), bReference: false);

        var xDecl = new ClassDeclSymbol(expectedDeclSymbol, Accessor.Private, NormalName("X"), Arr(NormalName("T")));
        xDecl.InitBaseTypes(null, interfaces: default);
        expectedDeclSymbol.AddType(xDecl);

        var xConstructorDecl = new ClassConstructorDeclSymbol(xDecl, Accessor.Public, parameters: default, bTrivial: true);
        xDecl.AddConstructor(xConstructorDecl);

        var xyDecl = new ClassDeclSymbol(xDecl, Accessor.Private, NormalName("Y"), typeParams: default);
        xyDecl.InitBaseTypes(null, interfaces: default);
        xDecl.AddType(xyDecl);

        var xyConstructorDecl = new ClassConstructorDeclSymbol(xyDecl, Accessor.Public, Arr(new FuncParameter(FuncParameterKind.Default, new TypeVarType(0, NormalName("T")), NormalName("t"))), bTrivial: true);
        xyDecl.AddConstructor(xyConstructorDecl);

        var xytDecl = new ClassMemberVarDeclSymbol(xyDecl, Accessor.Private, bStatic: false, new TypeVarType(0, NormalName("T")), NormalName("t"));
        xyDecl.AddMemberVar(xytDecl);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedDeclSymbol, resultDeclSymbol));
    }

    // X<Y>.Y => X<X<T>.Y>.Y  // 그거랑 별개로 인자로 들어온 것들은 적용을 시켜야 한다   
    [Fact]
    public void EvaluateType_InstantiatedTypeExpInDeclSpace_MakeOpenSymbol()
    {
        // class X<T> { class Y { X<Y>.Y t; } } // t의 타입은 X<X<T>.Y>.Y
        var syntax = SScript(new S.TypeDeclScriptElement(new S.ClassDecl(null, "X", Arr(new S.TypeParam("T")), baseTypes: default,
            Arr<S.ClassMemberDecl>(
                new S.ClassMemberTypeDecl(new S.ClassDecl(null, "Y", typeParams: default, baseTypes: default,
                    Arr<S.ClassMemberDecl>(
                        new S.ClassMemberVarDecl(null, 
                            new S.MemberTypeExp(
                                new S.IdTypeExp("X", Arr<S.TypeExp>(new S.IdTypeExp("Y", TypeArgs: default))),
                                "Y", TypeArgs: default
                            ),
                            Arr("t")
                        )
                    )
                ))
            )
        )));

        var (resultDeclSymbol, _) = Build(NormalName("TestModule"), syntax);

        var expectedDeclSymbol = new ModuleDeclSymbol(NormalName("TestModule"), bReference: false);

        var xDecl = new ClassDeclSymbol(expectedDeclSymbol, Accessor.Private, NormalName("X"), Arr(NormalName("T")));
        xDecl.InitBaseTypes(null, interfaces: default);
        expectedDeclSymbol.AddType(xDecl);

        var xConstructorDecl = new ClassConstructorDeclSymbol(xDecl, Accessor.Public, parameters: default, bTrivial: true);
        xDecl.AddConstructor(xConstructorDecl);

        var xyDecl = new ClassDeclSymbol(xDecl, Accessor.Private, NormalName("Y"), typeParams: default);
        xyDecl.InitBaseTypes(null, interfaces: default);
        xDecl.AddType(xyDecl);

        // X<X<T>.Y>.Y

        // X<T>
        var expectedModule = factory.MakeModule(expectedDeclSymbol);
        var x_T = factory.MakeClass(expectedModule, xDecl, Arr<IType>(new TypeVarType(0, NormalName("T"))));

        // X<T>.Y
        var x_Ty = factory.MakeClass(x_T, xyDecl, typeArgs: default);

        // X<X<T>.Y>
        var x_x_Ty = factory.MakeClass(expectedModule, xDecl, Arr<IType>(new ClassType(x_Ty)));

        // X<X<T>.Y>.Y
        var x_x_Tyy = factory.MakeClass(x_x_Ty, xyDecl, typeArgs: default);

        var xytDecl = new ClassMemberVarDeclSymbol(xyDecl, Accessor.Private, bStatic: false, new ClassType(x_x_Tyy), NormalName("t"));
        xyDecl.AddMemberVar(xytDecl);

        var xyConstructorDecl = new ClassConstructorDeclSymbol(xyDecl, Accessor.Public, Arr(new FuncParameter(FuncParameterKind.Default, new ClassType(x_x_Tyy), NormalName("t"))), bTrivial: true);
        xyDecl.AddConstructor(xyConstructorDecl);

        var context = new CyclicEqualityCompareContext();
        Assert.True(context.CompareClass(expectedDeclSymbol, resultDeclSymbol));
    }
}