// 4

int i = 0;
var x = 3, y = ref i;

ref var s1 = ref x, p; // 에러, p에 initializer가 없습니다

bool b = true;
ref var pi = ref i, pb = ref b; // pi는 ref int, pb는 ref bool 입니다

y = 4;

@$y