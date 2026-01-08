#pragma once
#include <cassert>
#include <concepts>

namespace Citron {

template<typename T>
concept Transactionable = requires(T a) {
    { a.BeginTransaction() } -> std::same_as<void>;
    { a.CommitTransaction() } -> std::same_as<void>;
    { a.RollbackTransaction() } -> std::same_as<void>;
};

template<Transactionable T>
class Transaction
{
    T& target;
    bool ended;

public:
    Transaction(T& target) : target{target}, ended{false} { target.BeginTransaction(); }

    void Commit()
    {
        if (!ended)
        {
            target.CommitTransaction();
            ended = true;
        }
    }

    void Rollback()
    {
        if (!ended)
        {
            target.RollbackTransaction();
            ended = true;
        }
    }

    ~Transaction()
    {
        // commit인지, rollback인지를 Transaction에서는 처리하지 말고 그냥 ended체크만 한다
        assert(ended);
    }
};

} // namespace Citron