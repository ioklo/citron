// hi hello world world true true false false onetwo true false true true false false true false true true

string s = "hi";
string t = s;

s = "hello"; // 

@$t $s ${s = "world"} $s

string t2 = "${"h"}${"i"}";
@ ${t == t2} ${s == "world"} ${t != t2} ${s != "world"}

@ ${"one" + "two"}

@ ${"s1" < "s1abcd"} ${"s1abcd" < "s1"} ${"s1" <= "s1abcd"} ${"s1" <= "s1"} ${"s1abcd" <= "s1"}

@ ${"s1" > "s1abcd"} ${"s1abcd" > "s1"} ${"s1" >= "s1abcd"} ${"s1" >= "s1"} ${"s1abcd" >= "s1"}
