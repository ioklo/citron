# General
함수와 람다의 인자로만 쓰일 수 있습니다. 함수가 리턴을 절대로 하지 않는다는 것을 의미합니다.

```
never panic() { }

never F()
{
    panic();
}
```

빈 함수는 never가 아닙니다. void입니다
```
never F() { } // 에러
```

