// 4

ref<int> F(ref<int> i)
{
    return ref i; // 
}

int x = 3;
var y = F(ref x);
y = 4;
@$y