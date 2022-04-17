using Citron.Syntax;
using System;
using System.Runtime.Serialization;

namespace Citron.Analysis
{
    // 분석 체인을 빠져나올때 쓰는 예외, 에러 보고와는 분리한다.
    class AnalyzerFatalException : Exception
    {        
    }
}