# 自定义函数
函数，可以理解为对多个语句和语法块的封装。

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

## 闭包！！！
Kula 语言已经支持了正确的闭包，我们用一个简单的例子来验证！
```
make_counter = func():Func {
    n = 0;
    return func():None {
        n = plus(n, 1);
        println(n);
    };
};

foo = make_counter();
foo();
foo();
foo();
foo();
```
闭包的思想在这里不做过多的讨论。    
总之，Kula 可以正确的完成闭包所要做的事。