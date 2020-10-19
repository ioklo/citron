namespace Gum.IR0
{
    public struct ExpInfo
    {
        public Exp Exp { get; }
        public TypeId TypeId { get; }

        public ExpInfo(Exp exp, TypeId typeId)
        {
            Exp = exp;
            TypeId = typeId;
        }
    }
}
