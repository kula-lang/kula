# 自定义函数
Kula 对函数的支持很特殊：只允许匿名形式的函数

## 格式
```
<func_name> = func(<arg_name1>:<arg_type>):<return_type> {
    <statement>
    <statement>
    ...
    return <value>;
};
```
注意：
1. 函数声明需要 `func` 关键字
2. 如果有参数，一定要对每一个参数都指定类型。
3. 返回值的类型一定要指定（*你也可以选择不返回值*）。
4. 函数虽然是匿名函数，但是一定要被变量接收！不被接收的函数是无法使用的！（暂时视为特性）
5. 别忘了末尾的分号。因为声明匿名函数也是一个赋值语句。

## 举个例子！
```
print = func(info:Str, val:Num):None {
    println(concat(info, toStr(val)));
};
print("a = ", 5);
```
是不是非常简单！？
