#pragma once

#include <cstddef>
#include <concepts>
#include <iterator>
#include <memory>
#include <new>
#include <ranges>
#include <type_traits>
#include <utility>

namespace Citron {

template<typename TItem> requires std::is_pointer_v<TItem>
class AnyPtrView
{
    static constexpr std::size_t VIEW_BUFFER_MAX = 8 * sizeof(void*);
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
        using DerefFn = TItem(*)(const iterator*);
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
        using value_type = TItem;
        using difference_type = std::ptrdiff_t;

        iterator() = default;

        template<typename Iter, typename Sent> 
            requires std::sentinel_for<Sent, Iter> && std::convertible_to<std::iter_reference_t<Iter>, TItem>
        iterator(Iter it, Sent sent)
        {
            static_assert(sizeof(Iter) <= ITER_BUFFER_MAX);
            static_assert(sizeof(Sent) <= SENT_BUFFER_MAX);
            static_assert(alignof(Iter) <= alignof(std::max_align_t));
            static_assert(alignof(Sent) <= alignof(std::max_align_t));

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

            deref_fn = [](const iterator* self) -> TItem {
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
            inc_fn(this);
            return *this;
        }

        void operator++(int)
        {
            ++(*this);
        }

        TItem operator*() const
        {
            return deref_fn(this);
        }

        bool operator==(std::default_sentinel_t) const
        {
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
    using DestroyFn = void (*)(AnyPtrView*) noexcept;
    using MoveFn = void (*)(AnyPtrView* dst, AnyPtrView* src);
    using BeginFn = iterator(*)(AnyPtrView*);

    DestroyFn destroy_fn = nullptr;
    MoveFn move_fn = nullptr;
    BeginFn begin_fn = nullptr;

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
    }

    template<typename R>
    using AllViewT = decltype(std::views::all(std::declval<R>()));

public:
    AnyPtrView() = default;

    template<typename R> requires 
        (!std::same_as<std::remove_cvref_t<R>, AnyPtrView>) &&
        std::ranges::viewable_range<R> &&
        requires { typename AllViewT<R>; } &&
        std::convertible_to<std::ranges::range_reference_t<AllViewT<R>&>, TItem>
    AnyPtrView(R&& r)
    {
        auto view = std::views::all(std::forward<R>(r));
        using V = decltype(view);

        static_assert(sizeof(V) <= VIEW_BUFFER_MAX);
        static_assert(alignof(V) <= alignof(std::max_align_t));

        new (view_buffer) V{std::move(view)};

        destroy_fn = [](AnyPtrView* self) noexcept {
            self->view_ptr<V>()->~V();
        };

        move_fn = [](AnyPtrView* dst, AnyPtrView* src) {
            new (dst->view_buffer) V{std::move(*src->view_ptr<V>())};

            dst->destroy_fn = src->destroy_fn;
            dst->move_fn = src->move_fn;
            dst->begin_fn = src->begin_fn;

            src->destroy_fn(src);
            src->clear();
        };

        begin_fn = [](AnyPtrView* self) -> iterator {
            V* v = self->view_ptr<V>();

            return iterator{
                std::ranges::begin(*v),
                std::ranges::end(*v)
            };
        };
    }

    AnyPtrView(AnyPtrView&& other)
    {
        if (other.move_fn)
            other.move_fn(this, &other);
    }

    AnyPtrView& operator=(AnyPtrView&& other)
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

    AnyPtrView(const AnyPtrView&) = delete;
    AnyPtrView& operator=(const AnyPtrView&) = delete;

    ~AnyPtrView()
    {
        if (destroy_fn)
            destroy_fn(this);
    }

    iterator begin()
    {
        return begin_fn(this);
    }

    std::default_sentinel_t end() const
    {
        return {};
    }
};

} // namespace Citron