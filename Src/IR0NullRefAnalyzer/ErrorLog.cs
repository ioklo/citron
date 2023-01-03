using Citron.Log;

namespace Citron.IR0Analyzer.NullRefAnalysis
{
    public abstract record class ErrorLog(string Message) : ILog;

    // IR0Analyzer
    public record class R0101_StaticNotNullDirective_LocationIsNull() : ErrorLog("");
}