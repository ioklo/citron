#pragma once
#include "SyntaxConfig.h"
#include <Infra/Json.h>
#include <Infra/Unreachable.h>

namespace Citron {

//enum class UnaryOpSyntaxKind
//{
//    PostfixInc, PostfixDec,
//
//    Minus, LogicalNot, PrefixInc, PrefixDec,
//
//    Ref, Deref, // &, *, local인지 box인지는 분석을 통해서 알아내게 된다
//};
//
//inline JsonString ToJson(UnaryOpSyntaxKind kind)
//{
//    switch (kind)
//    {
//        case UnaryOpSyntaxKind::PostfixInc: return JsonString(U"PostfixInc");
//        case UnaryOpSyntaxKind::PostfixDec: return JsonString(U"PostfixDec");
//
//        case UnaryOpSyntaxKind::Minus: return JsonString(U"Minus"); 
//        case UnaryOpSyntaxKind::LogicalNot: return JsonString(U"LogicalNot"); 
//        case UnaryOpSyntaxKind::PrefixInc: return JsonString(U"PrefixInc");
//        case UnaryOpSyntaxKind::PrefixDec: return JsonString(U"PrefixDec");
//
//        case UnaryOpSyntaxKind::Ref: return JsonString(U"Ref"); 
//        case UnaryOpSyntaxKind::Deref: return JsonString(U"Deref");
//    }
//
//    unreachable();
//}

} // namespace Citron
