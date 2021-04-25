# scope 변수를 가진 struct boxing하기

// 1. 
```csharp
scope struct X // stack대신 scope용어를 썼다
{
    ref<int> i;    
    default_constructor;
}

// 여기서 box<X> 는 box<free<X>> 이다
box<X> b = box X(3);
```

// 2. handle member scope struct
scope struct Y
{
    ref<int> j;
}

scope struct X
{
    ref<int> i;
    Y y1;
    ref<Y> y2;
}

// 1. free를 도입할까
free<X> x; // scope struct

struct free<X>
{
    int i;      // 일반 struct의 ref는 일반 타입으로..
    free<Y> y1; // scope struct면 free 적용
    free<Y> y2; // scope struct의 ref는 free 적용
}

// free를 손으로 못쓰게 한다
box<X>는 box<free<X>>
box<free<X>>는 not allowed

X x1;
free<X> x2 = x1; // 복사, 왕창 복사

