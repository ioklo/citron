# Current Agenda

## Topic
`RDecl` 제거와 `RNode` 기반 semantic tree 재구성

## Current Direction
- semantic tree의 중심은 `RNode`로 옮긴다.
- `RNode`는 우선 tree / name / member relation을 담당하는 lightweight node로 유지한다.
- 기존 `RDecl` 계층은 점진적으로 역할을 잃게 만들고, 충분히 이행되면 제거한다.
- declaration category, richer metadata, accessibility policy는 `RNode` 본체보다 별도 payload / checker / policy 계층으로 두는 방향을 우선 검토한다.

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
