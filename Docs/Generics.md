# General
사용자 정의 타입이나 함수에 타입을 인자로 넘겨줄 수 있습니다. 사용자는 상황에 따라 타입을 대입해서, 타입이 다르지만 로직은 비슷한 코드를 재사용할 수 있게 됩니다.

Symbol type, member functions, global functions 에 타입 매개변수를 정의할 수 있습니다.
%%Test(Generics_General, 3)%%
```
struct A<T>
{
    T t;
}

void Main()
{
    var a = A<int>(3);
    @$a.t
}
```

# Nested Type Parameters

Symbol의 outer에 type parameter가 있다면, Symbol도 그 type parameter를 쓸 수 있습니다. symbol의 outer개념은 class나 struct의 base와 다른 개념입니다. base의 type parameter는 항상 instantiated 되기때문에 존재하지 않습니다.

```
class C<T>
{
    struct S<U>
    {
        T t;
        U u;
    }
}

var s = C<int>.S<string>(3, "hi");
```

같은 이름의 파라미터인 경우 가장 마지막에 선언된 정의가 이전 정의를 가립니다. 가린다고 해서 이전 타입 파라미터가 쓸모 없어지는 것은 아닙니다.
```
class C<T>
{
    T t;
    
    struct S<T>
    {
        T t;
    }
}

var s = C<int>.S<bool>(false); // bool이 사용됩니다.

```

# Type Constraint
interface를 사용해서 타입 매개변수에 제약사항을 줄 수 있습니다. constraint 용어는 타입인자로 넣을 수 있는 관점에서 제약의 뜻이지만, 실제로 쓰이는 쪽에서는 interface에 선언된 작업만큼 할 수 있는 일이 더 늘어납니다. where 절을 사용해서 constraint를 설정할 수 있습니다
```cs
interface ICancellable<T>
{
    static T New();
    void Cancel();
}

void F<T>()
    where T : ICancellable<T>
{
    var t = T.New();
    t.Cancel();
}
```
