# 2026-07-22 RName / RIdentifier 역할 분리

## Status

`RName`과 declaration key의 역할을 재검토했다. `RDeclKeyString`과 구체 variant 형식은 후보이며 확정하지 않았다.

## Confirmed Direction

- `RName`은 source lookup용 plain string으로 축소하지 않는다.
- `RName`은 RSymbol 전반에서 쓰는 구조화된 semantic name value다. local variable, parameter, member, reserved compiler name 등 declaration lookup 이외의 자리에 쓸 수 있다.
- `RName_CtorParam`은 memberwise constructor parameter가 원래 member name에서 유래했다는 정보를 보존하는 값이다. declaration identity가 아니므로 `RName`에 남는다.
- `RDecl`의 same-outer exact key는 `RDeclKey`이며, `RName`과 분리한다.
- `ResolveIdentifier` 같은 source/semantic lookup은 `RName`을 입력으로 받고 function overload group을 반환할 수 있다. internal exact child lookup은 `RDeclKey`, CTI symbol reference와 global declaration path는 `RIdentifier`를 사용한다.

예:

```text
void F(int i)

RName:        F
RDeclKey:     function F with Normal:int parameter identity
```

constructor, destructor, lambda, impl처럼 lookup name이 없는 declaration도 `RDeclKey`는 가진다.

## Why RName Is Not a Lookup-Only Type

`RName_CtorParam(index, paramText)`은 함수 parameter의 표시/semantic name으로 쓰인다. parameter name은 function declaration identity에 포함되지 않지만 compiler가 memberwise constructor provenance를 추적하는 데는 필요하다. 따라서 `RName` variant를 declaration key로 재해석하거나 plain source string으로 교체하면 안 된다.

`RName_Reserved`도 `this`, `return` 등 compiler semantic name에 사용될 수 있다. foreach convention name과 source-visible ordinary name의 정책은 별도 문제다.

## Candidate: Structured Identifier + Serialized Identifier

다음은 내일 재논의할 후보이며 아직 wiki current rule이 아니다.

```text
RDeclKey           structured same-outer declaration key
RDeclKeyString     canonical serialized text of RDeclKey
```

candidate variants:

```text
RDeclKey_Name(RName)
RDeclKey_Func(RName, vector<RFuncParamIdentifier>)
RDeclKey_Ctor(vector<RFuncParamIdentifier>)
RDeclKey_Dtor()
RDeclKey_Lambda(index)
RDeclKey_ImplTrait(target, traitTypeId)
```

`RFuncParamIdentifier`는 parameter name을 넣지 않고 passing kind와 canonical type identity만 보관하는 후보다.

`RDeclKey_ImplTrait`에 full `RDeclKey`를 value로 직접 넣으면 recursive variant가 된다. current same-outer direct target type-member 제약 아래에서는 target에 `RDeclKeyString` 또는 제한된 named-type key를 넣는 방안을 검토한다.

## Open Questions

- `RDeclKey`를 구조화 variant로 둘지, canonical string만 보관할지.
- `RDeclKeyString`을 별도 wrapper로 둘지, `RDeclKey::ToString()` 반환값으로 충분할지.
- same outer에서 type/variable/namespace 같은 서로 다른 category가 동일 source name을 가질 수 있는지. 가능하다면 `RDeclKey` 충돌을 막기 위해 category를 넣거나 cross-category duplication을 금지해야 한다.
- function parameter passing kind와 generic signature를 exact declaration identity/overload identity에 포함할지.
- signature가 완성되기 전에 overload group에 등록되는 함수의 exact identifier map을 언제 seal/register할지.
