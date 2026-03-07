# Member Translation Snapshot

Updated: 2026-03-06

Summary
- `IrExpAndMemberName...` 계열 translator는 전면 통합하지 않는다.
- 공통 해석 단계만 helper로 추출하고, 산출물 생성은 각 translator가 유지한다.
- static/instance 제약과 kind mismatch는 번역 단계에서 즉시 진단한다.

Implementation Direction
- `ResolveMemberOnStaticBase(...)` 같은 helper를 `SyntaxIR0Translator` 내부 공용 파일로 둔다.
- typed getter 또는 wrapper를 우선 도입하고, 오류 모델은 `expected<T, Error>` 계열로 잡는다.
- verifier는 debug/CI 안전망으로만 사용한다.

Hot Areas
- `src/SyntaxIR0Translator/IrExpAndMemberName*`
- `src/SyntaxIR0Translator/`
- `src/R...`
