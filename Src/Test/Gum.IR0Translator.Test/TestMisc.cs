using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using System.Collections.Immutable;
using System.Linq;

using static Gum.Infra.Misc;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator.Test
{
    static class TestMisc
    {   
        public static void VerifyError(IEnumerable<IError> errors, AnalyzeErrorCode code, S.ISyntaxNode node)
        {
            var result = errors.OfType<AnalyzeError>()
                .Any(error => error.Code == code && error.Node == node);

            Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
        }
    }
}
