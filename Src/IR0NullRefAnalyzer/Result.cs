namespace Citron.IR0Analyzer.NullRefAnalysis
{   
    public abstract class Result
    {
        public class Success : Result
        {
            public readonly static Success Instance = new Success();
            Success() { }
        }
    }
}
