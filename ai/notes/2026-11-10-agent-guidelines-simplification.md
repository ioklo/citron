# 에이전트 지침 간소화와 오래된 의미론 요약 검토

Date: 2026-11-10
Status: AGENTS.md 간소화 및 사용자 승인 후 Core Semantics wiki 통합 완료

- 사용자가 AGENTS.md 간소화를 요청했고 Citron_Core_Semantics_for_Codex.md의 노후화를 지적했다.
- AGENTS.md에서 중복 체크리스트, 빌드 명령, 산출물 목록, 오래된 선언 모델 설명을 제거하고 역할·권한·설계 논의·맥락·수정/검증 규칙과 참고 경로로 정리했다.
- 사용자가 확정한 표현 해석을 유지했다. “~있을까”, “~할 수 있나”는 질문이고, “해줘”, “해줬으면 좋겠어”처럼 명시적으로 요청할 때 소스를 수정한다.
- Core Semantics 요약을 현재 의미론의 최우선 기준으로 사용하지 않도록 AGENTS.md에 명시했다. 새 언어 규칙을 확정하거나 기존 의미론을 변경한 것은 아니다.

## 오래된 설명과 검토 근거

- 기존 AGENTS.md는 RNode 중심 재구성과 RDecl 계층 제거를 현재 작업으로 설명했다. current agenda는 해당 정리가 완료됐고 현재 작업 주제가 아니라고 기록한다. 요약을 다시 복제하지 않고 현재 기록으로 안내한다.
- Core Semantics는 authoritative/strict 규칙을 선언하지만, reference의 역할과 lifetime 진단 설명은 현재 `ai/wiki/language/types-and-references.md` 및 `ai/notes/2026-05-14-reference-and-memory-safety-policy.md`의 제한된 reference surface와 전역 lifetime safety 비보장 정책을 충분히 반영하지 않는다.
- Core Semantics의 `S s;` 설명은 trivial constructor 조건과 미초기화 예시 사이에서도 혼동 여지가 있다. 이 검토에서 정답 문법이나 초기화 규칙을 새로 정하지 않았다.
- `docs/` 보호 규칙에 따라 Core Semantics 본문은 수정하지 않았다. 본문 정리 또는 폐기는 별도 승인 후 다룬다.

## 검증

- 문서만 변경했다. 참고 링크의 대상 존재와 diff를 확인했다.
- 소스 수정, 빌드, 테스트 실행은 하지 않았다.
## 후속 승인과 통합

- 사용자가 Core Semantics 문서를 제거하고 wiki에 통합하는 방향을 승인했다. 위의 docs 본문 유지 기록은 승인 전 상태다. 이번 삭제는 해당 문서에 대한 개별 승인 범위이며 일반 docs 보호 규칙은 유지한다.
- `docs/Citron_Core_Semantics_for_Codex.md`를 삭제했다. 원문은 git history로 확인한다.
- `ai/wiki/language/core-semantics.md`에 기존 12개 주제와 generic 용어의 현재 wiki 경로를 연결했다. 기존 wiki로 설명 가능한 내용은 복제하지 않았다.
- 기존 wiki에 없거나 상충하는 초기화·trivial constructor·scope exit·parameter modifier·shared 생성 문법·enum pattern·tal 관련 주장은 확인 필요 항목으로 남겼다. 명세가 새로 확정되었다고 기록하지 않았다.
- TypeParam/TypeVar/TypeArg 용어 설명을 compiler/declaration-model.md에 통합했다. 과거 parameter 구현 클래스 목록은 현재 모델로 옮기지 않았다.
- language index에 새 입구를 등록하고 AGENTS.md는 현재 wiki로 직접 안내하도록 정리했다.
- 문서 링크와 diff를 확인했다. 소스 변경이나 빌드·테스트 실행은 하지 않았다.
