#pragma once

#include <cassert>
#include <cstddef>
#include <concepts>
#include <iterator>
#include <memory>
#include <new>
#include <ranges>
#include <type_traits>
#include <utility>

namespace Citron {

template<typename TPtr> requires std::is_pointer_v<TPtr>
class AnyPtrSizedRange
{
    template<std::size_t Actual, std::size_t Max> struct BufferTooSmall; // diagnostic helper

    static constexpr std::size_t VIEW_BUFFER_MAX = 4 * sizeof(void*);
    alignas(std::max_align_t) std::byte view_buffer[VIEW_BUFFER_MAX];

public:
    class iterator
    {
        static constexpr std::size_t ITER_BUFFER_MAX = 4 * sizeof(void*);
        static constexpr std::size_t SENT_BUFFER_MAX = 4 * sizeof(void*);

        alignas(std::max_align_t) std::byte iter_buffer[ITER_BUFFER_MAX];
        alignas(std::max_align_t) std::byte sent_buffer[SENT_BUFFER_MAX];

        using DestroyFn = void (*)(iterator*) noexcept;
        using MoveFn = void (*)(iterator* dst, iterator* src);
        using IncFn = void (*)(iterator*);
        using DerefFn = TPtr(*)(const iterator*);
        using EqEndFn = bool (*)(const iterator*);

        DestroyFn destroy_fn = nullptr;
        MoveFn move_fn = nullptr;
        IncFn inc_fn = nullptr;
        DerefFn deref_fn = nullptr;
        EqEndFn eq_end_fn = nullptr;

        template<typename Iter>
        Iter* iter_ptr()
        {
            return std::launder(reinterpret_cast<Iter*>(iter_buffer));
        }

        template<typename Iter>
        const Iter* iter_ptr() const
        {
            return std::launder(reinterpret_cast<const Iter*>(iter_buffer));
        }

        template<typename Sent>
        Sent* sent_ptr()
        {
            return std::launder(reinterpret_cast<Sent*>(sent_buffer));
        }

        template<typename Sent>
        const Sent* sent_ptr() const
        {
            return std::launder(reinterpret_cast<const Sent*>(sent_buffer));
        }

        void clear() noexcept
        {
            destroy_fn = nullptr;
            move_fn = nullptr;
            inc_fn = nullptr;
            deref_fn = nullptr;
            eq_end_fn = nullptr;
        }

    public:
        using iterator_concept = std::input_iterator_tag;
        using iterator_category = std::input_iterator_tag;
        using value_type = TPtr;
        using difference_type = std::ptrdiff_t;

        iterator() = default;

        template<typename Iter, typename Sent> requires 
            std::sentinel_for<Sent, Iter> &&
            std::convertible_to<std::iter_reference_t<Iter>, TPtr>
        iterator(Iter it, Sent sent)
        {
            if constexpr (ITER_BUFFER_MAX < sizeof(Iter)) { BufferTooSmall<sizeof(Iter), ITER_BUFFER_MAX> diag{}; }
            if constexpr (SENT_BUFFER_MAX < sizeof(Sent)) { BufferTooSmall<sizeof(Sent), SENT_BUFFER_MAX> diag{}; }

            static_assert(alignof(Iter) <= alignof(std::max_align_t),
                "AnyPtrSizedRange::iterator: iterator alignment is too large");

            static_assert(alignof(Sent) <= alignof(std::max_align_t),
                "AnyPtrSizedRange::iterator: sentinel alignment is too large");

            new (iter_buffer) Iter{std::move(it)};

            try
            {
                new (sent_buffer) Sent{std::move(sent)};
            }
            catch (...)
            {
                iter_ptr<Iter>()->~Iter();
                throw;
            }

            destroy_fn = [](iterator* self) noexcept {
                self->iter_ptr<Iter>()->~Iter();
                self->sent_ptr<Sent>()->~Sent();
            };

            move_fn = [](iterator* dst, iterator* src) {
                new (dst->iter_buffer) Iter{std::move(*src->iter_ptr<Iter>())};

                try
                {
                    new (dst->sent_buffer) Sent{std::move(*src->sent_ptr<Sent>())};
                }
                catch (...)
                {
                    dst->iter_ptr<Iter>()->~Iter();
                    throw;
                }

                dst->destroy_fn = src->destroy_fn;
                dst->move_fn = src->move_fn;
                dst->inc_fn = src->inc_fn;
                dst->deref_fn = src->deref_fn;
                dst->eq_end_fn = src->eq_end_fn;

                src->destroy_fn(src);
                src->clear();
            };

            inc_fn = [](iterator* self) {
                ++(*self->iter_ptr<Iter>());
            };

            deref_fn = [](const iterator* self) -> TPtr {
                return *(*self->iter_ptr<Iter>());
            };

            eq_end_fn = [](const iterator* self) -> bool {
                return *self->iter_ptr<Iter>() == *self->sent_ptr<Sent>();
            };
        }

        iterator(iterator&& other)
        {
            if (other.move_fn)
                other.move_fn(this, &other);
        }

