// 2

void F(out int* i)
{
    *i = 2;
}

int j = 3;
F(out &j); // out을 반드시 써줘야 합니다

@$j