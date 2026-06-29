# Design Principles

Status: draft current
Area: language, philosophy, name lookup
Keywords: explicit, resolution, shadowing, overload, accessibility

Citron 표면 언어를 정할 때 반복해서 유지하려는 성향을 모아 둔 페이지다.

## Current Direction
- Citron은 explicit한 언어를 지향한다.
- 다만 explicit함이 같은 의미를 여러 문법으로 중복 표현하게 만드는 방향으로 가는 것은 피한다.
- 기본 resolution은 단순하고 일관되게 유지하되, 사용자가 원하면 더 장황하더라도 특정 대상을 명시적으로 가리킬 수 있는 경로를 제공한다.

## Explicit, But Without Redundant Meaning
- 언어는 의도가 중요한 곳에서는 explicit한 표기를 선호한다.
- 반대로 같은 의미를 반복해서 적게 만드는 문법은 피한다.
- 예를 들어 이미 outer kind로 accessor 의미가 정해지는 자리에서는 class/struct에 불필요하게 같은 의미의 추가 표기를 늘리지 않는 방향을 선호한다.

## Stable Resolution First
- 기본 name lookup과 overload selection은 가까운 선언, 현재 scope, 현재 규칙을 기준으로 안정적으로 동작해야 한다.
- scope가 달라졌다는 이유만으로 전혀 다른 선언으로 조용히 fallback하는 해석은 선호하지 않는다.
- 이름을 찾았다면, 그 이름이 가리키는 선언과 접근 가능 여부를 분리해서 판단하는 쪽을 선호한다.

## Explicit Escape Hatch
- Shadowing, overload ambiguity, private member, nested declaration 같은 이유로 기본 resolution에서 원하는 대상을 바로 고를 수 없더라도, 사용자가 그 대상을 명시적으로 다시 가리킬 수 있는 수단을 제공하는 쪽을 선호한다.
- 즉 "기본 해석은 단순하게, 예외적 접근은 명시적으로"라는 원칙을 유지한다.
- 이 escape hatch의 정확한 surface syntax는 topic별로 후속 결정한다.

## Consequences
- private / not-accessible declaration도 semantic model에서 식별 가능하게 유지하고, 접근 가능성은 별도 accessibility rule로 판정하는 방향과 잘 맞는다.
- shadowing된 상위 scope 이름, 기본 규칙으로 선택되지 않은 overload, 바깥 declaration path도 필요하면 다시 지목할 수 있어야 한다.
- 사용자 입장에서는 자동 우회보다 명시적 재지정이 가능하다는 점이 더 중요하다.

## Open Points
- shadowed outer scope name에 접근하는 explicit syntax
- overload set에서 특정 candidate를 지목하는 explicit syntax
- private member나 hidden nested declaration을 source에서 어떤 형태로 재지정할지

## History
- `ai/notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
