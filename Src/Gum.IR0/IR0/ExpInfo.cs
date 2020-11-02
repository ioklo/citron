namespace Gum.IR0
{
    public struct ExpInfo
    {
        public Exp Exp { get; }
        public Type Type { get; }

        public ExpInfo(Exp exp, Type type)
        {
            Exp = exp;
            Type = type;
        }
    }
}
