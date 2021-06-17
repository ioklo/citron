// 4

int i = 0;
var x = 3, y = ref i;

var s1 = ref x, p; // 에러, p에 initializer가 없습니다

y = 4;

@$y