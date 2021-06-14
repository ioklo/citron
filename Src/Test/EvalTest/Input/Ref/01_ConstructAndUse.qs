// 3

int s = 3;
var i = ref s; // i: ref int
@{$i} // 이후에 i는 int로 보여야 한다, IR0에서는 DerefLoc(LocalVarLoc(i))
