// 4

int i = 0;
var x = 3, y = ref i;

ref var s1 = ref x, p; // ����, p�� initializer�� �����ϴ�

bool b = true;
ref var pi = ref i, pb = ref b; // pi�� ref int, pb�� ref bool �Դϴ�

y = 4;

@$y