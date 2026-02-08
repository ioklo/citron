# Struct ctor lowering 논의 (2026-02-05)

## 배경
- SyntaxIR0Translator는 SExp → ImExp → ReExp → MExp/MLoc/MOperand 흐름.
- 현재 `S(...)`는 `MExp_NewStruct`로 내려감.
- 목표 방향: “최적화는 명시적으로” 하되, 표현력/의미 일관성을 유지하고 싶음.

## 핵심 논의 요약
1) `S(...)`를 MExp에서 제거하고 위치별로 직접 ctor 호출을 하려는 아이디어가 있었음.
2) 하지만 `S(...)`는 VarDecl/Assign/Call 외에도 표현식 문맥에서 자주 필요함:
   - return, ternary, list/collection, 중첩 호출, 메서드 체이닝 등.
3) 그래서 `S(...)`를 **표현식(MExp)** 으로 유지하는 편이 범용적이고 단순함.
4) “최적화는 명시적으로”를 유지하려면:
   - 의미(semantics)는 동일하게 두고,
   - 특정 문맥(VarDecl/Assign/Call 등)에서 **동치 변환을 반드시 적용**하는 규약으로 강제하는 방식이 적절.

## 결론/방향
- `S(...)`의 의미는 일반 표현식으로 유지.
- 다만 VarDecl/Assign/Call 문맥에서는 **in-place ctor 호출로 lowering을 반드시 수행**하는 “컴파일 규약”을 둔다.
- 즉 “semantic이 다르다”가 아니라 “동치 변환을 강제한다”로 문서화.

## 주의점
- “반드시” 규칙은 동치성이 확실해야 함:
  - 부작용 순서, 임시 수명, 예외, 오버로드 해석 등이 동일해야 함.
- 동치성 보장이 어렵다면 강제 규칙을 완화해야 함.
