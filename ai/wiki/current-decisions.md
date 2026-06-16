# Current Decisions

Status: current snapshot

이 문서는 agent가 현재 큰 결정을 빠르게 파악하기 위한 요약이다. 자세한 내용은 각 topic page를 본다.

## Documentation
- 현재 기준 지식은 `ai/wiki/`에 둔다.
- `ai/notes/`는 history로 유지한다.
- `ai/specs/`와 `ai/implementations/`는 deprecated reference이며, wiki가 채워진 뒤 archive로 옮긴다.

## Language
- `some Trait`는 함수 return position 전용 opaque result marker다. 일반 type expression이 아니며 변수/인자/generic argument 위치에는 쓰지 않는다.
- `some Trait` 결과는 `var`로만 받으며, source-level에서는 declared trait surface만 사용할 수 있다.
- `trait`는 static contract다. `interface`는 dynamic/runtime dispatch contract로 따로 둔다.
- `func<R, Params...>`는 callable static contract type expression이다. `lambda<>` type expression은 두지 않는다.
- 일반 `dyn trait`는 도입하지 않는다. dynamic dispatch가 필요하면 명시적 `interface`를 설계한다.
- associated type inference는 v1에서 제외한다. trait 구현체는 `using` 또는 nested type으로 associated type requirement를 명시적으로 충족한다.
- `T&`는 일반 first-class type constructor가 아니라 parameter/return/local alias/implicit this 같은 제한된 surface slot의 reference 표기다.

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
- QIR call lowering은 logical slot model과 passing mode를 사용하고, physical ABI 선택은 후속 lowering에 맡긴다.

## Process
- 기능/버그 수정에는 관련 테스트 추가/갱신을 동반한다.
- Windows 빌드/테스트는 `ai/process/build-test-and-generation-windows.md`를 따른다.
- `.g.cpp`, `.g.h`는 직접 수정하지 않고 generator를 통해 갱신한다.
