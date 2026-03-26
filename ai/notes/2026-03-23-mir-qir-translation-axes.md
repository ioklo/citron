# 회의 / 설계 노트

Date: 2026-03-23
Title: MIR -> QIR translator 축을 `MRead` / `MCreate` / `MLoc` 기준으로 재정리

Status
- current

Summary
- MIR -> QIR translation의 상위 엔트리를 `MExp` / `MInitExp` / `MLoc` 기준으로 두기보다, 문맥 의미인 `MRead` / `MCreate` / `MLoc` 기준으로 두는 쪽이 더 자연스럽다는 결론에 도달했다.
- `MLoc`은 다른 둘과 같은 레벨의 대안이 아니라, `MRead_Loc`, `MCreate_NBC` 등이 공통으로 참조하는 place 표현이다.
- 따라서 QIR translator의 상위 축은 다음 세 가지로 본다.
  - `TranslateMReadToQInsts`
  - `TranslateMCreateToQInsts`
  - `TranslateMLocToQInsts`
- 내부 구현에서는 여전히 `MExp`, `MInitExp` translator/helper가 필요할 수 있으나, 그것들은 하위 구현 세부사항으로 본다.

Context
- 기존 구현은 대체로 다음 질문을 중심으로 작성되어 있었다.
  - `MExp`를 QIR로 어떻게 바꾸는가
  - `MLoc`을 QIR로 어떻게 바꾸는가
  - `MInitExp`를 QIR로 어떻게 바꾸는가
- 그러나 최근 `MRead = Exp | Loc`, `MCreate = BC | NBC`, `MLoc` 분리 논의 이후에는 호출자 입장에서 더 중요한 정보가 "이 자리가 read인가, create인가, loc인가"라는 점이 분명해졌다.
- 특히 `MExp_Store`, `MStmt_Exp`, assign/store source, 조건식 같은 곳에서 `MExp` / `MInitExp` / `MLoc` 자체보다 문맥 의미가 먼저 드러나는 편이 번역 책임을 명확히 한다.

Decision
## 1) QIR translation의 상위 엔트리는 `MRead` / `MCreate` / `MLoc`
권장 상위 함수:
- `TranslateMLocToQInsts(...)`
- `TranslateMReadToQInsts(...)`
- `TranslateMCreateToQInsts(...)`

의도:
- `TranslateMLocToQInsts`
  - place 자체를 QIR에서 어떻게 접근할지 계산한다.
- `TranslateMReadToQInsts`
  - read 입력을 QIR에서 소비 가능한 결과로 정규화한다.
- `TranslateMCreateToQInsts`
  - create 입력을 특정 destination place에 초기화한다.

## 2) `MLoc`은 별도 기반 축으로 유지
- `MLoc`은 `MRead` / `MCreate`와 같은 층의 경쟁 분류가 아니다.
- `MRead_Loc`도 내부적으로는 `MLoc`을 감싸고 있고, `MCreate_NBC`도 결국 어떤 `MLoc` 또는 그에 대응하는 destination address를 필요로 한다.
- 따라서 `MLoc`은 기반 place 표현으로 유지한다.

## 3) `MRead_Loc`와 `MLoc` translator는 겉으로는 두 벌이 되지만, primitive는 `MLoc`
- `TranslateMLocToQInsts(loc)`는 place를 번역하는 primitive다.
- `TranslateMReadToQInsts(MRead_Loc{loc})`는 내부에서 `TranslateMLocToQInsts(loc)`를 호출해 그 결과를 read 문맥으로 재해석하는 adapter 성격으로 본다.
- 즉 둘은 완전히 독립적인 구현이라기보다, `MRead_Loc`가 `MLoc` translation을 재사용하는 관계를 기본으로 한다.

## 4) `MRead_Exp`와 `MCreate_BC`는 겉으로 비슷해도 인터페이스 책임이 다르다
- `MRead_Exp`
  - 본질: 읽기 결과를 하나 얻는다.
  - 자연스러운 lowering: 필요하면 translator 내부에서 slot을 하나 할당하고, `MExp`를 그 slot에 계산한 뒤 반환한다.
  - 외부에서 destination slot을 꼭 넘길 필요는 없다.
- `MCreate_BC`
  - 본질: 어떤 destination을 채우는 BC create다.
  - 일반적으로 destination-aware 하다.
  - 다만 `MStmt_Exp`처럼 결과를 버리는 문맥이 있으므로, API 수준에서 destination을 optional로 둘지 여부는 별도 판단이 필요하다.

## 5) `MRead_Exp`와 `MCreate_BC`는 초기에 두 벌로 두는 것을 허용
- 둘 다 내부적으로 `MExp`를 내리므로 구현이 중복될 수 있다.
- 그러나 현재 단계에서는 문맥 책임이 다르므로 성급한 공통화보다, 일단 두 벌로 구현하고 이후 크게 다르지 않을 때 helper로 합치는 쪽을 선호한다.
- 같은 이유로 `MRead_Loc`와 `MLoc` 처리도 겉으로는 두 벌처럼 보일 수 있다.

