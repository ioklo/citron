﻿// $Error

struct S
{
    int x;

    seq int F()
    {
        yield x;
    }
}

S s = S(3);
seq<int> sq = box s.F(); // 에러, box 불가