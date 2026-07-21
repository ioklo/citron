# 회의 / 설계 노트

Date: 2026-07-20
Title: `RImplTraitDecl` tree와 trait call target

## Summary

- 기존 `NStructInfo -> NImplTrait -> NImplTraitMember -> NImplTraitFunc` witness payload 모델 대신, impl syntax를 internal `RImplTraitDecl` symbol subtree로 나타내는 방향을 선택했다.
- `RImplTraitDecl`은 ordinary source name을 선언하지 않는다. lexical tree에는 compiler-private `RName_Impl(index)`로 들어가지만 ordinary member lookup, function overload group, CTI에는 노출하지 않는다.
- trait requirement 함수 구현은 `RImplTraitFuncDecl`로 두고 `RFuncDecl`을 구현한다. 이로써 일반 함수 body와 같은 `MFuncBody`, ABI, QIR call 경로를 재사용한다.
- call site는 source-local `NImplTraitFunc`를 보관하지 않는다. `Trait` call은 requirement (`RTraitFuncDecl`)와 applied trait를 보존하고, lowering이 direct impl call 또는 trait table call을 결정한다.

## 1. 기존 payload 모델의 문제

기존 구조는 다음과 같았다.

```text
RStructDecl
  └─ NStructInfo
       └─ NImplTrait
            └─ NImplTraitMember
                 └─ NImplTraitFunc
```

`NImplTraitFunc`를 stable pointer로 두고 MIR body의 owner/key로 쓰는 방안을 검토했다. 그러나 direct trait impl call이 기존 `RFuncDecl*` 기반 ABI/QIR 경로를 통과하려면 별도 callable declaration 또는 여러 downstream special case가 필요했다. 또한 call site가 `N*`를 보관하면 외부 CTI conformance와 `some Trait` opaque value를 표현할 수 없다.

global integer identity도 검토했지만, compiler-internal source payload에 registry, lifetime, invalid index, debug 역조회 규칙을 새로 추가한다. compact index가 필요해지는 시점은 semantic identity가 아니라 MIR/QIR module function table을 만들 때다.

## 2. Internal impl trait tree

```text
lexical outer
  └─ RImplTraitDecl                // RName_Impl(index)
       - target struct
       - matched conformance header
       - trait / trait arguments
       - impl generic binder
       └─ RImplTraitMemberDecl
            └─ RImplTraitFuncDecl  // RFuncDecl
```

`RImplTraitDecl`의 tree outer는 target struct가 아니라 impl syntax가 놓인 lexical outer다. target struct를 lexical outer로 쓰면 lexical lookup, target member lookup, impl generic binder가 하나의 outer chain에 섞인다. impl body의 target member/`this` lookup은 dedicated policy로 처리한다.

`RImplTraitMemberDecl`은 trait requirement와 대응하는 내부 member base다. function requirement implementation은 `RImplTraitFuncDecl : RImplTraitMemberDecl + RFuncDecl`로 둔다. associated type/const requirement가 생겨도 같은 member relation을 사용한다.

이 설계에서는 `NStructInfo`가 보관하던 `implTraits`가 중복이므로 제거한다. canonical conformance header entry는 `RStructDecl`에 남아 public surface를 담당하고, impl bind가 끝난 뒤 `RImplTraitDecl*`를 연결한다.

## 3. Extension과 helper

extension bundle의 최종 surface는 아직 확정되지 않았다. 다만 trait requirement implementation은 canonical conformance와 같으므로 `RImplTraitDecl`, `RImplTraitMemberDecl`, `RImplTraitFuncDecl` 계열을 재사용하는 방향을 우선한다.

```text
RExtensionDecl
  └─ RExtensionFuncDecl       // bundle-private shared helper

RImplTraitDecl
  └─ RImplTraitFuncDecl       // trait requirement implementation
```

`RExtensionFuncDecl`은 trait requirement를 충족하는 member가 아니라 extension bundle이 제공하는 private 공용 helper이므로 `RImplTraitMemberDecl`으로 만들지 않는다.

## 4. Trait call model

trait는 static contract이고 ordinary `dyn trait`는 없다. 그러므로 `MCallable_Trait`의 semantic identity는 local source implementation pointer가 아니라 trait requirement여야 한다.

```text
MCallable_Trait
  - RTraitFuncDecl* requirement
  - applied trait identity / trait arguments
  - trait function type arguments
  - receiver
```

lowering은 conformance availability에 따라 다음을 선택한다.

| 상황 | lowering |
|---|---|
| concrete local/external conformance가 확정됨 | `RImplTraitFuncDecl` 또는 exported witness symbol의 direct call |
| `T : Trait` generic constraint | caller-provided trait table/dictionary entry call |
| `some Trait` opaque value | opaque metadata의 trait witness entry call |
| 향후 class/interface runtime dispatch | `Virtual` call 계열 |

MIR에서 semantic `Trait` call을 곧바로 `Direct`/`TraitTable`로 나눌지, QIR/lowering 단계에서 나눌지는 미확정이다. direct/indirect return 및 parameter passing mode는 call dispatch 종류와 별개다.

## Open Points

- impl body에서 impl generic binder, target struct member, target struct outer, lexical outer를 어떤 lookup priority로 결합할지
- `RName_Impl(index)`를 어떤 owner-local stable ordinal로 정하고 diagnostics/printer에는 어떻게 표시할지
- canonical impl과 separate extension bundle impl의 `RImplTraitDecl` tree outer 및 conformance entry 연결 규칙
- trait table ABI에서 generic constraint dictionary, opaque metadata, external witness symbol을 어떤 공통 interface로 표현할지
