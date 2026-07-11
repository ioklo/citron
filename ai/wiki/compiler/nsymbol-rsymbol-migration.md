# `NSymbol`에서 `RSymbol`로의 이행

Status: current implementation snapshot  
Area: compiler, symbol  
Keywords: NSymbol, RSymbol, RDecl, RFactory, migration, trait

## Current State

- 주 컴파일 경로의 declaration 구현과 생성 책임은 `NSymbol`에서 `RSymbol`로 옮겨졌다.
- `RFactory`가 `RDecl`의 소유자이며 `MakeDecl<TDecl>(...)`로 source-origin declaration을 생성한다.
- `SmTranslator`, MIR, QIR, QEvaluator와 EvalTests의 정상 경로는 `RDecl`, `RTypeDecl`, `RFuncDecl` 및 구체 `R*Decl`을 사용한다.
- 이행은 `RNode` 중심 semantic tree 재구성과는 별개의 과도기 단계다. 현재 `R*Decl`은 여전히 tree, category, source payload 책임을 함께 갖고 있으며, 장기적으로는 `RNode`와 payload/category 계층으로 분리한다.

## Completed Migration Scope

다음 declaration 구현은 `RSymbol` 쪽으로 이동했다.

- namespace와 module
- global function 및 공통 function/generic component
- class, struct와 그 ctor/dtor/function/variable member
- enum, enum element와 element variable
- interface, lambda, lambda variable, type parameter
- declaration/type/function result wrapper와 이름, `this` type metadata

번역기 쪽에서는 old `N*Decl`을 경유하지 않고 `R*Decl`을 받아 symbol 구축과 body translation을 수행한다. 함수의 `seq` 여부처럼 declaration surface에 유지하지 않는 source 정보는 syntax task에서 body translation context로 별도 전달한다.

## Deliberately Remaining or Incomplete Pieces

### `NSymbol` compatibility residue

- `NSymbol` 프로젝트에는 현재 `NFactory` thin wrapper가 남아 있다. `RFactory`를 보관할 뿐 declaration을 생성하지 않는다.
- EvalTests는 이 `NFactory`를 생성하지만 현재 번역 경로에서는 사용하지 않는다.
- QEvaluator.Tests와 QlTranslator에는 삭제된 old `N*Decl` API를 참조하는 과도기 코드가 남아 있다. 주 컴파일 경로와 별개이므로, 해당 target을 다시 활성화하거나 test를 정비할 때 함께 제거 또는 이식해야 한다.
- `NSymbol/CMakeSources.g.txt`에는 삭제된 source/header 항목이 남아 있다. 생성 규칙에 따라 build graph를 정리하는 작업이 필요하다.

### Trait / extend

trait와 extend의 parser/AST는 존재하지만 `RSymbol` semantic 구현은 아직 시작 단계다.

- `RTraitDecl`과 `RTraitFuncDecl`은 골격만 있으며 declaration/category API 구현이 없다.
- `RFactory`에는 trait type 생성·interning이 없고 `MakeType(RTypeDecl*, ...)`의 visitor에도 trait branch가 없다.
- `RStructDecl`에는 `RType_Trait*` conformance 목록을 받을 슬롯만 있다. 이 type 자체와 conformance/witness model은 아직 구현되지 않았다.
- SmTranslator의 모든 scope에서 `STraitDecl` 방문은 미구현이고 `SExtendDecl`은 예외를 던진다.

따라서 trait/extend 구현은 old `NTraitDecl`을 복구하는 방향이 아니라, 위 `RSymbol` 모델에 trait declaration, trait type, conformance/witness declaration을 추가하는 작업으로 시작한다.

## Recent Commit Sequence

- `3f9ada56`, `ee7683cc`: 대부분의 `NSymbol` declaration 구현을 `RSymbol`로 이동했다. `RTraitDecl`은 제외됐다.
- `a88dc656`, `6cf30efa`: SmTranslator부터 EvalTests까지 consumer를 `R*` API로 전환했다.
- `4ab9122d`: 이관 중 남은 body translation 문제를 정리했다. `seq`는 syntax task에서 전달하고 `this`는 `RThisKind`의 type을 직접 사용한다. `CanAccess`는 임시로 항상 허용하며 별도 TODO로 남아 있다.

## Follow-up Order

1. Trait의 `RTypeDecl` / `RDecl` / type factory integration을 먼저 완성한다.
2. Trait requirement와 conformance/witness declaration을 symbol 단계에 표현한다.
3. `STraitDecl`과 `SExtendDecl`을 SmTranslator skeleton/task 단계로 연결한다.
4. `NSymbol` 잔재 target, test, generated build-source 목록을 별도 정리한다.
5. 이후 `RDecl`의 tree 책임을 `RNode`로 점진적으로 이행한다.

## Related Documents

- `declaration-model.md`: `RDecl`/`RNode` 장기 모델
- `../language/trait-and-interface.md`: trait, extension, impl, extend의 언어 규칙
- `../../notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`: declaration model 논의 기록
