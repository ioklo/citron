using Citron.Infra;
using Citron.IR0Visitor;
using Citron.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using R = Citron.IR0;

namespace Citron.IR0Analyzer.NullRefAnalysis
{
    public class ScriptAnalyzer
    {
        public static Result Analyze(R.Script script, ILogger logger)
        {
            var globalContext = new GlobalContext(logger);
            var localContext = new LocalContext(null); // 최상위

            StmtAnalyzer.Analyze(script.TopLevelStmts, globalContext, localContext);            

            return Result.Success.Instance;
        }
    }
}
