# 2026-07-21 Global Identifier Encoding

## Status

`GlobalDeclIdentifier` / `GlobalTypeIdentifier`의 canonical 문자열 형식을 정리했다. 이 문서는 상세 문법과 보류 항목을 기록한다. 현재 유효한 요약 규칙은 `ai/wiki/compiler/declaration-model.md`와 `ai/wiki/current-decisions.md`에 둔다.

## Goal

- 외부 module declaration도 stable 문자열 identifier로 찾을 수 있어야 한다.
- identifier는 live `RType*`로 역직렬화할 필요가 없다. canonical 문자열 equality/hash만으로 semantic identity를 비교할 수 있으면 된다.
- 생성 순서 의존 ordinal은 module boundary identity에 사용할 수 없다.

## Identity Layers

```text
RName                 lookup surface / overload family key
RIdentifier           same outer 안의 exact declaration key
GlobalDeclIdentifier  module prefix + RIdentifier path
GlobalTypeIdentifier  canonical type expression
```

예:

```text
RName:                 F
RIdentifier:           F(N$pi)
GlobalDeclIdentifier:  Core::N.S.F(N$pi)
```

`RIdentifier`는 canonical string을 보관하는 얇은 value wrapper로 둘 수 있다. `RName`은 lookup용으로 유지한다.

## General Encoding Rules

- `RName_Normal`은 raw text를 사용한다.
- normal name 안의 `$`, `,`, `.`, `:`, `(`, `)`, `<`, `>`는 `$`를 앞에 붙여 escape한다.
- 인자가 없으면 tag만 쓴다.
- 인자가 하나면 tag 뒤에 바로 붙인다.
- 둘 이상의 argument는 `,`로 나누며, `RName` payload에는 `(...)`, `RType` child/type argument에는 `<...>`를 쓴다.
- canonical identifier에는 whitespace를 넣지 않는다.

## RName Encoding

```text
$N                 RName_None (root 외에는 보통 사용하지 않음)
Foo                RName_Normal("Foo")
$RE                reserved name (E는 Enumerator 같은 fixed code)
$L30               RName_Lambda(30)
$C(20,x)           RName_CtorParam(20, "x")
$I(S,Core::Tr<$T0>) RName_ImplTrait
```

reserved-name code의 정확한 문자 mapping은 안정적으로 고정해야 하며 C++ enum ordinal에 의존하지 않는다.

`RName_ImplTrait`의 첫 argument는 current lexical outer의 direct target type member여야 하는 `TargetRIdentifier`다. 두 번째 argument는 항상 module prefix를 포함한 `TraitGlobalTypeIdentifier`다. 따라서 같은 module trait에도 prefix를 생략하지 않는다.

```text
Core::N.$I(S,Core::N.Tr<$T0>)
```

이 규칙은 canonical impl target이 same-outer direct type member라는 현재 제약에 의존한다. future extension/specialized target은 별도 target-pattern identity 규칙을 도입할 수 있다.

## Function Identifier

function parameter encoding은 passing-kind code와 type ID를 붙인다.

```text
N$pi    Normal int
R$pi    Ref int
I$pb    In bool
M...    Move
F...    Forward
O...    Out
P...    Params
i...    Init
```

함수의 return type은 declaration identity에 넣지 않는다.

```text
F(N$pi,IS1.S2<$T0,$pb>)
```

passing kind만 다른 함수를 overload로 허용할지, function generic signature를 `RIdentifier`에 어떻게 반영할지는 아직 별도 language/overload policy가 필요하다.

## Type Encoding

```text
$V                 void
$pi                int
$pb                bool
$T0                canonical type-variable binder slot
$N$pb              nullable<bool>
$n$pb              nullable-inplace<bool>
$P$pi              ptr<int>
$S$pi              shared<int>
$B$pi              box<int>
$t<$pi,$pb>        tuple<int,bool>
Core::S1.S2<$T0,$pb> nominal constructed type
```

unary type constructor는 child가 하나이므로 bracket을 생략한다. tuple과 nominal type은 여러 child boundary가 필요하므로 `<...>`를 사용한다.

nominal type argument는 현재 `RTypeArguments`의 flattened order를 그대로 쓴다.

```text
S1<T1>.S2<bool> -> Core::S1.S2<$T0,$pb>
```

outer/inner declaration segment마다 argument를 분배해 `S1<$T0>.S2<$pb>`로 보이는 표기는 후속 과제로 남긴다.

tuple label은 type identity에 포함하지 않는다. 현 `RType_Tuple` interning key가 label을 포함하는 구현은 tuple 의미를 별도로 다룰 때 이 규칙에 맞춰 정리해야 한다.

`RType_Func`, lambda type, opaque return type은 현재 encoding 범위에서 보류한다. opaque는 return position 전용이므로 함수 declaration ID에는 들어가지 않는다.

## Remaining Questions

- `GlobalDeclIdentifier`의 module prefix는 우선 one-segment `ModuleName::`로 둔다. module name이 compilation/import universe에서 유일하지 않게 되면 package/registry identity를 `ModuleIdentifier` 규칙으로 확장한다.
- `$T0`의 canonical binder-slot numbering과 impl-header alpha-equivalence 규칙.
- transparent type alias를 normalize한 뒤 type ID를 생성하는 정확한 resolver API.
- function generic signature 및 passing kind의 overload identity 정책.
