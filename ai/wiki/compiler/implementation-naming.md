# Implementation Naming

Status: current
Area: compiler
Keywords: naming, Resolve, Get, outer, base, RDeclRes, implementation

Citron compiler implementation에서 declaration / symbol / resolver 주변 이름을 붙일 때의 현재 기준이다. 이 문서는 언어 규칙보다 낮은 레벨의 구현 네이밍 룰을 기록한다.

## Current Rules

- `RDeclRes`를 리턴하는 lookup 함수는 `Resolve`로 시작한다.
  - 예: `ResolveIdentifier`, `ResolveVar`
  - 단순 보유물 직접 접근이나 이미 정해진 index/key 접근은 `Get`을 사용할 수 있다.

- symbol / declaration 문맥에서 containing tree edge는 `outer`라고 부른다.
  - 예: `GetOuter`, `RTypeDeclOuter`

- inheritance edge는 `base`라고 부른다.
  - 예: `baseClass`, `GetBase...`

- `parent`는 containing relation과 inheritance relation을 흐리므로 symbol / declaration tree naming에서는 피한다.

## Notes

- `Resolve`는 이름 lookup, overload filtering, fallback scope search처럼 탐색 의미가 있는 API에 붙인다.
- `Get`은 이미 소유하거나 계산 대상이 명확한 값을 가져오는 API에 붙인다.
