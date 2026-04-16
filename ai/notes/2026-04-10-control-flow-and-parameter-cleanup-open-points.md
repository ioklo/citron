# 회의 / 실험 노트

Date: 2026-04-10
Title: return/fallthrough 검사, QIR synthetic block, parameter cleanup convention 논의

Summary
- `inline` 안의 `return`처럼 expression 내부에서 함수 전체 control-flow가 끝나는 경우, MIR에서 정확히 return completeness를 판정하기가 복잡하다는 점을 확인했다.
- MIR에서 복잡한 flow analysis를 만들기보다, QIR lowering의 `Ready`/`Done` 결과를 이용해 missing return을 판단하는 방향을 검토했다.
- `inline_exit`처럼 미리 만들어 둔 synthetic join block이 실제로는 연결되지 않아 unreachable block으로 남는 문제를 논의했다.
- 함수 parameter의 construct/destroy 책임을 caller/callee 중 어디에 둘지, C++ ABI와 Citron의 `MTopLevel_*` cleanup 경계를 기준으로 비교했다.
- parameter cleanup convention은 아직 확정하지 않고 다음 논의로 넘긴다.

Context
## 1) `inline` 내부 `return`과 return completeness
다음과 같은 코드에서 `inline` expression 내부의 `return`은 함수 전체를 종료한다.

```citron
int F()
{
    bool b = inline { return 4; };
}
```

- MIR 구조만 보고 함수가 fallthrough 가능한지 정확히 판단하려면 `inline`, `leave`, `return`, `break`, labeled break, `try`/`catch` 등을 모두 고려해야 한다.
- 처음에는 MIR에서 `Body(vector<MStmt*> stmts)`를 앞에서부터 순회하며 `MayContinue`/`NeverContinue` 같은 flow summary를 계산하는 방향을 검토했다.
- 그러나 labeled `break`, `leave`, loop/switch/inline target 소비까지 들어가면 MIR에서 작은 CFG 분석기를 다시 만드는 모양이 된다.

Decision Candidates
- MIR에서는 복잡한 return completeness 분석을 하지 않는다.
- QIR lowering 결과가 `Ready`인지 `Done`인지로 최종 fallthrough 여부를 판단한다.
- void 함수에 대해서는 MIR 단계에서 복잡한 복합 statement 분석 없이, 마지막 direct statement가 `MStmt_Return`이 아니면 synthetic `return;`을 추가하는 단순 보정만 검토한다.

예상 정책:
- void 함수:
  - 마지막 direct stmt가 `MStmt_Return`이 아니면 synthetic `return;` 추가
  - 복합 stmt 내부가 항상 return하는지는 MIR에서 보지 않는다
- non-void 함수:
  - MIR에서 trailing return을 만들지 않는다
  - QIR body lowering 결과가 `Ready`로 끝나면 missing return 에러
  - `Done`으로 끝나면 fallthrough가 없으므로 OK

Notes
- 이 방식은 unreachable synthetic return이 생길 수 있다.
- 나중에 warning 품질을 위해 source stmt와 synthetic stmt를 구분하는 origin marker가 필요할 수 있다.

## 2) QIR synthetic block과 unreachable verifier
다음 예시에서 `inline_exit` block이 미리 만들어졌지만 실제로 연결되지 않을 수 있다.

```citron
int F()
{
    bool b = inline { return 4; };
    return 2;
}
```

- `inline` lowering에서 미리 `inline_exit` block을 만들어 두고, body가 끝나면 그쪽으로 jump하게 준비한다.
- 그런데 body 안에서 `return 4;`가 실행되면 inline expression 자체가 `Done`이 되어 `inline_exit`로 가는 edge가 생기지 않는다.
- 이후 `VerifyBlocks`가 unreachable block을 발견할 수 있다.

논의한 대응:
- verifier warning으로 계속 남기는 것은 부적절하다. 이것은 사용자 unreachable code라기보다 lowering convenience로 생긴 unused synthetic block이다.
- 가능하면 join block을 lazy하게 만든다.

예상 구조:

```cpp
QBlock* inlineExitBlock = nullptr;

auto getInlineExitBlock = [&]() -> QBlock*
{
    if (!inlineExitBlock)
        inlineExitBlock = bodyContext.AddBlock("inline_exit");
    return inlineExitBlock;
};
```

