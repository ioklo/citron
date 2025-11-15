#pragma once
#include "MIRConfig.h"
#include <vector>
#include <memory>

namespace Citron {

class MStmt;
class MExp;
class MLoc;
class MData;
struct MFuncBody;

class MFactory
{
    std::vector<std::unique_ptr<MData>> datas;
    std::vector<std::unique_ptr<MStmt>> stmts;
    std::vector<std::unique_ptr<MExp>> exps;
    std::vector<std::unique_ptr<MLoc>> locs;

public:
    MIR_API MFactory();
    MIR_API ~MFactory();

    MIR_API MData* MakeMData(std::vector<MFuncBody>&& funcBodies);

    template<typename TMStmt, typename... TArgs> requires std::derived_from<TMStmt, MStmt>
    TMStmt* MakeMStmt(TArgs&&... args)
    {
        auto stmt = std::make_unique<TMStmt>(std::forward<TArgs>(args)...);
        auto* pStmt = stmt.get();
        stmts.push_back(std::move(stmt));
        return pStmt;
    }

    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    TMExp* MakeMExp(TArgs&&... args)
    {
        auto exp = std::make_unique<TMExp>(std::forward<TArgs>(args)...);
        auto* pExp = exp.get();
        exps.push_back(std::move(exp));
        return pExp;
    }

    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    TMLoc* MakeMLoc(TArgs&&... args)
    {
        auto loc = std::make_unique<TMLoc>(std::forward<TArgs>(args)...);
        auto* pLoc = loc.get();
        locs.push_back(std::move(loc));
        return pLoc;
    }
};

using MFactoryPtr = std::shared_ptr<MFactory>;

} // namespace Citron