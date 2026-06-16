# QIR Call ABI and Evaluator Snapshot

Updated: 2026-04-26
Status: current draft

Summary
- QIR call lowering은 logical slot model을 유지하되, physical register/stack 선택은 QEvaluation/LLVM lowering에 맡긴다.
- Direct return은 callee slot으로 표현하지 않고 hidden return destination으로 처리한다.
- Indirect return은 current preferred shape로 hidden first slot을 사용할 수 있다.
- QEvaluator는 backend 성능/메모리 동작이 아니라 observable event trace를 보존하는 reference executor다.

Layering
- MIR:
  - ABI-neutral language semantics를 표현한다.
  - BC/NBC, read/create/materialize, object lifetime 의미를 명시한다.
- QIR:
  - logical slot과 call/return/passing mode를 표현한다.
  - target-specific physical register/stack layout을 고정하지 않는다.
- QEvaluation:
  - QIR 의미를 일반적으로 실행한다.
  - slot storage와 frame metadata를 직접 관리해도 된다.
- LLVM/backend:
  - register return, sret hidden parameter, alloca, stack argument, target calling convention을 물리화한다.

Slot Model
- QIR slot은 함수 본문이 참조하는 logical storage/value carrier다.
- slot index는 의미를 암묵적으로 encode하지 않는다.
- slot에는 role metadata를 둔다.

Recommended slot roles:
- `HiddenReturnDestPtr`
- `This`
- `Parameter`
- `Local`
- `Temp`

Direct Return
- Direct return result는 callee slot에 두지 않는다.
- callee frame이 정리되면 callee slot storage가 사라지기 때문이다.
- Direct return은 hidden frame metadata `returnDest`에 쓴다.
- QEvaluation에서는 `returnDest`가 caller destination slot storage를 직접 가리킬 수 있다.
- LLVM lowering에서는 target ABI에 따라 register return으로 물리화할 수 있다.

Indirect Return
- Indirect return은 caller-provided return storage를 사용한다.
- current preferred shape:
  - hidden direct `returnDest`는 사용하지 않는다.
  - slot 0 role = `HiddenReturnDestPtr`
  - slot 0은 return object가 아니라 return object storage를 가리키는 pointer value를 담는다.
- Direct/void return 함수에는 `HiddenReturnDestPtr` slot이 없다.
- Indirect return object의 destructor와 storage reclaim은 caller가 담당한다.

This and Parameters
- `this`는 특수 위치가 아니라 일반 slot으로 유지한다.
- parameter도 일반 slot으로 유지한다.
- Indirect return 함수에서는 `HiddenReturnDestPtr` 뒤에 `this`, explicit arguments, locals, temps가 이어진다.
- Direct/void return 함수에서는 `this` 또는 첫 parameter/local이 첫 slot이 될 수 있다.
- 의미 판정은 slot index가 아니라 slot role metadata로 한다.

Parameter Passing Modes
- `Direct`
  - callee는 독립된 by-value parameter slot을 본다.
  - QEvaluation은 callee-owned storage를 만들고 값을 copy할 수 있다.
  - LLVM lowering은 register/SSA로 전달하고, address가 필요할 때만 materialize할 수 있다.
- `Indirect`
  - callee slot에는 caller-provided storage/object를 가리키는 pointer value가 있다.
  - NBC by-value parameter는 construct/destroy owner 규약을 따른다.
- `Ref`
  - callee slot에는 caller object를 alias하는 pointer/reference value가 있다.
  - callee mutation은 caller object에 관찰된다.

Call Arguments
- `QArg_CallArg_AddrOfSlot(s)`는 caller frame의 slot `s` storage address를 값으로 전달한다.
- 이것은 callee slot storage가 caller slot을 alias한다는 뜻이 아니다.
- formal parameter passing mode와 callee slot role이 call argument 해석을 결정한다.

Examples:
- Direct formal + slot argument:
  - callee-owned storage에 value copy
- Indirect/Ref formal + addr-of-slot argument:
  - callee slot에 pointer value 저장
- `HiddenReturnDestPtr` slot + addr-of-slot argument:
  - callee slot에 return destination pointer 저장
- Nested indirect return:
  - current function의 `HiddenReturnDestPtr` slot이 담은 pointer value를 nested call의 return destination으로 넘길 수 있다.

QEvaluator Behavior
- QEvaluator는 semantic reference executor다.
- physical call frame, stack/register shape, BC copy count를 backend와 일치시킬 필요는 없다.
- 반드시 보존해야 하는 것:
  - by-value parameter 독립성
  - ref/alias parameter aliasing
  - Direct/Indirect return destination 의미
  - NBC construct/copy/move/assign/destroy 순서
  - external side effect, mutable state mutation, trap/diagnostic/throw 순서
- Direct parameter copy가 QEvaluation에서 발생하더라도, QIR에 physical copy requirement로 노출하지 않는다.

Current Open Points
- Indirect return을 hidden first slot으로 확정할지, Direct return처럼 hidden `returnDest`로 통일할지 최종 결정이 필요하다.
- `QSlotRole`을 QIR public model로 둘지 translator-private metadata로 둘지 결정이 필요하다.
- 현재 `QReturnPassingMode_Indirect { size_t index }` 모델을 role 기반으로 바꿀지 검토해야 한다.
- NBC indirect parameter destructor owner는 callee-destroy 초안과 caller-destroy 대안을 계속 비교해야 한다.
- Direct parameter의 address가 필요할 때 LLVM lowering에서 materialize하는 규칙을 정리해야 한다.
