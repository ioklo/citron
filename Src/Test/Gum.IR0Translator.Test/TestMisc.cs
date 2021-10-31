using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Gum.Collections;
using System.Linq;

using static Gum.Infra.Misc;

using S = Gum.Syntax;
using Gum.Log;

namespace Gum.IR0Translator.Test
{
    static class TestMisc
    {   
        public static void VerifyError(IEnumerable<ILog> logs, AnalyzeErrorCode code, S.ISyntaxNode node)
        {
            var result = logs.OfType<AnalyzeErrorLog>()
                .Any(error => error.Code == code && error.Node == node);

            Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
        }
    }
}
