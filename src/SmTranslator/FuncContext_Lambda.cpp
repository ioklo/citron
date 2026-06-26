#include "FuncContext_Lambda.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RDeclRes.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "ScopeContext.h"

using namespace std;

namespace Citron {

FuncContext_Lambda::FuncContext_Lambda(const FuncContextPtr& outerFunc, const ScopeContextPtr& outerScope, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : outerFunc{outerFunc}, outerScope{outerScope}, bSeqFunc{bSeqFunc}, funcReturn{move(funcReturn)}, funcParams{move(funcParams)}, bLastParamVariadic{bLastParamVariadic}
{
}

bool FuncContext_Lambda::CanAccess(RDecl* target)
{
    return outerFunc->CanAccess(target);
}

RTypeDecl* FuncContext_Lambda::ResolveTypeDecl(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return outerFunc->ResolveTypeDecl(name, explicitTypeParamsExceptOuterCount);
}

// class C<T> { void F<S> {
//     List<T> x;      // 5) scopeContext.ResolveIdentifier(x, 0) => RDeclRes
//     var f = () => { // 4) funcContext.ResolveIdentifier(x, 0)
//         // 3) scopeContext.ResolveIdentifier(x, 0)
//
//         var g = () => { // 2) funcContext.ResolveIdentifier(x, 0)
//
//             // 1) 여기에서 scopeContext.ResolveIdentifier(x, 0) 호출
//             x;
//
//         }
//     }
// } }
expected<optional<BodyRes>, DiagPtr> FuncContext_Lambda::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 1. lambdaVar 검색
    throw NotImplementedException{};

    // 2. lambdaVar가 없으면 outer에서 검색
    auto e_o_bodyRes = outerScope->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
    RETURN_ON_ERROR(e_o_bodyRes);

    auto& o_bodyRes = *e_o_bodyRes;
    if (!o_bodyRes) return nullopt;

    // 로컬 var가 들어오면, LambdaVar로 만든다
    // var x = 1;
    // var l = () => x; // x는 처음엔 localVar, 그다음엔 lambdaVar
    // RDeclRes_NeedCapture("x", BodyRes_LocalVar(x))) 람다 l에 상위 funcContext의 localvar x를 캡쳐한다

    // 그냥 RDeclRes가 localVar를 승격시키라는 명령만 내보내면 좋을거 같은데,
    // 문제는 nested인 경우 승격을 어떻게 표현하는가
    // var x = 1;
    // var l = () { var l2 = () => x; }
    // RDeclRes_NeedCapture("x", RDeclRes_NeedCapture("x", BodyRes_LocalVar(x))))
    // nested 깊이는 상위 funcContext의 깊이 만큼임을 보장한다
    return o_bodyRes->Visit([this, &name](auto& bodyRes) -> BodyRes
    {
        using T = remove_cvref_t<decltype(bodyRes)>;

        if constexpr (same_as<T, BodyRes_NeedCapture>)
        {
            return BodyRes_NeedCapture{name, make_unique<BodyRes>(move(bodyRes))};
        }
        else if constexpr(same_as<T, BodyRes_LocalVar>)
        {
            // 로컬 var 중에서도 primitive만 암시적으로 복사 형식으로 capture를 합니다
            // 나머지는 capture list에 명시적으로 적어주는 것으로 (복사, ref)
            if (dynamic_cast<RType_Primitive*>(bodyRes.type))
            {
                return BodyRes_NeedCapture{name, make_unique<BodyRes>(move(bodyRes))};
            }
            else
            {
                // 에러 처리, 명시적으로 써주세요
                throw NotImplementedException{};
            }
        }
        else if constexpr (same_as<T, BodyRes_LocalRef>)
        {
            if (dynamic_cast<RType_Primitive*>(bodyRes.type))
            {
                return BodyRes_NeedCapture{name, make_unique<BodyRes>(move(bodyRes))};
            }
            else
            {
                // 에러 처리, primitive 타입이 아니면 명시적으로 써주세요
                throw NotImplementedException{};
            }
        }
        else if constexpr (same_as<T, BodyRes_ThisVar>)
        {
            // this는 명시적으로 써줘야 하기 때문에, 자동 캡쳐하지 않습니다
            // 캡쳐에 명시적으로 써져 있었다면, RDeclRes_LambdaVar로 들어왔을 겁니다
            // 에러 처리
            throw NotImplementedException{};
        }
        else if constexpr (same_as<T, BodyRes_RDeclRes>)
        {
            // rDeclRes는 consexpr분기용, bodyRes는 move할때 씁니다. (rDeclRes를 bodyRes move이후에 참조하지 않도록 주의)
            return bodyRes.declRes.Visit([this, &name, &bodyRes](auto& rDeclRes) -> BodyRes {
                using U = remove_cvref_t<decltype(rDeclRes)>;

                if constexpr (same_as<U, RDeclRes_LambdaVar>)
                {
                    // 람다 variable이라고 할지라도, primitive가 아니면 암시적으로 캡쳐하지 않습니다
                    auto* declType = rDeclRes.decl->GetUnboundDeclType();

                    if (dynamic_cast<RType_Primitive*>(declType))
                    {
                        return BodyRes_NeedCapture{name, make_unique<BodyRes>(move(bodyRes))};
                    }
                    else
                    {
                        // 에러 처리, primitive 타입이 아니면 명시적으로 써주세요
                        throw NotImplementedException{};
                    }
                }
                else if constexpr (same_as<U, RDeclRes_ClassVar>)
                {
                    // TODO: this의 classVar라면 this를 capture 했는지 보고, 안했다면, 에러
                    throw NotImplementedException{};
                }
                else if constexpr (same_as<U, RDeclRes_StructVar>)
                {
                    // TODO: this의 classVar라면 this를 capture 했는지 보고, 안했다면, 에러
                    throw NotImplementedException{};
                }
                else 
                    return bodyRes;

            });
        }
        else return bodyRes; // 나머지는 그대로 리턴
    });
}

RFuncReturn FuncContext_Lambda::GetUnboundFuncReturn()
{
    return funcReturn;
}

void FuncContext_Lambda::SetOpenFuncReturn(RType* retType)
{
    assert(holds_alternative<RFuncReturn_NotSet>(funcReturn));
    funcReturn = RFuncReturn_Normal{retType};
}

RTypeArguments* FuncContext_Lambda::MakeOpenTypeArgs()
{
    return outerFunc->MakeOpenTypeArgs();
}

bool FuncContext_Lambda::IsSeqFunc()
{
    return bSeqFunc;
}

MLoc_This* FuncContext_Lambda::MakeThisLoc()
{
    return outerFunc->MakeThisLoc();
}

} // namespace Citron