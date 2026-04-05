# 회의 / 실험 노트

Date: 2026-04-04
Title: `for` lowering의 outer scope 표현과 scope annotation 방향

Summary
- `for`의 initializer 선언 범위를 MIR에서 명시적으로 드러내기 위해 도입한 `MStmt_Scope` 방향을 유지하기로 했다.
- `for`의 경계를 더 잘 보이게 하기 위해 `MStmt_For` 바깥쪽을 별도 `For`로 감싸고 내부에 `ForCore`를 두는 구조는 채택하지 않는다.
- `MScopeKind`는 break/continue target 같은 semantic 기능이 있을 때만 쓰고, readability/debug 목적의 출처 표식은 별도 annotation으로 분리하는 쪽을 우선 선호한다.

Context
- 현재 `for (int i = 0; i < count; i++) body;`는 MIR에서 대체로 다음 canonical form으로 내려간다.

```cpp
MStmt_Scope {
    int i = 0;
    MStmt_For(cond, cont, MStmt_Scope { body; })
}
```

- 이 구조는 lexical scope를 `MStmt_Scope`로 명시한다는 최근 방향과 잘 맞는다.
- 다만 MIR dump를 볼 때, 바깥 `scope`가 단순 block인지 `for-init scope`인지 즉시 드러나지 않아 `for`의 경계가 다소 흐려 보인다는 문제가 있었다.
- 이를 해결하는 후보로 다음과 같은 구조를 검토했다.

```cpp
MStmt_For {          // outer
    MStmt_Scope {
        int i = 0;
        MStmt_ForCore(cond, cont, MStmt_Scope { body; })
    }
}
```

Decision
- `for`의 canonical MIR form은 기존처럼 `Scope + For` 조합을 유지한다.
- `MStmt_ForCore` 같은 전용 inner stmt는 도입하지 않는다.
- 이유:
  - `MStmt_Scope`는 lexical scope를 나타낸다는 최근 정리와 일관된다.
  - `ForCore`는 "항상 `For` 안에만 있어야 한다" 같은 추가 invariant를 만들고, visitor/lowering/verifier 표면을 늘린다.
  - 최근 `MStmt_IfBind` 제거 결정과 마찬가지로, 의미 차이가 충분히 크지 않은 특수 stmt kind 증식은 피하는 편이 낫다.

- 현재 권장 canonical form:

```cpp
MStmt_Scope(Default) {
    init...
    MStmt_For(cond, contStmt, bodyScope)
}
```

- 여기서:
  - outer `MStmt_Scope`는 `for` initializer의 lexical scope를 나타낸다.
  - inner `bodyScope`는 loop body scope를 나타낸다.
  - break/continue target identity는 loop body 쪽의 loop-related semantic 정보가 담당한다.

- `MScopeKind`에는 semantic 기능이 있는 경우만 싣는다.
  - 예: loop/switch target 식별, `labelId` 연결, reachable target 판정
- `for-init scope`처럼 주로 사람이 MIR를 읽기 쉽게 하기 위한 정보는 `MScopeKind`에 바로 싣기보다, 별도 debug/source-origin annotation으로 분리하는 쪽을 우선 선호한다.

Rationale
- 2026-04-03 note에서 `MStmt_Scope`와 `MTopLevel_*`를 서로 다른 축의 정보로 분리한 것처럼, 여기서도 semantic annotation과 debug annotation을 분리하는 편이 자연스럽다.
- `MScopeKind`가 semantic consumer를 위한 태그인지, printer용 장식인지가 섞이기 시작하면 이후 verifier와 lowering 판단 기준이 흐려질 수 있다.
- 반면 debug/source-origin annotation은 downstream correctness와 분리해서 더 자유롭게 붙였다 뗄 수 있다.
- 따라서 다음과 같은 역할 분리가 바람직하다.
  - `MScopeKind`: semantic annotation
  - debug/source-origin annotation: readability, dump, tracing 보조 정보

Open Points
- `MStmt_Scope`에 debug/source-origin annotation을 실제로 추가할지, 아니면 printer가 `Scope + For` canonical pattern을 인식해 재구성 출력할지
- loop identity를 `MStmt_For` 자체와 `MStmt_Scope(MScopeKind_Loop)` 중 어디에 귀속시키는 것이 이후 lowering에 더 일관적인지
- `while`, `foreach` 등 다른 loop 계열에도 같은 annotation 원칙을 동일 적용할지

Action Items
- [ ] `for`의 canonical MIR form을 `Scope + For`로 보는 규칙을 구현/메모 차원에서 명문화
- [ ] `MScopeKind`와 debug/source-origin annotation의 역할 분리 기준 정리
- [ ] `MPrinter`에서 `for-init scope`가 더 잘 드러나게 하는 방법 검토
