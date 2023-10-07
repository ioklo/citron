using System.Threading.Tasks;
using Citron.IR2;
using Citron.Symbol;

namespace Citron;

public class IR2Evaluator
{
    public static ValueTask Evaluate(Program program, SymbolId entryId)
    {
        // var funcInfo = program.GetFuncInfo(entryId.GetDeclSymbolId());

        // generics 고려 해야 한다

        return ValueTask.CompletedTask;
    }

}

