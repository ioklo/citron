using System;
using System.Collections.Generic;

namespace Gum.Syntax
{
    public abstract class ScriptElement
    {
    }    

    public class FuncDeclScriptElement : ScriptElement
    {
        public FuncDecl FuncDecl { get; }
        public FuncDeclScriptElement(FuncDecl funcDecl)
        {
            FuncDecl = funcDecl;
        }

        public override bool Equals(object? obj)
        {
            return obj is FuncDeclScriptElement element &&
                   EqualityComparer<FuncDecl>.Default.Equals(FuncDecl, element.FuncDecl);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FuncDecl);
        }

        public static bool operator ==(FuncDeclScriptElement? left, FuncDeclScriptElement? right)
        {
            return EqualityComparer<FuncDeclScriptElement?>.Default.Equals(left, right);
        }

        public static bool operator !=(FuncDeclScriptElement? left, FuncDeclScriptElement? right)
        {
            return !(left == right);
        }
    }

    public class StmtScriptElement : ScriptElement
    {
        public Stmt Stmt { get; }
        public StmtScriptElement(Stmt stmt)
        {
            Stmt = stmt;
        }

        public override bool Equals(object? obj)
        {
            return obj is StmtScriptElement element &&
                   EqualityComparer<Stmt>.Default.Equals(Stmt, element.Stmt);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Stmt);
        }

        public static bool operator ==(StmtScriptElement? left, StmtScriptElement? right)
        {
            return EqualityComparer<StmtScriptElement?>.Default.Equals(left, right);
        }

        public static bool operator !=(StmtScriptElement? left, StmtScriptElement? right)
        {
            return !(left == right);
        }
    }

    public class EnumDeclScriptElement : ScriptElement
    {
        public EnumDecl EnumDecl { get; }
        public EnumDeclScriptElement(EnumDecl enumDecl) { EnumDecl = enumDecl; }
    }
        
}