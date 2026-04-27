# 회의 / 설계 노트

Date: 2026-04-23
Title: Intrinsic ABI signature 표현력과 declaration 배치 정리

Status
- current

Summary
- `HandleIntrinsicCall`, `HandleCall`, `HandleCallCore`로 call ABI 조립을 공통화하는 방향은 유지한다.
- 문제의 핵심은 `QFuncInfo`가 아니라, `QFuncInfo`를 계산하기 위한 intrinsic signature metadata의 표현력이 `RFuncDecl`보다 약하다는 점이다.
- 단순한 `retType + paramTypes` 수준의 `QIntrinsicInfo`로는 generic 반환 타입, `[in]/[move]/[forward]/[params]` 같은 parameter kind, recursive generic type을 충분히 표현하기 어렵다.
- 별도 `QSigType` 계층을 만들어 해결하는 방법은 결국 `RType`의 재귀 구조와 type variable 모델을 다시 복제하게 되므로 비용이 크다.
- 현재 단계에서는 intrinsic용 declaration/signature를 `IR0IR1Translator` 내부 구현으로 두고, `QInst_IntrinsicKind`는 lowering/evaluator/backend 식별자로만 유지하는 쪽을 우선 선호한다.

Context
- MIR -> QIR 번역기에서 함수 호출과 intrinsic 호출 모두 ABI-aware하게 처리하기 위해 `HandleIntrinsicCall`, `HandleCall`, `HandleCallCore`를 도입했다.
- `HandleCallCore`는 `QFuncInfo`를 기준으로 다음 책임을 공통으로 처리한다.
  - return passing mode
  - this passing mode
  - parameter passing mode
- 일반 함수는 `RFuncDecl`로부터 이 정보를 충분히 유도할 수 있다.
- 그러나 intrinsic은 기존 `QIntrinsicInfo`가 대체로
  - 반환 타입
  - 파라미터 타입 목록
  정도만 갖는 구조였기 때문에 다음 사례를 표현하기 어렵다.
  - `ListIterator<T> GetListIterator<T>([in] List<T>& list)`
  - `Add_String_StringInRef_StringInRef` 같이 logical signature 상 `[in]` parameter kind가 중요한 경우

Observed problem
## 1) `QIntrinsicInfo`의 단순 type list는 ABI 입력으로 부족하다
- ABI가 필요한 것은 단순한 "physical arg type list"가 아니라, 함수 수준의 signature 정보다.
- 예를 들어 `[in] List<T>&`는 단지 "pointer one slot"로 축약하면 semantic parameter kind가 사라진다.
- `GetListIterator<T>`처럼 return type이 type argument에 따라 달라지는 intrinsic은 generic-aware signature가 필요하다.

## 2) 별도 `QSigType` 계층은 결국 `RType`를 재구현하게 된다
- 처음에는 intrinsic 전용 `QCallableSignature`, `QSigType_TypeVar` 같은 별도 schema를 두는 방안을 검토했다.
- 하지만 `List<List<T>>` 같은 recursive generic type을 지원하려면,
  - type decl
  - nested type args
  - type variable
  - substitution
  를 다뤄야 하므로 `RType`의 상당 부분을 다시 만들게 된다.
- 특히 현재 `RType_TypeVar`가 declaration-space의 `RTypeParamDecl`에 묶여 있는 구조와도 어긋난다.
- 따라서 intrinsic ABI만을 위해 `RType`과 거의 동등한 또 하나의 타입 표현을 만드는 것은 과하다.

Options considered
## A) Intrinsic을 `RFuncDecl` 패밀리로 승격
- 장점:
  - 기존 `RType`, `RFuncParameter`, generic parameter 모델을 그대로 재사용할 수 있다.
  - ABI 계산 입력을 일반 함수와 거의 동일하게 맞출 수 있다.
- 우려:
  - source symbol / external symbol 모델과의 배치가 애매하다.
  - intrinsic은 source resolver가 직접 찾을 대상이 아니고, external library symbol과도 정확히 일치하지 않는다.
  - 지금 단계에서 별도 symbol family까지 만드는 것은 구현 범위를 크게 넓힌다.

## B) `QIntrinsicInfo`를 확장해 generic/typevar/parameter kind를 모두 표현
- 장점:
  - translator 내부에서 닫힌 해결책처럼 보인다.
  - source / symbol 계층을 건드리지 않을 수 있다.
- 단점:
  - type expression이 recursive해지는 순간 `RType` 복제 비용이 커진다.
  - generic과 parameter kind가 늘어날수록 사실상 mini declaration model이 된다.

