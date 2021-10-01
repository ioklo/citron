using System;
using R = Gum.IR0;

// TODO: 그냥 모듈 이름이랑 맞추는게 나을 것 같다
namespace Gum.IR0Analyzer
{
    // 지정된 순서대로 순회한다
    public class IR0DataFlowAnalyzer
    {
        public static void AnalyzeScript(R.Script script, IIR0DataFlowAnalyzer analyzer)
        {
            foreach (var stmt in script.TopLevelStmts)
                AnalyzeStmt(stmt, analyzer);
        }

        public static void AnalyzeStmt(R.Stmt stmt, IIR0DataFlowAnalyzer analyzer)
        {   
        }
    }
}
