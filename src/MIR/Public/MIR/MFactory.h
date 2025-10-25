#pragma once
#include <vector>
#include <memory>

namespace Citron {

class MStmt;
class MExp;
class MLoc;

class MFactory
{
    std::vector<std::unique_ptr<MStmt>> mStmts;
    std::vector<std::unique_ptr<MExp>> mExps;
    std::vector<std::unique_ptr<MLoc>> mLocs;

public:
    template<typename TMStmt, typename... TArgs> requires std::derived_from<TMStmt, MStmt>
    TMStmt* MakeMStmt(TArgs&&... args)
    {
        auto stmt = std::make_unique<TMStmt>(std::forward<TArgs>(args)...);
        auto* pStmt = stmt.get();
        mStmts.push_back(std::move(stmt));
        return pStmt;
    }

    template<typename TMExp, typename... TArgs> requires std::derived_from<TMExp, MExp>
    TMExp* MakeMExp(TArgs&&... args)
    {
        auto exp = std::make_unique<TMExp>(std::forward<TArgs>(args)...);
        auto* pExp = exp.get();
        mExps.push_back(std::move(exp));
        return pExp;
    }

    template<typename TMLoc, typename... TArgs> requires std::derived_from<TMLoc, MLoc>
    TMLoc* MakeMLoc(TArgs&&... args)
    {
        auto loc = std::make_unique<TMLoc>(std::forward<TArgs>(args)...);
        auto* pLoc = loc.get();
        mLocs.push_back(std::move(loc));
        return pLoc;
    }
};

using MFactoryPtr = std::shared_ptr<MFactory>;

} // namespace Citron