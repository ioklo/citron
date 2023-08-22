# General
이름을 붙일 수 있는 일련의 타입 묶음입니다. 값 중심 타입입니다.

# 타입 선언
```
tuple_type_exp = '(' tuple_item_list ')'

tuple_item_list = type_exp id ',' type_exp id      // 무조건 두개 이상
                | tuple_item_list ',' type_exp id
                      
```

```
type t = (int i, string s, bool b);
```

# 값 만들기 및 멤버 사용
```
(int i, string s, bool b) x = (3, "hi", false);
 
var x = (i: 3, s: "hi", b: false);

@${x.b}
```

# 이름 생략하기
이름을 생략하면 멤버를 0, 1, ... 처럼 숫자로 접근할 수 있습니다.
```
var x = (3, "hi", false);

@ ${x.1}
```

# 멤버 이름은 상관없는 타입
`struct`와 다른 점은 타입 순서만 맞으면 같은 타입으로 인식한다는 점입니다. 
```
struct S1 { int x; int y; }
struct S2 { int x; int y; }

S1 s1 = S1(3, 4);
S2 s2 = s1; // 에러, 다른 타입에 넣으려고 했습니다
```

```cs
(int x, int y) t1 = (x: 2, y: 3);
(int s, int t) t2 = t1; // 가능
```

[Tuple을 함수 인자로 사용하기](Function#use-tuple-as-function-arguments.md)