        iterator& operator=(iterator&& other)
        {
            if (this == &other)
                return *this;

            if (destroy_fn)
                destroy_fn(this);

            clear();

            if (other.move_fn)
                other.move_fn(this, &other);

            return *this;
        }

        iterator(const iterator&) = delete;
        iterator& operator=(const iterator&) = delete;

        ~iterator()
        {
            if (destroy_fn)
                destroy_fn(this);
        }

        iterator& operator++()
        {
            assert(inc_fn);
            inc_fn(this);
            return *this;
        }

        void operator++(int)
        {
            ++(*this);
        }

        TPtr operator*() const
        {
            assert(deref_fn);
            return deref_fn(this);
        }

        bool operator==(std::default_sentinel_t) const
        {
            if (!eq_end_fn)
                return true;

            return eq_end_fn(this);
        }

        friend bool operator==(std::default_sentinel_t s, const iterator& it)
        {
            return it == s;
        }

        bool operator!=(std::default_sentinel_t s) const
        {
            return !(*this == s);
        }

        friend bool operator!=(std::default_sentinel_t s, const iterator& it)
        {
            return !(it == s);
        }
    };

private:
    using DestroyFn = void (*)(AnyPtrSizedRange*) noexcept;
    using MoveFn = void (*)(AnyPtrSizedRange* dst, AnyPtrSizedRange* src);
    using BeginFn = iterator(*)(AnyPtrSizedRange*);
    using SizeFn = std::size_t(*)(const AnyPtrSizedRange*);

    DestroyFn destroy_fn = nullptr;
    MoveFn move_fn = nullptr;
    BeginFn begin_fn = nullptr;
    SizeFn size_fn = nullptr;

    template<typename V>
    V* view_ptr()
    {
        return std::launder(reinterpret_cast<V*>(view_buffer));
    }

    template<typename V>
    const V* view_ptr() const
    {
        return std::launder(reinterpret_cast<const V*>(view_buffer));
    }

    void clear() noexcept
    {
        destroy_fn = nullptr;
        move_fn = nullptr;
        begin_fn = nullptr;
        size_fn = nullptr;
    }

    template<typename R>
    using AllViewT = std::views::all_t<R>;

public:
    AnyPtrSizedRange() = default;

    template<typename R> requires
        (!std::same_as<std::remove_cvref_t<R>, AnyPtrSizedRange>) &&
        std::ranges::viewable_range<R>&&
        std::ranges::sized_range<AllViewT<R>>&&
        std::convertible_to<std::ranges::range_reference_t<AllViewT<R>&>, TPtr>
    AnyPtrSizedRange(R&& r)
    {
        auto view = std::views::all(std::forward<R>(r));
        using V = decltype(view);

        if constexpr (VIEW_BUFFER_MAX < sizeof(V)) { BufferTooSmall<sizeof(V), VIEW_BUFFER_MAX> diag{}; }
        static_assert(alignof(V) <= alignof(std::max_align_t), "AnyPtrSizedRange: view alignment is too large");

        new (view_buffer) V{std::move(view)};

        destroy_fn = [](AnyPtrSizedRange* self) noexcept {
            self->view_ptr<V>()->~V();
        };

        move_fn = [](AnyPtrSizedRange* dst, AnyPtrSizedRange* src) {
            new (dst->view_buffer) V{std::move(*src->view_ptr<V>())};

            dst->destroy_fn = src->destroy_fn;
            dst->move_fn = src->move_fn;
            dst->begin_fn = src->begin_fn;
            dst->size_fn = src->size_fn;

            src->destroy_fn(src);
            src->clear();
        };

        begin_fn = [](AnyPtrSizedRange* self) -> iterator {
            V* v = self->view_ptr<V>();

            return iterator{
                std::ranges::begin(*v),
                std::ranges::end(*v)
            };
        };

        size_fn = [](const AnyPtrSizedRange* self) -> std::size_t {
            const V* v = self->view_ptr<V>();
            return static_cast<std::size_t>(std::ranges::size(*v));
        };
    }

    AnyPtrSizedRange(AnyPtrSizedRange&& other)
    {
        if (other.move_fn)
            other.move_fn(this, &other);
    }

    AnyPtrSizedRange& operator=(AnyPtrSizedRange&& other)
    {
        if (this == &other)
            return *this;

        if (destroy_fn)
            destroy_fn(this);

        clear();

        if (other.move_fn)
            other.move_fn(this, &other);

        return *this;
    }

    AnyPtrSizedRange(const AnyPtrSizedRange&) = delete;
    AnyPtrSizedRange& operator=(const AnyPtrSizedRange&) = delete;

    ~AnyPtrSizedRange()
    {
        if (destroy_fn)
            destroy_fn(this);
    }

    iterator begin()
    {
        if (!begin_fn)
            return {};

        return begin_fn(this);
    }

    std::default_sentinel_t end() const
    {
        return {};
    }

    std::size_t size() const
    {
        if (!size_fn)
            return 0;

        return size_fn(this);
    }

    std::size_t Count() const
    {
        return size();
    }

    bool empty() const
    {
        return size() == 0;
    }
};

} // namespace Citron