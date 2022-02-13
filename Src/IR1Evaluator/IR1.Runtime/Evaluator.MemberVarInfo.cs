namespace Citron.IR1.Runtime
{
    public partial class Evaluator
    {
        struct MemberVarInfo
        {
            public int Index { get; }
            public MemberVarInfo(int index)
            {
                Index = index;
            }
        }
    }
}
