// 2

void F(ref int i)
{
    i = 2;
}

int j = 3;
F(ref j);

@$j