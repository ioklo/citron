using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    // Name algebraic data type 
    public abstract record class Name
    {
        public record class Singleton(string DebugText) : Name;

        public static readonly Singleton IndexerGet = new Singleton("IndexerGet");
        public static readonly Singleton IndexerSet = new Singleton("IndexerSet");
        public static readonly Singleton Constructor = new Singleton("Constructor");
        public static readonly Singleton OpInc = new Singleton("OpInc");
        public static readonly Singleton OpDec = new Singleton("OpDec");

        public static readonly Name Nullable = new Singleton("Nullable");
        public static readonly Name CapturedThis = new Singleton("CapturedThis");

        // for TopLevelStmt
        public static readonly Name TopLevel = new Singleton("TopLevel");

        public record class Anonymous(int Index) : Name;
        public record ConstructorParam(int Index, string Text) : Name; // trivial constructor에서 base로 가는 parameter
        public record class Normal(string Text) : Name;
    }
}
