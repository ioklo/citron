#pragma once

#define BEGIN_CITRON_VISITOR(Name) \
template<class TFrom, class TVisitor> \
concept Name##ConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>; \
\
template<typename TVisitor, typename... TVisitorArgs> \
concept Name##Visitable = requires(TVisitor && v, TVisitorArgs&&... args) \
{\
    typename std::remove_cvref_t<TVisitor>::ResultType; \

#define MIDDLE_CITRON_VISITOR(Name)

#define 