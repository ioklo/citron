#include "MLocQInstsTranslation.h"

#include <variant>

#include "Infra/Exceptions.h"
#include "MIR/MLoc.h"

#include "QBodyContext.h"

using namespace std;

namespace Citron::IR0IR1Translator {

// 메모리 주소를 value로 돌려주는 
class MLocQInstsTranslator
{
public:
    using ResultType = expected<QLocResult, DiagPtr>;
    QBodyContext& bodyContext;

public:
    MLocQInstsTranslator(QBodyContext& bodyContext)
        : bodyContext{bodyContext} {
    }
    
    ResultType Visit(MLoc_Temp* loc) { throw NotImplementedException{}; }

    ResultType Visit(MLoc_LocalVar* loc)
    {
        size_t slotIndex = bodyContext.GetLocalVarSlotIndex(loc->name);
        return QLocResult_Slot{slotIndex};
    }

    ResultType Visit(MLoc_LambdaVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_ListIndexer* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_StructVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_ClassVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_EnumElemVar* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_This* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_LocalDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_BoxDeref* loc) { throw NotImplementedException{}; }
    ResultType Visit(MLoc_NullableValue* loc) { throw NotImplementedException{}; }
};

expected<QLocResult, DiagPtr> TranslateMLocToQInsts(MLoc* loc, QBodyContext& bodyContext)
{
    MLocQInstsTranslator translator{bodyContext};
    return Accept(translator, loc);
}

} // Citron