- `leave`가 실제로 발생하거나 inline body가 `Ready`로 끝날 때만 `getInlineExitBlock()`을 호출한다.
- inline body가 `Done`이고 leave가 없었다면 exit block은 생성하지 않는다.
- 만약 미리 block을 만드는 구조를 유지한다면, `Synthetic`/`Source` 같은 block origin을 두고 unused synthetic block은 pruning 또는 verifier ignore 대상으로 분리하는 방안도 있다.

## 3) parameter construct/destroy convention
다음 형태의 함수 호출에서 parameter와 return value lifetime 책임을 어디에 둘지 논의했다.

```citron
S F(S s)
{
    S s2;
    ...
}
```

검토한 축:
- return value `S`
  - caller가 return destination storage 준비
  - callee가 return object construct
  - caller가 이후 destroy
- local `S s2`
  - callee가 construct/destroy
- by-value parameter `S s`
  - caller가 argument expression을 알고 있으므로 argument/parameter object construct는 caller 쪽이 자연스럽다
  - destructor를 caller가 할지 callee가 할지는 convention 선택 사항

C++ ABI 참고:
- Itanium C++ ABI 계열은 non-trivial by-value parameter에 대해 대체로 caller construct + caller destroy 쪽이다.
- MSVC/Microsoft C++ ABI 계열은 callee destroy 쪽으로 알려져 있다.
- C++ 표준은 parameter가 function exit 시 파괴되는지 enclosing full-expression 끝에서 파괴되는지를 implementation-defined로 둔다.

논의한 예시 1:

```cpp
const std::string& foo(std::unique_ptr<std::string> ptr) {
    return *ptr;
}
```

- callee-destroy라면 `foo`가 반환한 직후 parameter `ptr`이 파괴되어 반환 reference가 바로 dangling이 된다.
- caller-destroy + full-expression end라면 호출 expression이 끝날 때까지 `ptr`이 살아 있어, 같은 full-expression 안에서는 reference가 우연히 유효할 수 있다.
- 둘 다 의존하면 안 되는 위험한 코드다.

논의한 예시 2:

```cpp
std::unique_lock<std::mutex> with(std::mutex& m) {
    return std::unique_lock<std::mutex>(m);
}

bool sink(std::unique_lock<std::mutex> lk) {
    return true;
}

std::mutex m;
int main() {
    return sink(with(m)) && sink(with(m));
}
```

- caller-destroy를 full-expression 끝까지 미루면 첫 번째 `sink(with(m))`의 `unique_lock`이 `&&` 오른쪽 평가 전까지 파괴되지 않아 deadlock 가능성이 있다.
- 이 문제의 본질은 Itanium 자체가 아니라, parameter destruction을 enclosing full-expression 끝까지 미루는 lifetime 선택이다.

Citron 현재 방향 관련 관찰:
- Citron은 이미 `MTopLevel_*` 경계마다 temporary cleanup을 처리하는 방향이다.
- 따라서 caller-destroy를 택하더라도 C++ Itanium처럼 statement/full-expression 끝까지 미루지 않고, call 직후 또는 해당 `MTopLevel_Call` 종료 시점에서 parameter object를 destroy하는 convention을 선택할 수 있다.
- 이 경우 constructor/destructor 쌍이 caller call-lowering에 같이 보이고, full-expression lifetime 연장 문제도 피할 수 있다.

Open Points
- Citron internal convention을 caller-destroy parameter로 둘지, callee-destroy parameter로 둘지 아직 확정하지 않는다.
- caller-destroy를 택한다면 destroy 시점은 C++ Itanium식 full-expression end가 아니라 `MTopLevel_Call` 종료 시점으로 둘 가능성이 있다.
- callee-destroy를 택하면 parameter lifetime이 function body scope와 일치하지만, constructor와 destructor가 caller/callee에 나뉜다.
- external ABI compatibility는 Citron internal convention과 별도로 adapter/thunk로 처리할지 검토가 필요하다.
- QIR에 특정 C++ ABI convention을 너무 일찍 박지 않을지, 혹은 Citron internal ABI를 하나로 고정하고 external ABI adapter에서 변환할지 결정해야 한다.

