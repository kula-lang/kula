# 程序结构
从结构角度讲，Kula 是一个面向过程的语言。整个程序由多条语句或语法块构成。

## 一个 HelloWorld 实例
```python
# hello_world
println("hello_world");     
```

我们可以看到：
1. 程序中只包含一条语句
2. 语句由分号结尾
3. 分号后面是一条注释，用 `#` 符号开头，遇到换行符结束

值得注意的是：
1. Kula 的程序会按照语句的顺序逐个执行
2. Kula 是大小写敏感的，大写和小写字符将被视为不同的字符
3. 所有 **语句** 必须以分号 `;` 结尾，否则将不被视为一个完整的语句

## 执行之？
让这个语句运行起来，我们有两种不同的方式：
1. 利用REPL模式打开 Kula 主程序，在命令行内键入完整的语句
2. 利用脚本模式，首先在 `.kula` 文件中编写完整的程序代码，再让 Kula 主程序打开之

最终，如果您能看到 `hello_world` 出现在屏幕内，那么您就成功了！     
当然，Debug模式下，您会看见更多的内容
```shell
>> println("hello_world");
Lexer ->
        < NAME      | println   >
        < SYMBOL    | (         >
        < STRING    | hello_world>
        < SYMBOL    | )         >
        < SYMBOL    | ;         >

Parser ->
        [ STRING    | hello_world]
        [ FUNC      | println   ]

Output ->
hello_world

VM ->
        End Of Kula Program
```