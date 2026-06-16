# Current Decisions

Status: current snapshot

이 문서는 agent가 현재 큰 결정을 빠르게 파악하기 위한 요약이다. 자세한 내용은 각 topic page를 본다.

## Documentation
- 현재 기준 지식은 `ai/wiki/`에 둔다.
- `ai/notes/`는 history로 유지한다.
- 기존 specs/implementations 내용은 wiki로 옮긴 뒤 active tree에는 유지하지 않는다. 필요하면 `git history: ai/specs/`와 `git history: ai/implementations/`를 확인한다.

## Language
- `some Trait`는 함수 return position 전용 opaque result marker다. 일반 type expression이 아니며 변수/인자/generic argument 위치에는 쓰지 않는다.
- `some Trait` 결과는 `var`로만 받으며, source-level에서는 declared trait surface만 사용할 수 있다.
- `trait`는 static contract다. `interface`는 dynamic/runtime dispatch contract로 따로 둔다.
- `func<R, Params...>`는 callable static contract type expression이다. `lambda<>` type expression은 두지 않는다.
- 일반 `dyn trait`는 도입하지 않는다. dynamic dispatch가 필요하면 명시적 `interface`를 설계한다.
- associated type inference는 v1에서 제외한다. trait 구현체는 `using` 또는 nested type으로 associated type requirement를 명시적으로 충족한다.
- `concept`는 초기에는 반복되는 `where` constraint 묶음으로 본다.
- `T&`는 일반 first-class type constructor가 아니라 parameter/return/local alias/implicit this 같은 제한된 surface slot의 reference 표기다.
- Nested nullable은 flatten하지 않는다. `C?`는 compressed nullable representation, 일반 `T?`는 tagged nullable representation으로 본다.
- 초기 `foreach`는 `RefEnumerable` / `RefEnumerator` 위의 `foreach(var& x in e)`를 우선한다. `foreach(var x in e)`의 `RefEnumerable` fallback은 두지 않는다.
- `void`는 `tuple<>`와 별도 타입이다. `T = void` generic argument는 허용하되 내부에는 `__VoidSubst`를 사용할 수 있다.
- BC/NBC value semantics는 observable behavior 기준으로 구분한다. NBC lifetime operation은 보존해야 한다.
- Return은 기본적으로 RVO path를 요구하고, NBC return call은 dest-passing / caller-provided storage를 기본 lowering으로 본다.
- Binding을 만드는 `is`는 일반 expression context에서 금지하고, `if` condition의 top-level에서만 허용한다.
- `visibility`는 source name lookup rule이고, `reachability`는 compiler가 semantic information을 알 수 있는지의 문제로 분리한다.
- Nested declaration은 논리적으로 허용 가능하지만, v1 허용 범위는 implementation scope와 design stability에 따라 결정한다.

## Modules And CTI
- `cti`는 declaration/import boundary다.
- Swift식 opaque result metadata 모델을 택하면 consumer-facing `rcti`는 없어질 수 있다.
- `cti`는 `some` opaque result identity, metadata accessor symbol, opaque sret ABI contract를 담을 수 있어야 한다.
- 같은 module 안 unit 상호참조와 `using unit` 부활 여부는 아직 재검토 중이다.

## Compiler
- `some` opaque result call은 metadata accessor, value witness, trait witness, opaque sret로 낮춘다.
- value witness는 size/align/copy/move/destroy 같은 값 기본 연산 테이블이다.
- trait witness는 trait requirement를 backing type 구현으로 연결하는 테이블이다.
- MIR 값 모델은 BC/NBC, read/create/init destination을 분리하는 방향을 유지한다.
- `MCreate`는 MIR surface에 유지하되, lowering 중심 primitive는 `TranslateMExp`, `TranslateMLoc`, `TranslateMInitExp`로 분리한다.
- QIR call lowering은 logical slot model과 passing mode를 사용하고, physical ABI 선택은 후속 lowering에 맡긴다.
- QIR slot index만으로 meaning을 추론하지 않고 role metadata를 둔다. Indirect return은 hidden first slot `HiddenReturnDestPtr` shape를 current preferred로 본다.
- QEvaluator는 backend physical behavior가 아니라 observable event trace를 보존하는 semantic reference executor다.

## Process
- 기능/버그 수정에는 관련 테스트 추가/갱신을 동반한다.
- Windows 빌드/테스트는 `ai/process/build-test-and-generation-windows.md`를 따른다.
- `.g.cpp`, `.g.h`는 직접 수정하지 않고 generator를 통해 갱신한다.
