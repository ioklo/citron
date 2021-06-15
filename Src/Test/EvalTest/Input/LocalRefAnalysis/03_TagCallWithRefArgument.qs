// $Error()
struct S
{
    seq int F(ref int i)
    {
        yield i;        
    }
}

{
    int i = 6;
    S s = S();

    var sq = box s.F(ref i); // 에러, ref 가 들어가는 seq call 값은 ref대상의 스코프를 벗어날 수 없다
}