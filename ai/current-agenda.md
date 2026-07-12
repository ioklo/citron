# Current Agenda

## Topic
canonical trait conformance와 `impl` 구현

## Current Direction
- `RDecl`/`RNode` 정리는 완료했다. semantic tree 재구성은 현재 작업 주제가 아니다.
- 첫 trait 구현 범위는 원본 module의 canonical conformance로 제한한다: `struct S : Trait`와 대응 `impl S : Trait`.
- 먼저 trait declaration/type, struct trait 목록, witness `impl` declaration을 RSymbol과 SmTranslator skeleton 단계에 연결한다.
- 이름 있는 외부 `extension` bundle, 소비자 `extend` activation, overlap/ambiguity 처리는 후속 단계다.

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

## Update Rule
- 현재 주제가 바뀌면 이 파일을 먼저 갱신한다.
- 완전히 확정된 규칙만 `ai/wiki/current-decisions.md`로 옮긴다.
- 세부 대화나 실험 결과는 `ai/notes/`에 남기고, 이 파일에는 현재 작업을 이어가기 위한 최소 맥락만 유지한다.
