using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

using Citron.Infra;
using Citron.Test.Misc;
using Citron.Symbol;
using Citron.Test;
using Citron.Analysis;

using Xunit;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Syntax.SyntaxFactory;
using static Citron.Test.SyntaxIR0TranslatorMisc;
using static Citron.Infra.Misc;
using Citron.Collections;
using System.Text.Json;

namespace Citron.Test
{
    public class TranslatorTests
    {
        Name moduleName;
        SymbolFactory factory;
        R.IR0Factory r;
        ImmutableArray<ModuleDeclSymbol> refModuleDecls;

        public TranslatorTests()
        {
            moduleName = NormalName("TestModule");
            factory = new SymbolFactory();

            // runtime module
            var runtimeModuleDSymbol = new ModuleDeclSymbol(NormalName("System.Runtime"), bReference: true);
            var runtimeModuleSymbol = factory.MakeModule(runtimeModuleDSymbol);

            // system namespace
            var systemNSDSymbol = new NamespaceDeclSymbol(runtimeModuleDSymbol, NormalName("System"));
            runtimeModuleDSymbol.AddNamespace(systemNSDSymbol);            
            var systemNSSymbol = factory.MakeNamespace(runtimeModuleSymbol, systemNSDSymbol);

            IType MakeBoolType()
            {
                var boolDSymbol = new StructDeclSymbol(systemNSDSymbol, Accessor.Public, NormalName("Boolean"), typeParams: default);
                boolDSymbol.InitBaseTypes(baseStruct: null, interfaces: default);
                systemNSDSymbol.AddType(boolDSymbol);

                ITypeSymbol boolSymbol = factory.MakeStruct(systemNSSymbol, boolDSymbol, typeArgs: default);
                return boolSymbol.MakeType();
            }
            var boolType = MakeBoolType();

            IType MakeIntType()
            {
                var intDSymbol = new StructDeclSymbol(systemNSDSymbol, Accessor.Public, NormalName("Int32"), typeParams: default);
                intDSymbol.InitBaseTypes(baseStruct: null, interfaces: default);
                systemNSDSymbol.AddType(intDSymbol);
                ITypeSymbol intSymbol = factory.MakeStruct(systemNSSymbol, intDSymbol, typeArgs: default);
                return intSymbol.MakeType();
            }
            var intType = MakeIntType();

            IType MakeStringType()
            {
                var stringDSymbol = new ClassDeclSymbol(systemNSDSymbol, Accessor.Public, NormalName("String"), typeParams: default);
                stringDSymbol.InitBaseTypes(baseClass: null, interfaces: default);
                systemNSDSymbol.AddType(stringDSymbol);
                ITypeSymbol stringSymbol = factory.MakeClass(systemNSSymbol, stringDSymbol, typeArgs: default);
                return stringSymbol.MakeType();
            }
            var stringType = MakeStringType();

            R.IR0Factory.ListTypeConstructor MakeListTypeConstructor()
            {
                var listDSymbol = new ClassDeclSymbol(systemNSDSymbol, Accessor.Public, NormalName("List"), Arr(NormalName("TItem")));
                listDSymbol.InitBaseTypes(baseClass: null, interfaces: default); // 일단;
                systemNSDSymbol.AddType(listDSymbol);
                return itemType =>
                {
                    return ((ITypeSymbol)factory.MakeClass(systemNSSymbol, listDSymbol, Arr(itemType))).MakeType();
                };
            }
            var listTypeConstructor = MakeListTypeConstructor();

            r = new R.IR0Factory(boolType, intType, stringType, listTypeConstructor);
            refModuleDecls = Arr(runtimeModuleDSymbol);
        }

        R.Script? Translate(S.Script syntaxScript, bool raiseAssertFailed = true)
        {
            var testLogger = new TestLogger(raiseAssertFailed);
            var factory = new SymbolFactory();
            return SyntaxIR0Translator.Build(moduleName, Arr(syntaxScript), refModuleDecls, factory, testLogger);
        }
        

        // Trivial Cases
        [Fact]
        public void CommandStmt_TranslatesTrivially()
        {
            var syntaxCmdStmt = SCommand(
                SString(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(SString("World"))));

            var syntaxScript = SScript(syntaxCmdStmt);

            var script = Translate(syntaxScript);

            var expectedStmt = r.Command(
                r.String(
                    r.TextElem("Hello "),
                    r.ExpElem(r.String("World"))));

            var expected = r.Script(moduleName, Arr<R.Stmt>(expectedStmt));            
            
            AssertEquals(expected.ModuleDeclSymbol, script.ModuleDeclSymbol);
            Assert.Equal(expected.StmtBodies, script.StmtBodies);
        }

        [Fact]
        public void ModuleDeclSymbol_Serialize()
        {
            var syntaxCmdStmt = SCommand(
                SString(
                    new S.TextStringExpElement("Hello "),
                    new S.ExpStringExpElement(SString("World"))));

            var syntaxScript = SScript(syntaxCmdStmt);

            var script = Translate(syntaxScript);

            var serializeContext = new SerializeContext();
            var text = SerializeContext.Serialize(script.ModuleDeclSymbol);
        }
    }
}
