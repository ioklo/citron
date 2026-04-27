# 회의 / 실험 노트

Date: 2026-04-26
Title: QIR function call slot/return behavior and observable behavior boundary

Summary
- QIR 함수 호출 규약에서 slot, return destination, parameter passing mode의 역할을 다시 정리했다.
- Direct return은 callee slot에 두면 callee frame 정리 후 값 위치가 사라지므로 hidden return destination으로 다루는 방향을 선호한다.
- Indirect return은 hidden first slot으로 둘 수 있다. 이 slot은 return object 자체가 아니라 caller-provided return storage를 가리키는 pointer value다.
- QEvaluator는 실제 backend의 register/stack 동작을 재현하기보다, QIR 의미의 reference executor로 observable event trace를 보존하는 역할을 한다.

Context
- 기존 ABI v0 초안은 `Direct`/`Indirect` passing mode를 call boundary contract로 정의하고, `Indirect`는 caller-provided storage를 통해 전달한다고 정리했다.
- NBC 반환 call은 sret(dest-passing) 규약으로 lowering한다는 기존 결정이 있다.
- 다만 "sret를 QIR에서 반드시 첫 번째 일반 인자로 표현해야 하는가"와 "return destination을 QEvaluation frame metadata로 둘 수 있는가"는 별도 문제다.
- QIR 명령어는 기본적으로 slot operand를 사용하므로, 특수 위치를 늘리면 정보는 명확해지지만 명령어 모델이 복잡해질 수 있다.

Decisions
## 1) Slot은 logical storage/value carrier이고, physical register/stack choice는 backend가 결정한다
- QIR slot은 함수 본문이 참조하는 logical slot이다.
- QEvaluation에서는 slot을 실제 stack 위 storage나 pointer-sized storage로 구현할 수 있다.
- LLVM lowering에서는 slot을 register/SSA value, alloca, stack argument, pointer argument 등으로 물리화할 수 있다.
- 따라서 QIR에 "모든 인자를 sequential stack area에 복사한다"는 물리 규칙을 박지 않는다.

## 2) Direct return은 callee slot으로 두지 않는다
- Direct return 값을 callee slot 0에 두면, callee frame을 pop한 뒤 caller가 읽을 안정적인 위치가 없다.
- Direct return은 callee frame의 hidden return destination metadata를 통해 caller-provided destination에 쓴다.
- QEvaluation에서는 `returnDest`가 caller slot storage를 직접 가리키게 할 수 있다.
- LLVM lowering에서는 target ABI에 따라 register return으로 물리화하고, caller가 필요하면 결과를 slot에 store한다.

## 3) Indirect return은 hidden first slot으로 둘 수 있다
- Indirect return에서 slot 0을 사용하는 안은 허용 가능하다.
- 이때 slot 0은 return object가 아니라 return object storage를 가리키는 pointer value다.
- slot 0에는 `HiddenReturnDestPtr` 같은 role metadata를 둔다.
- Indirect return 함수의 slot layout 예:
  - slot 0: `HiddenReturnDestPtr`
  - 이후: `this`, explicit arguments, locals, temps
- Direct/void return 함수에는 `HiddenReturnDestPtr` slot이 없다.

## 4) this와 parameter는 일반 slot으로 유지한다
- `this`는 함수 본문에서 일반 값처럼 읽히고, field access base나 다른 call argument로 자주 쓰인다.
- 따라서 `this`까지 특수 위치로 빼기보다 slot으로 두는 편이 명령어 모델을 단순하게 유지한다.
- 다만 slot index만으로 의미를 추론하지 않고, `QSlotInfo`에 role metadata를 둔다.
- 권장 role 예:
  - `HiddenReturnDestPtr`
  - `This`
  - `Parameter`
  - `Local`
  - `Temp`

## 5) Parameter passing mode의 의미
- `Direct`
  - callee는 독립된 by-value parameter slot을 본다.
  - QEvaluation에서는 callee-owned storage에 값을 copy할 수 있다.
  - LLVM lowering에서는 register/SSA로 전달하고, 주소가 필요할 때만 materialize할 수 있다.
- `Indirect`
  - callee slot에는 caller-provided storage/object를 가리키는 pointer value가 있다.
  - NBC by-value parameter처럼 object lifetime semantics가 필요한 경우 construct/destroy owner 규약을 따른다.
- `Ref`
  - callee slot에는 caller object를 alias하는 pointer/reference value가 있다.
  - callee의 mutation은 caller 쪽 object에 관찰된다.

