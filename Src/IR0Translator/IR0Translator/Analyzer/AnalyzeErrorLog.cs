using Citron.Log;
using Citron.Syntax;
using System;
using System.Collections.Generic;
using S = Citron.Syntax;

namespace Citron.IR0Translator
{
    public class AnalyzeErrorLog : ILog
    {
        public AnalyzeErrorCode Code { get; }
        public S.ISyntaxNode Node { get; }
        public string Message { get; }

        public AnalyzeErrorLog(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
        {
            (Code, Node, Message) = (code, node, msg);
        }

        public override bool Equals(object? obj)
        {
            return obj is AnalyzeErrorLog log &&
                   Code == log.Code &&
                   EqualityComparer<ISyntaxNode>.Default.Equals(Node, log.Node);
                   // Message == log.Message; 메세지 비교 무시
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Node, Message);
        }
    }
}
