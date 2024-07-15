#pragma once
#include <variant>

namespace Citron {

using RExp = std::variant<
    // Storage
    class RLoadExp, class RAssignExp,

    // BoxRef
    class RBoxExp, class RStaticBoxRefExp, class RClassMemberBoxRefExp, class RStructIndirectMemberBoxRefExp, class RStructMemberBoxRefExp,

    // LocalRef
    class RLocalRefExp,

    // Literal
    class RBoolLiteralExp, class RIntLiteralExp, class RStringExp,

    // List
    class RListExp, class RListIteratorExp,

    // Call Internal
    class RCallInternalUnaryOperatorExp, class RCallInternalUnaryAssignOperatorExp, class RCallInternalBinaryOperatorExp,

    // Global
    class RCallGlobalFuncExp,

    // Class
    class RNewClassExp, class RCallClassMemberFuncExp, class RCastClassExp,

    // Struct
    class RNewStructExp, class RCallStructMemberFuncExp,

    // Enum
    class RNewEnumElemExp, class RCastEnumElemToEnumExp,

    // Nullable
    class RNewNullableExp, class RNullableNullLiteralExp,

    // Lambda
    class RLambdaExp, class RCallLambdaExp,

    // Func
    class RCastBoxedLambdaToFuncExp,

    // InlineBlock
    class RInlineBlockExp,

    // Test
    class RClassIsClassExp, class RClassAsClassExp,
    class RClassIsInterfaceExp, class RClassAsInterfaceExp,
    class RInterfaceIsClassExp, class RInterfaceAsClassExp,
    class RInterfaceIsInterfaceExp, class RInterfaceAsInterfaceExp,
    class REnumIsEnumElemExp, class REnumAsEnumElemExp
>;

template<typename TVisitor>
concept RExpVisitor = requires(TVisitor visitor) {
    
    { visitor.Visit(std::declval<RLoadExp>()) }; 
    { visitor.Visit(std::declval<RAssignExp>()) };

    { visitor.Visit(std::declval<RBoxExp>()) }; 
    { visitor.Visit(std::declval<RStaticBoxRefExp>()) }; 
    { visitor.Visit(std::declval<RClassMemberBoxRefExp>()) };
    { visitor.Visit(std::declval<RStructIndirectMemberBoxRefExp>()) }; 
    { visitor.Visit(std::declval<RStructMemberBoxRefExp>()) };
        
    { visitor.Visit(std::declval<RLocalRefExp>()) };
    { visitor.Visit(std::declval<RBoolLiteralExp>()) }; 
    { visitor.Visit(std::declval<RIntLiteralExp>()) };
    { visitor.Visit(std::declval<RStringExp>()) };

    { visitor.Visit(std::declval<RListExp>()) }; 
    { visitor.Visit(std::declval<RListIteratorExp>()) };

    { visitor.Visit(std::declval<RCallInternalUnaryOperatorExp>()) };
    { visitor.Visit(std::declval<RCallInternalUnaryAssignOperatorExp>()) }; 
    { visitor.Visit(std::declval<RCallInternalBinaryOperatorExp>()) };

    { visitor.Visit(std::declval<RCallGlobalFuncExp>()) };

    { visitor.Visit(std::declval<RNewClassExp>()) }; 
    { visitor.Visit(std::declval<RCallClassMemberFuncExp>()) }; 
    { visitor.Visit(std::declval<RCastClassExp>()) };

    { visitor.Visit(std::declval<RNewStructExp>()) };
    { visitor.Visit(std::declval<RCallStructMemberFuncExp>()) };

    { visitor.Visit(std::declval<RNewEnumElemExp>()) };
    { visitor.Visit(std::declval<RCastEnumElemToEnumExp>()) };

    { visitor.Visit(std::declval<RNewNullableExp>()) }; 
    { visitor.Visit(std::declval<RNullableNullLiteralExp>()) };

    { visitor.Visit(std::declval<RLambdaExp>()) }; 
    { visitor.Visit(std::declval<RCallLambdaExp>()) };

    { visitor.Visit(std::declval<RCastBoxedLambdaToFuncExp>()) };

    { visitor.Visit(std::declval<RInlineBlockExp>()) };

    { visitor.Visit(std::declval<RClassIsClassExp>()) }; 
    { visitor.Visit(std::declval<RClassAsClassExp>()) };
    { visitor.Visit(std::declval<RClassIsInterfaceExp>()) };
    { visitor.Visit(std::declval<RClassAsInterfaceExp>()) };
    { visitor.Visit(std::declval<RInterfaceIsClassExp>()) };
    { visitor.Visit(std::declval<RInterfaceAsClassExp>()) };
    { visitor.Visit(std::declval<RInterfaceIsInterfaceExp>()) }; 
    { visitor.Visit(std::declval<RInterfaceAsInterfaceExp>()) };
    { visitor.Visit(std::declval<REnumIsEnumElemExp>()) }; 
    { visitor.Visit(std::declval<REnumAsEnumElemExp>()) };
};


}