QLocResult discussion
## 6) `TranslateMReadToQInsts`의 반환 타입은 당분간 `QLocResult` 재사용 가능
- 현재 `QLocResult`는 다음 두 경우를 표현한다.
  - `slot`
  - `ptr이 들어있는 slot`
- `MRead_Exp`는 location이 없으므로 translator 내부에서 slot을 하나 할당해 값을 넣고 `slot` 결과로 반환하는 것이 자연스럽다.
- `MRead_Loc`는 `MLoc` translation 결과를 재사용할 수 있으므로, 결과 shape는 결국 `slot` 또는 `ptr-slot`으로 수렴한다.
- 따라서 개념상 이름은 다소 넓어졌지만, 당분간은 `TranslateMLocToQInsts`와 `TranslateMReadToQInsts`가 둘 다 `QLocResult`를 리턴해도 실용상 무리가 없다고 본다.
- 추후 이름이 계속 거슬리면 `QReadResult` 등으로 분리/rename할 수 있다.

Store lowering discussion
## 7) `MExp_Store(dest, MRead src)`의 기본 lowering은 `src -> value slot -> dest`
단순화해서 location이 slot 기반이라고 보면 기본 패턴은 다음과 같다.
1. `dest`를 번역해서 destination location/slot을 얻는다.
2. `src`를 읽어서 source value slot을 확보한다.
   - `MRead_Exp`면 새 slot을 할당하고 그 안에 계산
   - `MRead_Loc`면 loc translation 결과를 읽기 문맥에 맞게 사용
3. destination에 source 값을 쓴다.
4. expression result가 필요하면 source value를 재사용해 결과를 채운다.

논의 결과:
- `MRead_Exp`를 destination slot에 직접 계산시키는 fast path는 일부 경우 가능할 수 있으나, 기본 lowering 규칙은 별도 source value slot을 두는 쪽이 더 안전하고 단순하다.
- 단순 local var assign은 별도 명령어/경로에서 직접 처리할 수 있으므로, `MExp_Store`는 일반 loc 기반 store에 집중시키는 것이 자연스럽다.

Create destination discussion
## 8) `MCreate_NBC`는 value slot보다 destination address를 직접 받는 쪽이 맞다
- `MCreate_NBC`는 본질적으로 "어느 place를 초기화하는가"가 의미의 중심이다.
- 따라서 NBC create translator는 넓은 `QLocResult`보다, destination 주소가 들어있는 ptr-slot을 직접 받는 쪽을 선호한다.

권장 형태:
- `TranslateMCreateNBCToQInsts(create, size_t destPtrSlot)`

이유:
- NBC create는 ctor/copy/move/field-wise init 등 대부분의 경로가 결국 destination address 기반이다.
- `slot 또는 ptr-slot` 모두를 받게 하면 오히려 책임이 흐려진다.

## 9) `MStmt_Exp`에서 NBC create는 dummy storage가 필요
- BC는 값 결과를 버릴 수 있는 경우가 있다.
- NBC는 값을 버리더라도 실제 초기화가 일어날 destination place가 필요하다.
- 따라서 `MStmt_Exp`에서 `MCreate_NBC`를 처리할 때는:
  1. dummy object storage를 하나 마련
  2. 그 주소를 담은 ptr-slot을 마련
  3. `TranslateMCreateNBCToQInsts(create, dummyPtrSlot)` 호출
  4. full-expression 종료 시 dummy를 정리
  5. 결과는 버림
- 이는 NBC 값이 place 없이 존재하지 않는다는 현재 모델과 맞다.

BC create destination discussion
## 10) `MCreate_BC`의 destination optional 여부는 source kind에 따라 체감이 다르다
문제 사례:
- `F();` where `int F() { return 3; }`
- `a = 3;`

관찰:
- `F();`는 결과를 버리더라도 call lowering 관점에서는 concrete result slot이 하나 필요하다고 보는 편이 자연스럽다.
- 반면 `a = 3;`은 `(a = 3)` 자체를 위한 별도 result place가 없어도, source를 읽어서 destination에 쓰는 것으로 충분할 수 있다.

잠정 결론:
- `MCreate_BC`의 API를 `optional dest`로 둘 수는 있다.
- 그러나 `dest == nullopt`라고 해서 항상 concrete result storage가 불필요한 것은 아니다.
- 특히 BC 반환 call은 `nullopt` 상황에서도 내부적으로 result slot을 반드시 확보해야 할 수 있다.
- 따라서 `MCreate_BC`의 lowering은 destination optional 여부뿐 아니라 내부 `MExp`의 kind까지 보고 판단해야 한다.

Rationale
- `MRead` / `MCreate` / `MLoc` 축은 호출자의 문맥 의미를 직접 반영한다.
- `MExp` / `MInitExp` / `MLoc`만으로 public translation surface를 잡으면, 호출자 쪽에서 다시 read/create/loc 의미를 복원해야 한다.
- 반대로 문맥 기반 translator를 두면, 하위 `MExp` / `MInitExp` 처리는 helper로 내려 보내면서 상위 책임을 더 명확히 유지할 수 있다.

