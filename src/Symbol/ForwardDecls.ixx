export module Citron.MSymbol:ForwardDecls;

import <memory>;

// NOTICE: strong ownership모델에서 '모듈 파티션'에 export키워드 없이 전방선언을 하면 그 '모듈 파티션'에 귀속된다
//         전방선언을 export하면 '모듈'에 귀속되는 것 같다
// 
//         그래서 ForwardDelcs 파티션을 만들고, 필요한 모든 전방선언을 다 export로 설정했다.
//         모듈 파티션에서 전방선언이 필요한 경우, 직접 전방선언을 하지 말고 ForwardDecls 파티션에 선언을 한 후
//         import :ForwardDecls를 하도록 한다
// 
//         (단, 같은 모듈 파티션 안에서 전방선언이 필요한 경우에는 그대로 전방선언을 한다)
//
namespace Citron {


export class MDecl;
export class MDeclVisitor;

export class MModule;
export class MNamespaceDecl;

export class MGlobalFuncDecl; // top-level decl space

export class MClassDecl;
export class MClassCtorDecl;  // class decl space
export class MClassFuncDecl;  // class delc space
export class MClassVarDecl;

export class MStructDecl;
export class MStructCtorDecl; // struct decl space
export class MStructFuncDecl;  // struct decl space
export class MStructVarDecl;

export class MEnumDecl;
export class MEnumElemDecl;
export class MEnumElemVarDecl;

export class MInterfaceDecl;

export class MFuncParameter;

export class MFuncDeclVisitor;

export class MDeclId;
export using MDeclIdPtr = std::shared_ptr<MDeclId>;

export class MDeclIdFactory;

export class MType;
export using MTypePtr = std::shared_ptr<MType>;

export class MTypeArguments;
export using MTypeArgumentsPtr = std::shared_ptr<MTypeArguments>;

}