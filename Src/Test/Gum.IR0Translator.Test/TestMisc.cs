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

    static class MTypes
    {
        public static readonly M.Name System_Runtime = new M.Name.Normal("System.Runtime");
        public static readonly M.NamespacePath System = new M.NamespacePath(null, new M.Name.Normal("System"));

        public static readonly M.TypeId Int = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("Int32"), default);
        public static readonly M.TypeId Bool = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("Boolean"), default);
        public static readonly M.TypeId String = new M.RootTypeId(System_Runtime, System, new M.Name.Normal("String"), default);
    }
}