Action Items
- [ ] QIR lowering에서 `inline_exit` 같은 join block을 lazy creation으로 바꿀 수 있는지 검토
- [ ] void 함수 trailing synthetic `return;` 삽입 정책을 MIR에서 단순화할지 결정
- [ ] `MStmt_Return` 또는 stmt/block에 source/synthetic origin marker가 필요한지 검토
- [ ] by-value parameter의 Citron internal construct/destroy convention을 다음 논의에서 확정
- [ ] Citron internal convention과 외부 C++ ABI adapter 전략을 분리해서 정리

Update: 2026-04-11

Follow-up decision
## 4) Citron internal ABI를 먼저 고정하고, 외부 함수 경계에서 ABI별 lowering을 적용한다
- 후속 논의에서는 QIR 자체를 특정 외부 C++ ABI에 직접 종속시키기보다, **Citron만의 internal ABI를 하나 정하고** 이를 기본 lowering 규약으로 사용하는 방향을 우선 선호한다.
- 즉, 프로젝트 전체가 단일 external ABI를 강제로 따른다고 보기보다:
  - Citron 내부 함수 호출/정의는 `Citron internal ABI`로 lowering
  - 외부 인터페이스 함수(`extern`, DLL import, foreign C++ symbol 등)는 선언/심볼에 연결된 ABI 정책에 따라 별도 lowering 또는 adapter/thunk를 사용
- 이 방식이면 MIR은 계속 ABI-neutral하게 유지하고, QIR lowering 단계에서만 ABI policy를 입력으로 받아 필요한 차이를 반영할 수 있다.

정리:
- internal call: 항상 Citron ABI
- external boundary call: 대상 ABI(MSVC, Itanium 등)에 맞는 call lowering
- 필요 시 wrapper/thunk를 생성하여 internal ABI와 external ABI를 연결

Rationale
- QIR 전체를 ABI dialect별로 분기시키면 verifier, optimization, reasoning surface가 target마다 갈라질 수 있다.
- 반대로 internal ABI를 먼저 하나 고정하면 QIR의 기본 shape를 안정적으로 유지할 수 있다.
- 외부 ABI 차이는 주로 호출 경계에서 드러난다.
  - by-value parameter construct/destroy owner
  - indirect return/sret 여부
  - hidden parameter shape
  - member call / ctor / dtor signature details
- 따라서 이 차이를 외부 인터페이스 lowering 또는 adapter 계층으로 국소화하는 편이 전체 구조를 단순하게 만든다.

Expected impact on QIR
- QIR lowering API는 ABI policy를 입력으로 받는다.
- 다만 QIR 타입/노드 자체는 가능한 한 공통 모델을 유지한다.
- ABI 차이는 "어떤 cleanup owner를 선택하는가", "return을 direct/sret 중 무엇으로 lower하는가" 같은 정책 결정으로 주입한다.
- 가능하면 `MSVC 전용 QIR`, `Itanium 전용 QIR`처럼 IR dialect를 늘리기보다, 공통 QIR + ABI-aware lowering 형태를 유지한다.

Recommended layering
1. MIR
- ABI-neutral
- language semantics 중심

2. QIR lowering
- 입력: MIR + ABI policy
- 기본 정책: Citron internal ABI
- 외부 선언 호출 시: 선언에 연결된 foreign ABI policy 사용

3. LLVM / backend boundary
- 실제 target calling convention, symbol lowering, thunk emission 반영

Open points after follow-up
- Citron internal ABI의 by-value parameter destroy owner를 caller/callee 중 어느 쪽으로 확정할지
- internal ABI에서 caller-destroy를 택한다면 cleanup 시점을 `call 직후`와 `MTopLevel_Call 종료 시점` 중 어디로 둘지
- 외부 함수 선언에 ABI 속성을 어떻게 부여할지(`extern(msvc)`, `extern(itanium)` 등)
- thunk를 항상 명시적으로 둘지, QIR lowering이 직접 foreign ABI call을 뱉도록 할지
- class layout, mangling, RTTI, exception 같은 "type/module ABI" 문제를 call ABI와 어디까지 분리할지