Open points
- `TranslateMCreateBCToQInsts`의 최종 시그니처를 `optional<size_t>`로 둘지, 더 강한 destination 정책을 둘지
- `MRead_Loc`가 `QLocResult_PtrSlot`을 돌렸을 때의 공통 read-to-value helper를 둘지
- `MExp_Store` 결과값을 `src` value slot에서 복사할지, `dest`를 다시 읽어올지에 대한 기본 정책 정리
- `QLocResult` 명칭을 계속 재사용할지, 추후 `QReadResult` 등으로 분리할지

Action Items
- [ ] `TranslateMReadToQInsts` 초안 작성
- [ ] `TranslateMCreateBCToQInsts` / `TranslateMCreateNBCToQInsts` 시그니처 초안 확정
- [ ] `MStmt_Exp`의 BC/NBC 경로를 분리한 lowering 규칙 문서화
- [ ] `MExp_Store(dest, MRead src)`의 기본 lowering 패턴 구현 후 재검토

Update: 2026-03-23 (follow-up)

Follow-up discussion
## 11) `TRead_Exp`와 `TRead_Loc`의 반환 형태를 분리한다
기존에는 `TRead(exp)` 계열이 `slot | ptr`를 모두 돌려줄 수 있다는 가정 아래 생각을 진행했으나,
후속 논의에서 `Exp`와 `Loc`의 성격을 더 강하게 나누는 편이 낫다는 결론에 도달했다.

결론:
- `TRead_Exp(exp)`는 **항상 value가 들어있는 slot**을 리턴한다.
- `TRead_Loc(loc)`는 **`slot | ptr`** 를 리턴한다.

이유:
- `Exp`는 BC value 표현이므로 결과를 value slot으로 정규화해도 의미 손실이 없다.
- 반대로 `Loc`는 아직 "읽을 수 있는 source/place" 단계이므로,
  local var처럼 이미 slot-backed일 수도 있고, field/deref처럼 ptr-backed일 수도 있다.
- `TRead_Exp`를 항상 slot으로 고정하면 chain assign과 후속 BC 연산이 전부 slot 기반으로 이어질 수 있어 lowering이 단순해진다.

## 12) `MExp_Assign(dest, src)`의 `TRead_Exp` 결과는 가능하면 slot을 재사용한다
`MExp_Assign(MLoc* dest, MRead src)`에 대해 `TRead_Exp`를 정의할 때,
결과를 항상 destination location으로 돌려줄지, 아니면 재사용에 유리한 slot으로 돌려줄지를 검토했다.

잠정 결론:
- `TRead_Exp(MExp_Assign(...))`의 결과는 **항상 slot**이 되도록 정규화하는 쪽을 선호한다.
- 구체적으로:
  - `dest=slot, src=slot` -> `Assign(dest, src); return dest`
  - `dest=slot, src=ptr`  -> `Load(dest, src); return dest`
  - `dest=ptr,  src=slot` -> `Store(dest, src); return src`
  - `dest=ptr,  src=ptr`  -> `Load(temp, src); Store(dest, temp); return temp`
- 특히 마지막 경우(`ptr <- ptr`)는 `Memcpy(dest, src, size); return dest`보다,
  `temp`를 만들고 그 `temp`를 반환하는 쪽이 chain assign에서 후속 메모리 연산을 줄일 수 있다.

예:
- `a = b = c` 형태에서 중간 결과가 `temp slot`이면, 바깥 assign이 그 slot을 바로 source로 재사용할 수 있다.

## 13) `TCreate_BC`와 `TRead_Exp`는 계속 별도 성격으로 둔다
정의:
- `TCreate_BC(exp, slot)` : 주어진 destination slot에 `exp`의 결과물을 채우는 행동을 만든다.
- `TRead_Exp(exp)` : `exp`가 나타내는 BC value가 들어있는 slot을 리턴한다.

논의 결과:
- 둘 다 내부적으로 `MExp`를 다루지만, 반환/목적이 다르므로 개념적으로는 계속 분리한다.
- `TCreate_BC`는 destination-aware translation이고,
- `TRead_Exp`는 self-contained한 BC value slot producer다.

## 14) `TRead_Exp`의 C++ 반환 타입은 `size_t`보다 wrapper를 선호
구현 이슈로 `TRead_Exp`의 반환 타입을 `size_t`로 둘지, `QLocResult_Slot` wrapper로 둘지 검토했다.

잠정 결론:
- 외부에 드러나는 translator/helper 시그니처라면 `size_t`보다 `QLocResult_Slot`을 선호한다.
- 이유:
  - `size_t`는 단순 숫자여서 slot index라는 의미가 약하다.
  - `QLocResult_Slot`은 타입 수준에서 "slot 기반 결과"라는 뜻을 남긴다.
- 다만 아주 로컬한 내부 helper에서만 쓴다면 `size_t`도 실용적으로 가능하다.
- 현재 방향에서는 `TRead_Exp`가 `QLocResult_Slot`을 리턴하고, 필요할 때 `.slotIndex`를 꺼내 쓰는 쪽을 권장한다.
