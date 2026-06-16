# Compile Pipeline

Status: draft current
Area: compiler, build
Keywords: skeleton, cti, body compile, lowering, rcti

## Current Shape
Opaque `some` result metadata model을 기준으로 compiler phase는 아래처럼 본다.

```text
1. Skeleton / CTI phase
   - source declaration surface 수집
   - stable declaration identity 확보
   - function signature와 conformance surface 기록
   - some return은 opaque result identity와 metadata accessor contract로 기록

2. Body compile phase
   - function body typecheck
   - overload resolution
   - some return backing type 검증
   - metadata accessor / value witness / trait witness 구현 준비
   - MIR/QIR/lowering 진행

3. Consumer compile
   - imported cti를 읽음
   - some return 호출 시 metadata accessor를 사용
   - value witness로 unknown-size storage를 할당/파괴
   - trait witness로 trait method 호출
```

## CTI
`cti`는 import/typecheck/codegen boundary다.

`some` function에 대해 `cti`는 concrete backing type 대신 아래 contract를 제공한다.
- opaque result identity
- metadata accessor symbol
- opaque sret calling convention marker
- declared trait constraint

## RCTI
Consumer-facing `rcti`는 현재 모델에서 없어질 수 있다.

이전 `rcti`가 맡던 "backing type/layout을 consumer에게 알려주는 역할"은 metadata accessor와 value witness가 대신한다.

다만 compiler 내부 incremental cache로 resolved body information을 저장할 수 있다.

## Open Points
- same-module unit dependency를 implicit으로 둘지, `using unit`을 재도입할지
- body compile cache와 official import artifact를 어떻게 구분할지
- public opaque backing 변경이 downstream rebuild에 어떤 영향을 주는지

## History
- `ai/implementations/compiler-lowering-snapshot.md`
