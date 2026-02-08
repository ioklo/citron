# ADR: RDecl을 런타임/중간 선언으로 사용하기

**Date**: 2026-02-05

**Status**: accepted

Context
- 컴파일 파이프라인에서 구문 분석 이후에 생성되는 심볼은 여러 역할(소스-기반 선언, 외부 노출 선언, 컴파일 중 내부 선언)을 가진다.

Decision
- 세 가지 선언 모델을 명확히 구분한다: `NDecl`(소스 선언), `EDecl`(모듈 외부 노출 선언), `RDecl`(컴파일 중 내부 선언 인터페이스).

Alternatives
- 모든 선언을 하나의 타입으로 관리: 단순하지만 컴파일 단계별 책임 분리가 어려움.

Consequences
- 각 단계(예: `SyntaxIR0Translator`)는 필요한 `RDecl`을 생성/등록해야 함.
- `EDecl`은 `REDecl`로 래핑하여 `RDecl` 인터페이스를 제공하고, `NDecl`은 `RDecl`을 상속하여 구현한다.

RelatedFiles
- `src/SyntaxIR0Translator/*` — `RDecl` 생성과 MIR 산출 관련 로직
- `src/R...` (심볼 관련 폴더들) — 선언 타입 구현

FollowUp
- `ai/implementations/syntaxir0translator-implementation.md`에 `RDecl` 생성 코드 위치(참조 라인)를 추가할 것.
