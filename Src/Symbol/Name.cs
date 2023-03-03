using Citron.Infra;
using Pretune;
using System;
using System.Diagnostics;

namespace Citron.Symbol
{
    public abstract partial record class Name : ISerializable
    {
        public abstract void DoSerialize(ref SerializeContext context);
    }

    // Name algebraic data type 
    public abstract partial record class Name
    {
        public record class Singleton(string DebugText) : Name
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeString(nameof(DebugText), DebugText);
            }
        }

        public record class Anonymous(int Index) : Name
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeInt(nameof(Index), Index);
            }
        }

        public record ConstructorParam(int Index, string Text) : Name // trivial constructor에서 base로 가는 parameter
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeInt(nameof(Index), Index);
                context.SerializeString(nameof(Text), Text);
            }
        }

        public record class Normal(string Text) : Name
        {
            public override void DoSerialize(ref SerializeContext context)
            {
                context.SerializeString(nameof(Text), Text);
            }
        }
    }

    public static class Names
    {
        public static readonly Name IndexerGet = new Name.Singleton("IndexerGet");
        public static readonly Name IndexerSet = new Name.Singleton("IndexerSet");
        public static readonly Name Constructor = new Name.Singleton("Constructor");
        public static readonly Name OpInc = new Name.Singleton("OpInc");
        public static readonly Name OpDec = new Name.Singleton("OpDec");
        public static readonly Name Nullable = new Name.Singleton("Nullable");
    }
}
