# 회의 / 실험 노트

Date: 2026-04-30
Title: QIR slot 표현 유지와 managed_ptr annotation 방향

Summary
- QIR slot의 기본 표현 체계는 크게 바꾸지 않고 유지한다.
- indirect parameter passing 때문에 callee slot이 object 자체가 아니라 object를 가리키는 pointer value를 담는 경우가 생긴다.
- 이 차이를 새로운 slot kind 체계로 크게 확장하기보다, 우선 `managed_ptr` annotation으로 정리한다.
- slot 출력은 representation type을 드러내되, ownership/cleanup 정보는 `managed_ptr` flag로 표시하는 쪽을 선호한다.

Context
- 현재 ABI에서는 `string` 같은 NBC by-value parameter를 indirect passing으로 넘길 수 있다.
- 이 경우 caller는 storage alloc/init을 하고, callee는 destructor를 호출하며, caller는 storage reclaim을 담당할 수 있다.
- 따라서 callee의 parameter slot은 surface 타입이 `string`처럼 보여도, 실제로는 `string` object를 직접 담지 않고 `string*` pointer value를 담을 수 있다.
- 이 차이를 무시하면:
  - argument로 다시 넘길 때 `AddrOfSlot`과 `Slot` 중 어느 것을 써야 하는지 헷갈린다.
  - cleanup에서 object destructor를 slot address에 할지, slot value가 가리키는 object에 할지 헷갈린다.

Decision
## 1) slot 표현 타입 체계는 당장 뒤집지 않는다
- `QSlotInfo`를 direct/indirect slot enum으로 크게 재구성하는 작업은 지금 단계에서 보류한다.
- 우선 기존 slot 모델을 유지한 채 필요한 정보만 annotation으로 추가한다.

## 2) pointer representation이 필요한 slot에는 `managed_ptr` flag를 둔다
- slot이 semantic object를 직접 담는 대신, 그 object를 가리키는 pointer value를 담고 있고,
- 그 object cleanup 책임이 현재 함수 쪽에 있으면 `managed_ptr` annotation을 단다.
- 예:
  - `// slot s1: string`
  - `// slot s2: string*, managed_ptr`

## 3) 출력에는 representation type을 드러낸다
- slot 주석/디버그 출력에서는 semantic 설명만 숨기지 말고 실제 representation을 보이도록 한다.
- 즉 indirect managed parameter slot은 `string`이 아니라 `string*`로 보이게 하고, `managed_ptr` flag를 함께 표시한다.
- 이 방식은:
  - 왜 어떤 곳은 `[s]`를 쓰고
  - 어떤 곳은 `s`를 그대로 넘기는지
  를 QIR 텍스트만 보고도 이해하기 쉽게 만든다.

## 4) cleanup 규칙은 `managed_ptr` 기준으로 단순화한다
- 일반 object slot:
  - destructor가 object address를 받으면 `AddrOfSlot` 형태를 사용한다.
- `managed_ptr` slot:
  - slot value 자체가 object address이므로, 그 value를 destructor operand로 사용한다.
- 즉 "managed_ptr가 있으면 그 slot은 destructor 대상"이라는 규칙을 둘 수 있다.

## 5) BC/NBC와 direct/indirect representation은 분리된 축으로 유지한다
- indirect representation은 NBC 전용으로 제한하지 않는다.
- BC large value도 ABI에 따라 indirect parameter passing / indirect return을 사용할 수 있다.
- 따라서:
  - BC/NBC는 value semantics 축
  - direct/indirect-like representation은 slot representation 축
  으로 분리해서 본다.

Related observations
## 6) `QLocResult_Ptr` / `QReadResult_Ptr`는 raw pointer가 아니라 "pointer value가 들어있는 slot"을 가리킨다
- `QLocResult_Ptr { slotIndex }`의 `slotIndex`는 메모리 주소 자체가 아니라 slot index다.
- 즉 이것은 "ptr 타입의 값이 들어있는 slot"을 가리킨다.
- 이 의미는 위 `string*, managed_ptr` 같은 slot annotation과 잘 맞는다.

## 7) `MRead_Exp` / `MExp_Load` 논의와의 연결
- 과거에는 `MRead_Exp`가 value만 내보내고 `Ptr`을 내보내지 않도록 일부러 정규화하는 설계 흔적이 있었다.
- 현재 방향에서는 pointer-backed representation을 더 오래 보존하는 쪽으로 기울고 있으므로,
  `MExp_Load`가 불필요한 load/assign을 강제하는지 재검토 여지가 있다.
- 다만 이 메모의 직접 결정은 slot 표현을 크게 바꾸지 않고 `managed_ptr` annotation으로 우선 정리하는 것이다.

Recommended current shape
- slot info / printer:
  - direct object slot: `T`
  - managed pointer slot: `T*`, `managed_ptr`
- argument passing helper:
  - direct object slot을 object address로 넘길 때는 `AddrOfSlot`
  - managed pointer slot을 object address로 넘길 때는 `Slot`
- cleanup helper:
  - direct object slot이면 object address를 계산해 destructor 호출
  - managed pointer slot이면 slot value를 그대로 destructor operand로 사용

Open points
- `managed_ptr`가 ownership/cleanup만 뜻하는지, indirect representation까지 항상 포함하는지 명확히 할 필요가 있다.
- ref parameter처럼 pointer value는 담지만 managed가 아닌 slot과의 구분을 출력에서 어떻게 보여줄지 정리 필요.
- 나중에 ABI가 늘어날 때 `managed_ptr`만으로 충분한지, 혹은 parameter contract metadata를 추가해야 하는지 재검토가 필요하다.
