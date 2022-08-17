using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Module
{
    // Name algebraic data type 
    public abstract record Name
    {
        public record Singleton(string DebugText) : Name;

        public static readonly Singleton IndexerGet = new Singleton("IndexerGet");
        public static readonly Singleton IndexerSet = new Singleton("IndexerSet");
        public static readonly Singleton Constructor = new Singleton("Constructor");
        public static readonly Singleton OpInc = new Singleton("OpInc");
        public static readonly Singleton OpDec = new Singleton("OpDec");

        public static readonly Name Nullable = new Singleton("Nullable");
        public static readonly Name CapturedThis = new Singleton("CapturedThis");

        // for TopLevelStmt
        public static readonly Name TopLevel = new Singleton("TopLevel");

        public record Anonymous(int Index) : Name;
        public record ConstructorParam(int Index) : Name;
        public record Normal(string Text) : Name;
    }
}
