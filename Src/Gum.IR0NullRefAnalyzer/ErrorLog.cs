using Gum.Log;

namespace Gum.IR0Analyzer.NullRefAnalysis
{
    public abstract record ErrorLog(string Message) : ILog;

    // IR0Analyzer
    public record R0101_StaticNotNullDirective_LocationIsNull() : ErrorLog("");
}