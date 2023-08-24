# General
타입은 저장소가 실행 중에 올바른 모양의 값을 가지고 있도록 보장해주는 역할을 합니다. 타입은 크게 사용자 정의 타입과 결합타입으로 나눌 수 있습니다. 사용자 정의 타입은 코드에 타입을 선언해서 프로그램 내부에서 Symbol의 형태로 참조하게 됩니다. `struct`, `class`, `interface`, `enum`, `type variable` 이 이에 해당합니다. 결합타입은 Symbol 형태로 존재하지 않고, 다른 타입들을 조합해서 만들수 있습니다. `void`, `nullable`, `local pointer`, `box pointer`, `tuple`, `func<>`, `seq<>`, `task<>`, `list<>`등이 이에 해당합니다. 
# Type Expression

코드에서는 다음과 같이 타입을 표현할 수 있습니다
```
// 최상위
<TYPEEXP> = <NULLABLE_TYPEEXP>                     // T? 
          | <BOXPTR_TYPEEXP>                       // box T* 
          | <LOCALPTR_TYPEEXP>                     // T*
          | <ELSE_TYPEEXP>

<ELSE_TYPEEXP> = <MEMBERCHAIN_TYPEEXP>             // A.B.C
               | 'void'                            // void
               | '(' <TUPLE_LIST> ')'              // (T0, T1)
               | 'func' '<' <TYPEEXP> ',' <TYPEARGLIST> '>'      // func<T0, T1, TResult>
               | 'seq' '<' <TYPEEXP> '>'           // seq<int>
               | 'task' '<' <TYPEEXP> '>'          // task<int>
               | 'list' '<' <TYPEEXP> '>'          // list<int>
               | 'dict' '<' <TYPEEXP> ',' <TYPEEXP> '>' // dict<Key, Value>

<NULLABLE_TYPEEXP> = <BOXPTR_TYPEEXP> '?'          // box T*?
                   | <LOCALPTR_TYPEEXP> '?'        // T*?
                   | <PAREN_TYPEEXP> '?'           // (T*)?
                   | <ELSE_TYPEEXP> '?'            // A.B.C?

<BOXPTR_TYPEEXP> = 'box' <PAREN_TYPEEXP> '*'       // box (T*)*
                 | 'box' <ELSE_TYPEEXP> '*'        // box A.B.C*

<LOCALPTR_TYPEEXP> = <PAREN_TYPEEXP> '*'+          // (T?)***
                   | <ELSE_TYPEEXP> '*'+           // A.B.C***

<PAREN_TYPEEXP> = '(' <NULLABLE_TYPEEXP> ')'.      // (T?)
                | '(' <BOXPTR_TYPEEXP> ')'         // (box T*)
                | '(' <LOCALPTR_TYPEEXP> ')'       // (T*)

<MEMBERCHAIN_TYPEEXP> = <ID>                       // A
                      | <ID> '<' <TYPEARGLIST> '>' // A<int>
                      | <MEMBERCHAIN_TYPEEXP> '.' <ID> // A.B
                      | <MEMBERCHAIN_TYPEEXP> '.' <ID> '<' <TYPEARGLIST> '>' // A.B<int>

<TUPLE_LIST> = <TYPEEXP> <ID> ',' <TYPEEXP> <ID> // 무조건 두개 이상
             | <TUPLE_LIST> ',' <TYPEEXP> <ID>
                      
// 타입 인자리스트
<TYPEARGLIST> = <TYPEEXP> // int
              | <TYPEEXP> ',' <TYPEARGLIST> // int, short

```


# Type Alias
type alias를 사용해서 type expression에 이름을 줄 수 있습니다.
type alias는 Symbol Directory안에 만들어지며, 모듈참조를 통해 접근이 가능합니다

```
type Identity<X> = X; // type parameter도 쓸 수 있습니다
Identity<int> i = 3;  // int i = 3;

```

컴파일러가 컴파일 시점에 순환참조가 발견하면 에러를 냅니다.
```
type A = B;
type B = A;
```