## 6) QArg_CallArg_AddrOfSlot은 call setup용 address value다
- `QArg_CallArg_AddrOfSlot(s)`는 caller frame의 slot `s` storage address를 값으로 전달한다.
- 이것은 "callee slot storage가 caller slot을 alias한다"는 뜻이 아니다.
- formal parameter passing mode와 slot role이 call argument를 어떻게 해석할지 결정한다.
- 예:
  - Direct formal + slot arg: value copy
  - Indirect/Ref formal + addr-of-slot arg: pointer value 저장
  - HiddenReturnDestPtr slot + addr-of-slot arg: return destination pointer 저장

## 7) BC와 NBC의 observation 차이
- BC 값은 여러 번 copy되어도 언어 의미가 보존된다.
- BC copy 횟수, register/stack 선택, 불필요한 memcpy 유무는 observable behavior가 아니다.
- NBC 값은 constructor/destructor/copy/move/assign 같은 lifetime operation이 의미 이벤트다.
- 따라서 NBC는 값 자체가 자유롭게 copy되어 돌아다니기보다 address/place 중심으로 전달되는 모델이 자연스럽다.
- NBC call/return/parameter lowering은 object lifetime event와 ownership 규약을 정확히 보존해야 한다.

## 8) QEvaluator의 역할은 observable event trace 보존이다
- QEvaluator는 실제 backend의 성능/메모리 특성을 재현하는 실행기가 아니다.
- QEvaluator는 QIR semantic reference executor로서 observable event trace를 보존한다.
- 함수 call frame 생성, register/stack 사용, physical copy 횟수는 backend와 달라도 된다.
- 다만 by-value/ref/indirect 의미와 NBC lifetime event는 정확히 유지해야 한다.

Observable Behavior Boundary
- Observable behavior는 "system call 결과"만으로 좁히지 않는다.
- QIR/QEvaluator 기준 observable events:
  - external side effect의 순서와 내용
  - mutable/alias 가능한 program state mutation의 순서와 결과
  - NBC lifetime operation의 순서
    - construct
    - copy-construct
    - move-construct
    - assign
    - destroy
  - trap/panic/diagnostic/throw 같은 비정상 제어 결과와 그 전까지의 event 순서
  - 최종 반환값 또는 최종 externally visible state
- Evaluation order 자체는 독립적인 observation 대상이 아니다.
- Evaluation order는 위 observable events의 순서를 결정하는 language rule로 본다.
- 순수 BC 계산처럼 observable event를 만들지 않는 평가 순서는 backend optimization이 바꿔도 관찰되지 않을 수 있다.

Recommended Current Shape
- Direct return:
  - hidden frame metadata `returnDest`
  - no return slot
- Indirect return:
  - no hidden direct return destination
  - slot 0 role = `HiddenReturnDestPtr`
  - slot 0 stores pointer value to caller-provided return storage
- This/parameters:
  - slots after hidden indirect return slot, if any
  - role metadata로 식별
- QEvaluation:
  - Direct parameter는 필요하면 callee-owned storage로 copy
  - Indirect/Ref parameter는 pointer value를 slot에 저장
  - Direct return은 hidden `returnDest`에 write
  - Indirect return은 `HiddenReturnDestPtr` slot의 pointer가 가리키는 storage에 construct/write
- LLVM lowering:
  - Direct return/param은 register/SSA로 최적화 가능
  - Indirect return/param은 target ABI에 따라 hidden sret parameter, pointer argument 등으로 물리화

Open Points
- Direct return과 Indirect return을 모두 hidden `returnDest`로 통일할지, Indirect return만 hidden first slot으로 둘지 최종 확정 필요.
- `QSlotRole`을 QIR public model에 추가할지, translator-private metadata로 둘지 결정 필요.
- `QReturnPassingMode_Indirect { size_t index }`를 유지할지, `HiddenReturnDestPtr` role 기반으로 바꿀지 검토 필요.
- NBC indirect parameter의 destructor owner는 ABI v0 초안에서 callee로 되어 있으나, caller-destroy 안과 계속 비교가 필요하다.
- Direct parameter의 `AddrOf(slot)` 발생 시 LLVM lowering에서 materialize하는 규칙을 정리해야 한다.

Action Items
- [ ] QIR slot role metadata 초안을 코드 구조에 맞춰 설계한다.
- [ ] `QFuncInfo`의 return passing representation이 현재 결정과 맞는지 재검토한다.
- [ ] QEvaluator call setup을 passing mode와 slot role 기준으로 정리한다.
- [ ] Observable event trace 관점에서 QEvaluator 테스트가 무엇을 검증해야 하는지 목록화한다.
