# 逻辑控制块
Kula 中的逻辑控制被简化成了两个基本模型：if 和 while

## if 逻辑块
```
if (<Num>) {
    <statement>
    <statement>
    <statement>
    ...
}
```
非常简单，仅当 `Num` 的值为 `true` 时才会执行大括号里的内容。

值得注意的是：
1. Kula 没有 `else` 语句
2. 大括号是不可以被省略的，再次强调，Kula 中的所有括号都是 **不可添加不可缺省不可替代** 的

## while 逻辑块
```
while (<Num>) {
    <statement>
    <statement>
    <statement>
    ...
}
```
和 `if` 类似，但 `while` 的大括号内容执行结束后，会跳回条件判断处，再次重复以上过程

* 注意不要写死循环。