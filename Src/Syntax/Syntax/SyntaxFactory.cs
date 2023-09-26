using Citron.Collections;
using System.Linq;

using static Citron.Infra.Misc;

namespace Citron.Syntax
{
    public static class SyntaxFactory
    {
        public static StringExp SString(params StringExpElement[] elems)
            => new StringExp(Arr(elems));

        public static StringExp SString(string s)
            => new StringExp(Arr<StringExpElement>(new TextStringExpElement(s)));

        public static CommandStmt SCommand(params StringExp[] exps)
            => new CommandStmt(Arr(exps));

        public static EmbeddableStmt SEmbeddableBlankStmt()
        {
            return new EmbeddableStmt.Single(new BlankStmt());
        }

        public static VarDecl SVarDecl(TypeExp typeExp, string name, Exp? initExp = null)
        {
            return new VarDecl(typeExp, Arr(new VarDeclElement(name, initExp == null ? null : initExp)));
        }

        public static VarDeclStmt SVarDeclStmt(TypeExp typeExp, string name, Exp? initExp = null)
        {
            return new VarDeclStmt(SVarDecl(typeExp, name, initExp));
        }

        public static Script SScript(params Stmt[] stmts)
        {
            // void Main()
            // {
            //     stmts...
            // }
            return new Script(Arr<ScriptElement>(
                new GlobalFuncDeclScriptElement(
                    new GlobalFuncDecl(
                        null,
                        isSequence: false,
                        new IdTypeExp("void", default), "Main",
                        typeParams: default, parameters: default, stmts.ToImmutableArray()
                    )
                )
            ));
        }

        public static Script SScript(params ScriptElement[] elems)
            => new Script(Arr(elems));

        public static BlockStmt SBlock(params Stmt[] stmts)
            => new BlockStmt(Arr(stmts));

        public static Stmt SAssignStmt(Exp destExp, Exp srcExp)
        {
            return new ExpStmt(new BinaryOpExp(BinaryOpKind.Assign, destExp, srcExp));
        }

        public static IntLiteralExp SInt(int v) => new IntLiteralExp(v);
        public static BoolLiteralExp SBool(bool v) => new BoolLiteralExp(v);
        public static IdentifierExp SId(string name) => new IdentifierExp(name, default);
        public static IdentifierExp SId(string name, params TypeExp[] typeArgs) => new IdentifierExp(name, Arr(typeArgs));
        
        public static TypeExp SVarTypeExp() => new IdTypeExp("var", default);
        public static TypeExp SIntTypeExp() => new IdTypeExp("int", default);
        public static TypeExp SBoolTypeExp() => new IdTypeExp("bool", default);
        public static TypeExp SVoidTypeExp() => new IdTypeExp("void", default);
        public static TypeExp SStringTypeExp() => new IdTypeExp("string", default);
        public static TypeExp SNullableTypeExp(TypeExp innerTypeExp) => new NullableTypeExp(innerTypeExp);

        public static IdTypeExp SIdTypeExp(string name, params TypeExp[] typeArgs) => new IdTypeExp(name, Arr(typeArgs));
        public static MemberTypeExp Member(this TypeExp parent, string name, params TypeExp[] typeArgs) => new MemberTypeExp(parent, name, Arr(typeArgs));
        public static LocalPtrTypeExp SLocalPtrTypeExp(TypeExp innerTypeExp) => new LocalPtrTypeExp(innerTypeExp);
        public static BoxPtrTypeExp SBoxPtrTypeExp(TypeExp innerTypeExp) => new BoxPtrTypeExp(innerTypeExp);

        public static Exp Member(this Exp parent, string memberName, ImmutableArray<TypeExp> typeArgs = default)
        {
            return new MemberExp(parent, memberName, typeArgs);
        }

        public static ImmutableArray<FuncParam> SNormalFuncParams(params (TypeExp ParamType, string ParamName)[] ps)
        {
            var builder = ImmutableArray.CreateBuilder<FuncParam>(ps.Length);
            foreach (var p in ps)
                builder.Add(new FuncParam(HasOut: false, HasParams: false, p.ParamType, p.ParamName));
            return builder.MoveToImmutable();
        }

        public static ImmutableArray<Argument> SNormalArgs(params Exp[] exps)
        {
            var builder = ImmutableArray.CreateBuilder<Argument>(exps.Length);
            foreach (var exp in exps)
                builder.Add(new Argument(bOut: false, bParams: false, exp));
            return builder.MoveToImmutable();
        }

        public static CommandStmt SCommand(Exp exp)
        {
            return new CommandStmt(Arr(new StringExp(Arr<StringExpElement>(new ExpStringExpElement(exp)))));
        }

        public static ImmutableArray<Stmt> SBody(params Stmt[] stmts)
        {
            return Arr(stmts);
        }

        public static Exp SRefExp(Exp exp)
        {
            return new UnaryOpExp(UnaryOpKind.Ref, exp);
        }

        public static Exp SDerefExp(Exp exp)
        {
            return new UnaryOpExp(UnaryOpKind.Deref, exp);
        }

        public static GlobalFuncDeclScriptElement SMain(params Stmt[] body)
        {
            return new GlobalFuncDeclScriptElement(new GlobalFuncDecl(
                accessModifier: null, isSequence: false,
                SIntTypeExp(), "Main", typeParams: default, parameters: default,
                body.ToImmutableArray()
            ));
        }
    }
}
