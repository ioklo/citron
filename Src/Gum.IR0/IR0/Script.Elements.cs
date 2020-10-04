using System;
using System.Collections.Generic;

namespace Gum.IR0
{
    public partial class Script
    {
        public abstract class Element
        {
            internal Element() { }
        }

        public class FuncDeclElement : Element
        {
            public FuncDecl FuncDecl { get; }
            public FuncDeclElement(FuncDecl funcDecl)
            {
                FuncDecl = funcDecl;
            }

            public override bool Equals(object? obj)
            {
                return obj is FuncDeclElement element &&
                       EqualityComparer<FuncDecl>.Default.Equals(FuncDecl, element.FuncDecl);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FuncDecl);
            }

            public static bool operator ==(FuncDeclElement? left, FuncDeclElement? right)
            {
                return EqualityComparer<FuncDeclElement?>.Default.Equals(left, right);
            }

            public static bool operator !=(FuncDeclElement? left, FuncDeclElement? right)
            {
                return !(left == right);
            }
        }

        public class StmtElement : Element
        {
            public Stmt Stmt { get; }
            public StmtElement(Stmt stmt)
            {
                Stmt = stmt;
            }

            public override bool Equals(object? obj)
            {
                return obj is StmtElement element &&
                       EqualityComparer<Stmt>.Default.Equals(Stmt, element.Stmt);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Stmt);
            }

            public static bool operator ==(StmtElement? left, StmtElement? right)
            {
                return EqualityComparer<StmtElement?>.Default.Equals(left, right);
            }

            public static bool operator !=(StmtElement? left, StmtElement? right)
            {
                return !(left == right);
            }
        }

        public class EnumDeclElement : Element
        {
            public EnumDecl EnumDecl { get; }
            public EnumDeclElement(EnumDecl enumDecl) { EnumDecl = enumDecl; }
        }

        public class StructDeclElement : Element
        {
            public StructDecl StructDecl { get; }
            public StructDeclElement(StructDecl structDecl) { StructDecl = structDecl; }
        }
    }
}