## C) Intrinsic 전용 symbol family (`IntrinsicDecl`) 도입
- 장점:
  - source symbol도 external symbol도 아닌 compiler builtin declaration이라는 의미를 가장 정확히 표현할 수 있다.
  - 장기적으로는 가장 개념적으로 깔끔할 수 있다.
- 단점:
  - 현재 단계에서 새 symbol family, registry, adapter 또는 직접 구현을 모두 설계해야 한다.
  - 당장 필요한 문제는 IR0IR1Translator의 ABI 입력 표현력인데, 해결 범위가 그보다 커진다.

Decision
## 1) 현재 단계에서는 intrinsic signature declaration을 `IR0IR1Translator` 내부 구현으로 둔다
- source resolver가 intrinsic function을 이름으로 찾을 일은 없다고 본다.
- 따라서 intrinsic을 `NSymbol` 또는 `ESymbol`로 억지로 올리지 않는다.
- 또한 지금 당장 별도 `IntrinsicSymbol` family를 새 모듈로 만드는 것도 보류한다.
- 대신 `IR0IR1Translator` 내부에서만 쓰는 intrinsic callable declaration/descriptor를 두고, 이를 `QAbi`가 읽어 `QFuncInfo`를 계산하도록 한다.

의도:
- signature 표현력은 `RFuncDecl`에 가깝게 가져가되,
- scope는 translator 내부로 제한하여 변경 비용을 낮춘다.

## 2) `QInst_IntrinsicKind`는 declaration identity가 아니라 lowering identity로 유지한다
- intrinsic declaration은 "이 callable의 함수 시그니처는 무엇인가"를 표현한다.
- `QInst_IntrinsicKind`는
  - QIR emission
  - evaluator dispatch
  - LLVM lowering dispatch
  를 위한 식별자로 유지한다.
- 즉 signature와 lowering identity를 분리한다.

## 3) `QSigType` 같은 별도 recursive type hierarchy는 도입하지 않는다
- recursive generic type을 다루기 시작하면 사실상 `RType`를 다시 만들게 된다.
- intrinsic signature 표현력 강화를 위해 타입 시스템을 이중화하는 것은 피한다.
- 필요 시 translator 내부 intrinsic declaration은 `RType`/`RFuncParameter`를 최대한 재사용하는 쪽을 우선 검토한다.

## 4) symbol family 승격은 실제 수요가 생길 때 나중에 재검토한다
- 만약 intrinsic declaration을
  - semantic analysis
  - resolver
  - 여러 compiler stage
  가 공통으로 참조하게 되면, 그때 `IntrinsicDecl` family를 별도 도입하는 것을 다시 검토한다.
- 현재는 ABI-aware call lowering 문제가 주된 목적이므로, 범위를 translator 내부로 제한한다.

Rationale
- 이번 문제의 본질은 "intrinsic이 함수처럼 생긴 ABI/signature를 필요로 한다"는 점이지, 곧바로 "source/external symbol family와 같은 위상으로 승격해야 한다"는 뜻은 아니다.
- source resolver가 찾지 않는 대상이라면, declaration model을 public symbol 계층에 바로 올리지 않아도 된다.
- 동시에 physical arg type list 수준의 빈약한 metadata로는 이제 부족하므로, function-like signature abstraction은 필요하다.
- 따라서 현재 단계의 균형점은 "translator 내부 전용 intrinsic callable declaration"이다.

Open points
- translator 내부 intrinsic declaration이 `RFuncDecl`를 직접 상속할지, `QAbi` 전용의 얇은 function-like interface를 둘지
- intrinsic declaration 인스턴스를 어떤 registry/table에서 소유할지
- `MExp_CallIntrinsicKind`, `MInitExp_CallIntrinsicKind`, `QInst_IntrinsicKind` 사이의 매핑을 어떤 계층이 책임질지
- generic intrinsic의 type parameter owner를 translator 내부에서 어떻게 구성할지
- 장기적으로 runtime library call로 lowering되는 intrinsic과 pure QIR intrinsic을 같은 declaration model 아래 둘지

Action Items
- [ ] translator 내부 intrinsic callable declaration 초안 작성
- [ ] `QAbi::GetFuncInfo` 입력이 요구하는 최소 인터페이스(return type, this kind, param list, param kind)를 정리
- [ ] `GetListIterator<T>`와 `Add_String_StringInRef_StringInRef`를 새 모델로 표현하는 예시 작성
- [ ] `QInst_IntrinsicKind`를 signature metadata와 분리한 registry/table 구조 초안 작성
