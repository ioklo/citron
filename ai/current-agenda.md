# Current Agenda

## Topic
canonical trait conformance와 `impl` 구현

## Current Direction
- `RDecl`/`RNode` 정리는 완료했다. semantic tree 재구성은 현재 작업 주제가 아니다.
- 첫 trait 구현 범위는 원본 module의 canonical conformance로 제한한다: `struct S : Trait`와 대응 `impl S : Trait`.
- generic canonical impl은 `impl S<U> : Trait<U>`처럼 target pattern에 type parameter를 드러내는 표기로 구현한다. `U`는 header가 도입하며 `struct S<T> : Trait<T>`와 alpha-equivalent한 universal target으로 정규화한다.
- `where`를 가진 full generic impl과 direct specialization은 초기 범위에서 제외한다. specialization/conditional conformance는 named extension bundle과 `extend` activation 경로로 둔다.
- 먼저 trait declaration/type, struct trait 목록, witness `impl` declaration을 RSymbol과 SmTranslator skeleton 단계에 연결한다.
- 이름 있는 외부 `extension` bundle, 소비자 `extend` activation, overlap/ambiguity 처리는 후속 단계다.
- `impl`은 named `RDecl`이 아니라 struct/extension의 typed internal `N*Info` payload로 두고, public conformance header만 RSymbol surface에 남기는 방향이다.
- SmTranslator는 unit 간에는 global phase barrier를 유지하고, unit 내부의 세밀한 선행 조건은 order가 있는 task dependency로 표현하는 방향을 검토한다.

## Current Refactoring State
- declaration 구현과 주 번역 경로는 `NSymbol`에서 `RSymbol`로 이행됐다. `RFactory`가 `RDecl`을 소유·생성한다.
- `NSymbol`에는 현재 `NFactory` wrapper와 일부 비주력 target/test의 old API 참조가 남아 있다.
- trait/impl은 parser/AST까지만 연결돼 있으며, `RTraitDecl`, trait type/factory, SmTranslator visitor/task는 아직 구현 대상이다.
- 자세한 이행 범위와 잔재는 `ai/wiki/compiler/nsymbol-rsymbol-migration.md`를 본다.

## Recently Discussed Points
- namespace 수준의 `public/private`는 별도 export 키워드가 아니라 accessibility를 통해 export 의미를 포함한다.
- class member는 `public/protected/private`를 갖고, struct member는 `public/private`만 갖는다.
- struct는 현재 상속 불가 방향으로 정리되어 있으므로 struct `protected`는 두지 않는다.
- accessor는 namespace/class/struct에서 이름이 겹치더라도 의미 공간이 다르므로, 단일 universal accessor보다 context별 accessor 분리가 더 자연스럽다는 쪽으로 기울어 있다.
- 다만 이 accessor를 `RNode` 본체에 직접 넣기보다, declaration payload나 별도 accessibility policy 계층에 두는 쪽이 현재 `RNode` 방향과 더 잘 맞는다.

## Open Questions
- accessor를 정확히 어느 계층에 둘지: declaration payload, category view, 별도 metadata 중 어디가 가장 자연스러운지
- lookup / resolver 책임과 `RNode` 책임의 경계를 어디까지 나눌지
- nested type의 accessibility를 tree membership과 declaration accessibility 사이에서 어떻게 모델링할지
- generic conformance header의 generic signature, self type-argument pattern, trait type arguments, constraint를 어느 RSymbol API로 노출할지. Source header에서는 `impl S<U>`/`impl Bundle<U>`의 target pattern parameter와 `extend<U>` activation parameter를 구분해 표현한다.
- generic bundle pattern의 overlap을 `where` constraint까지 포함해 activation 시점에 어떻게 판정할지
- trait별 independent witness/conformance identity를 유지하면서 관련 trait 구현의 공용 helper/member를 bundle-private scope나 별도 mechanism으로 어떻게 제공할지. impl block에 trait requirement 밖 member를 허용할지 여부
- unit-local task graph의 freeze 시점, task order enum, failure propagation 규칙

## Update Rule
- 현재 주제가 바뀌면 이 파일을 먼저 갱신한다.
- 완전히 확정된 규칙만 `ai/wiki/current-decisions.md`로 옮긴다.
- 세부 대화나 실험 결과는 `ai/notes/`에 남기고, 이 파일에는 현재 작업을 이어가기 위한 최소 맥락만 유지한다.
