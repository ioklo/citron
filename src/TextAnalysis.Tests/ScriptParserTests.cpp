#include "pch.h"

#include <Syntax/ScriptSyntax.h>
#include <Syntax/StringExpSyntaxElements.h>
#include <Syntax/GlobalFuncDeclSyntax.h>
#include <Syntax/NamespaceDeclSyntaxElements.h>
#include <TextAnalysis/ScriptParser.h>

#include <Syntax/ExpSyntaxes.h>



#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(ScriptParser, ParseSimpleScript)
{
    auto [buffer, lexer] = Prepare(UR"---(
void Main()
{
    @ls -al
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = SScript({
        CommandStmtSyntax({
            StringExpSyntax({ TextStringExpSyntaxElement{U"ls -al"} })
        })
    });

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseFuncDecl)
{
    auto [buffer, lexer] = Prepare(U"void Func(int x, string y, params int z) { int a = 0; }");

    auto oScript = ParseScript(&lexer);

    auto expected = SScript({ GlobalFuncDeclScriptSyntaxElement(GlobalFuncDeclSyntax(
        nullopt,
        false,
        SVoidTypeExp(),
        U"Func", {},
        {
            FuncParamSyntax{false, false, IdTypeExpSyntax(U"int"), U"x"},
            FuncParamSyntax{false, false, IdTypeExpSyntax(U"string"), U"y"},
            FuncParamSyntax{false, true, IdTypeExpSyntax(U"int"), U"z"}
        },
        {
            VarDeclStmtSyntax(
                VarDeclSyntax{
                    IdTypeExpSyntax(U"int"),
                    {
                        VarDeclSyntaxElement{ U"a", IntLiteralExpSyntax(0) }
                    }
                }
            )
        }
    ))});

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseNamespaceDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(
namespace NS1
{
    namespace NS2.NS3
    {
        void F()
        {

        }
    }
}
)---");

    auto oScript = ParseScript(&lexer);    

    auto expected = SScript({ NamespaceDeclScriptSyntaxElement(NamespaceDeclSyntax(
        {U"NS1"},
        {
            NamespaceDeclNamespaceDeclSyntaxElement(NamespaceDeclSyntax(
                { U"NS2", U"NS3" },
                {
                    GlobalFuncDeclNamespaceDeclSyntaxElement(GlobalFuncDeclSyntax(
                        nullopt,
                        false,
                        SVoidTypeExp(),
                        U"F",
                        {},
                        {},
                        {}
                    ))
                }
            ))
        }
    )) });

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseEnumDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(
enum X
{
    First,
    Second(int i),
    Third
}
)---");

    auto oScript = ParseScript(&lexer);

    auto expected = SScript({ TypeDeclScriptSyntaxElement(EnumDeclSyntax(nullopt, U"X",
        {},
        {
            EnumElemDeclSyntax(U"First", {}),
            EnumElemDeclSyntax(U"Second", { EnumElemMemberVarDeclSyntax(SIntTypeExp(), U"i")}),
            EnumElemDeclSyntax(U"Third", {})
        }
    )) });

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseStructDeclAsync)
{
    auto [buffer, lexer] = Prepare(UR"---(
public struct S<T> : B, I
{
    int x1;
    public int x2;
    protected string y;
    private int z;

    public struct Nested<U> : B, I { int x; }

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
}
)---");

    auto oScript = ParseScript(&lexer);

    auto expected = SScript({ TypeDeclScriptSyntaxElement(StructDeclSyntax(
        AccessModifierSyntax::Public,

        U"S",

        { TypeParamSyntax{U"T"} },

        { SIdTypeExp(U"B"), SIdTypeExp(U"I") },

        {
            StructMemberVarDeclSyntax(nullopt, SIdTypeExp(U"int"), { U"x1" }),
            StructMemberVarDeclSyntax(AccessModifierSyntax::Public, SIdTypeExp(U"int"), { U"x2" }),
            StructMemberVarDeclSyntax(AccessModifierSyntax::Protected, SIdTypeExp(U"string"),{ U"y" }),
            StructMemberVarDeclSyntax(AccessModifierSyntax::Private, SIdTypeExp(U"int"), { U"z" }),

            StructMemberTypeDeclSyntax(StructDeclSyntax(
                AccessModifierSyntax::Public,
                U"Nested",
                { TypeParamSyntax{U"U"} }, { SIdTypeExp(U"B"), SIdTypeExp(U"I") },
                { StructMemberVarDeclSyntax(nullopt, SIdTypeExp(U"int"), { U"x" }) }
            )),

            StructMemberFuncDeclSyntax(
                nullopt,
                true,
                false,
                SVoidTypeExp(),
                U"Func",
                { TypeParamSyntax{U"X"} },
                { FuncParamSyntax{false, false, SIdTypeExp(U"string"), U"s"} },
                {}
            ),

            StructMemberFuncDeclSyntax(
                AccessModifierSyntax::Private,
                false,
                true,
                SIntTypeExp(),
                U"F2",
                {TypeParamSyntax{U"T"}},
                {},
                {YieldStmtSyntax(IntLiteralExpSyntax(4))}
            )
        }
    )) });

    EXPECT_SYNTAX_EQ(oScript, expected);
}


TEST(ScriptParser, ParseComplexScriptAsync)
{
    auto [buffer, lexer] = Prepare(UR"---(
void Main()
{
    int sum = 0;

    for (int i = 0; i < 5; i++)
    {
        if (i % 2 == 0)
            sum = sum + i;
        else @{
            echo hi 
        }
    }

    @echo $sum Completed!
}
)---");

    auto oScript = ParseScript(&lexer);

    auto expected = SScript({
        SVarDeclStmt(SIdTypeExp(U"int"), U"sum", IntLiteralExpSyntax(0)),

        ForStmtSyntax(
            VarDeclForStmtInitializerSyntax{ SVarDecl(SIdTypeExp(U"int"), U"i", IntLiteralExpSyntax(0)) },
            BinaryOpExpSyntax(BinaryOpSyntaxKind::LessThan, SId(U"i"), IntLiteralExpSyntax(5)),
            UnaryOpExpSyntax(UnaryOpSyntaxKind::PostfixInc, SId(U"i")),
            BlockEmbeddableStmtSyntax({
                IfStmtSyntax(
                    BinaryOpExpSyntax(BinaryOpSyntaxKind::Equal,
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Modulo, SId(U"i"), IntLiteralExpSyntax(2)),
                        IntLiteralExpSyntax(0)),
                    SingleEmbeddableStmtSyntax(ExpStmtSyntax(
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
                            SId(U"sum"),
                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Add, SId(U"sum"), SId(U"i"))))),
                    SingleEmbeddableStmtSyntax(CommandStmtSyntax({StringExpSyntax(U"            echo hi ")}))
                )
            })
        ),
        CommandStmtSyntax({StringExpSyntax({
            TextStringExpSyntaxElement{U"echo "},
            ExpStringExpSyntaxElement{SId(U"sum")},
            TextStringExpSyntaxElement{U" Completed!"}
        })})
    });

    EXPECT_SYNTAX_EQ(oScript, expected);
}