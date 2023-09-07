// $Error()

struct S
{
    int x;

    seq int F()
    {
        yield x;
    }
}

void Main()
{
    seq_value<int, sizeof<ref<S>> sv;
    
    {
        S s = S(3);
        sv = s.F(); // 에러, local variable의 seq call, 상위 스코프로 assign 불가
    }
}