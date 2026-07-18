# 회의 / 설계 노트

Date: 2026-07-18
Title: module declaration lookup과 accessibility check의 분리

## Decision

module import는 provider의 모든 declaration name을 compiler의 lookup surface에 올린다. `public`/`private`/`protected`는 name을 숨기는 lookup filter가 아니라, 이름을 찾은 뒤 해당 declaration을 현재 context에서 사용할 수 있는지 판정하는 accessibility rule이다.

```text
import / declaration reachability
    -> name lookup candidate discovery
    -> type argument / overload matching
    -> accessibility check
```

따라서 inaccessible declaration도 같은 scope의 accessible outer declaration을 shadow한다. lookup은 발견한 inner candidate에서 멈추며, access failure를 이유로 outer scope를 다시 탐색하지 않는다.

```citron
// Imported module A
private struct S { }

// Consumer
S value; // S를 찾은 뒤 accessibility error; 다른 outer S로 fallback하지 않음
```

## Reachability, Accessibility, ABI

- module의 모든 declaration name과 typecheck에 필요한 semantic metadata는 import consumer compiler에 reachable하다.
- `public`/`private`/`protected`는 source context의 use permission을 정한다.
- 이 declaration-metadata surface는 public API/ABI contract 또는 linker symbol export와 동일하지 않다. private declaration의 ABI stability/export policy는 별도로 관리한다.
- public declaration의 signature가 inaccessible type을 직접 노출해 consumer가 사용할 수 없게 되는 경우의 inconsistent accessibility error는 유지한다.

## Trusted Extension

external extension/impl은 trusted augmentation context다. ordinary consumer와 extension compiler는 같은 private declaration을 lookup candidate로 얻을 수 있지만, access policy가 다르다.

```text
ordinary consumer context  -> private access error
trusted extension context  -> target private member access 허용
```

이로써 extension compiler용 별도 private name lookup surface를 둘 필요는 없다. 필요한 것은 private semantic/ABI metadata를 포함한 declaration reachability와 context-aware accessibility checker다.

## Consequences

- `visibility`를 “이름을 lookup할 수 있는가”와 동의어로 쓰지 않는다. import된 module declaration은 lookup candidate가 될 수 있다.
- `Resolve*`는 candidate discovery/lexical shadowing을 담당하고, accessibility checker는 resolution 후 별도로 적용한다.
- `var`로 inaccessible concrete type의 값을 받는 기존 허용 규칙은 유지한다. explicit type annotation 등으로 이름을 쓰면 name-not-found가 아니라 accessibility error가 난다.

