# Kula 语言
> Kula 是一个轻量、高扩展的 基于 .NET 的解释型动态编程语言。

![Kula_Daiamondo](https://gimg2.baidu.com/image_search/src=http%3A%2F%2Fhbimg.b0.upaiyun.com%2F59af30fb8e979539fe816369c6dd37b06a3ee9a984f42-4G7yBG_fw658&refer=http%3A%2F%2Fhbimg.b0.upaiyun.com&app=2002&size=f9999,10000&q=a80&n=0&g=0n&fmt=jpeg?sec=1624933443&t=32e57f6dcb44de881f052a290ea0eddc)

## 简介
Kula 是一个轻量解释型动态语言。      
这是由 [*Hana Yabuki (AKA. 会唱歌的花枝丸)*](https://github.com/HanaYabuki) 个人开发的，自拟标准的 **玩具语言**。    
它可以基于 Microsoft .NET 5 框架 (或以上) 运行在 Windows 系统 (以及通过 .NET 框架在其他操作系统) 的计算机上。 

Kula 语言诞生于 2021 年 5 月 下旬，并且正在高速的发展之中。    
所以，Kula 语言会在不久的未来支持越来越多的现代编程语言 ~~(特指Hana喜欢的)~~ 特性。   

## Kula语言 的特点
* 生态
  * 环境易部署，可独立存在，也可嵌入其他 C# 项目
  * 和 C# 双向交互，可由 C# 实现高度的扩展性
  * 模块化编码，支持动态引用其他原生库
  * [优雅美观的官方文档](https://kula-lang.github.io/) 

* 语言本身
  * 基础语法形似主流语言
  * 强类型 + 动态类型
  * 面向过程
  * 函数式
  * 基于原型面向对象
  * *数组* 和 *字典* 数据容器
  * 支持灵活的对象强类型校验和高阶类型校验


## [官方帮助文档 *( 点我跳转 )*](https://kula-lang.github.io/)
> 单独的使用 Kula 语言不需要太多的前置知识，可以不学习 C#。    
> 但是 Kula 语言只有和 C#共同使用才能发挥其最大的作用！

## 伴生项目
* [Kula - Diana : Kula 语言的 VSCode 插件](https://github.com/kula-lang/Kula-Diana)
* [Kula - Guide : Kula 语言的 完全帮助文档](https://github.com/kula-lang/kula-lang.github.io)

------

## 更新日志

### 2022-1
- 1-15
  * 编译期异常规范化
  * 允许使用双线箭头 `=>` 定义函数及函数声明的返回值类型
  * ~~（偷偷改名字不过分吧）~~

### 2021-12
- 12-14
  * 允许 REPL 多行输入并运行代码
  * 改良模块部分，允许循环模块引用
  * 大幅改良异常系统
    * 封装异常
    * 编译期异常细节完整捕获，并输出行号
    * 运行时异常细分
  * ~~（偷偷给一些数据结构改了名儿，嘿嘿）~~

- 12-7
  * 小幅度调整终端参数语法和内核

### 2021-11
- 11-17
  * **`$import` 模块化语法支持** （Kula终于能给人用了！ 

- 11-15
  * **函数允许作为高阶类型存在** （从此Kula语言进化为真正的强类型！

- 11-14
  * 鸭子类型基本支持

- 11-13 
  * **新增鸭子类型 DuckType，允许对 `Map` 类型进行校验** 
  * ~~并带来了许多BUG~~

- 11-10 <small>*非常优雅*</small>
  * **PIPE语法** bug修复，非常优雅
  * *支持 REPL 模式下变量回显*，非常优雅
  * **Debug 模式移除，改成了位运算式变量配置**，非常优雅

### 2021-10
- 10-30
  * 标准库大部分封装到对应的 `Map` 结构内，隔离命名
  * 添加新函数 `unpack`、`unpackAll` 用以拆包

- 10-21
  * 项目迁移到 .NET 5.0
  * ~~怎么全是bug~~
  * 修复了 Parser 溢出的 bug，避免了许多迷惑的 SyntaxError
  * 微调版本信息

### 2021-9
- 9-17
  * **添加语法：PIPE 管道操作**，可以通过 `|` 运算符进行链式的函数调用
  * 调整 `null` 相关错误信息

### 2021-9
- 9-9
  * ~~好久没更新了~~
  * `Map` 容器增加操作：`remove(map: Map, key: Str): None` 移除元素

### 2021-7
- 7-29
  * 部分重写了 Parser，现在的 Parser 巧妙利用了许多 C#特性，封装性更好且副作用小
  * **支持与 `if` 对应的 `else` 语法**

- 7-12
  * 更改了 Shell 的使用方式
  * ***测试版放出来给大家试吧试吧！***

- 7-11
  * 添加内置常量语法
  * 更改编译器底层，确保了编译器线程安全

- 7-10
  * 添加 `null` 关键字，允许在 Kula 中使用空值 ~~(后果自负)(非常不建议使用)~~
  * 重写底层 Kula Func 机制，简化 `BFunc` 委托格式，确保 C# 可以直接*轻便的*调用 Kula 函数。
  * 完全支持 Kula-C# 交互。`KulaEngine`类 添加 `Call` 方法，允许在 C# 中调用 Kula 的函数

- 7-8
  * 修复 `Func` 类型 `toStr` 的错误
  * 底层实现机制稍稍改动，用了许许多多的自动属性（C#天下第一
  * `BuiltinFunc` 类型反复横跳...
  * 在 `KulaEngine` 中默认加入了一个 `Map`，用以 C# 和 Kula 交换数据
  * 添加用于遍历 `Map` 集合的 `for(m: Map, op: Func): None` 函数

- 7-1
  * 修复了Debug模式下的一些Bug，现在 Kula 可以很棒的记录运行时长了~

### 2021-6
- 6-29
  * ~~考试复习中途摸鱼~~
  * 取消了懒加载机制，以及下面的...
  * 更改了 `Func` 类型的 `toStr()` 方案，使得 `Func` 能够完整的反映其类型
  * 修改了异常的格式，使之更完整的反映错误信息

- 6-25
  * ~~咕咕咕~~
  * 删除了 `Queue<object>` 的 C# API，若想使用自行扩展
  * 加上了 *看着挺像那么回事儿* 的文档注释

- 6-14
  * 允许**使用 C# 补充实现 `BuiltinFunc` 来扩展内置函数**，允许覆盖内置函数
  * 补充了少量内置函数
  * 修正了和底层 C# 的交互方式，降低编译负担
  * ~~悄悄删除了所有的Release，一切重新开始~~

- 6-10
  * 底层重写，分离 控制台程序 和 KulaEngine
  * 允许作为DLL类库，嵌入C#程序
  * ~~允许通过 `Queue<object>` 和 C# 交互，(当然也增加了对应函数)~~
  * 默认启动模式改为 Release

- 6-6
  * 支持转义字符串
  * LexToken 和 VMNode 结构调整，效率提高

- 6-3
  * 添加了 *作为引用类型的* **数组** **字典**
  * 对应 数组和字典 增加了新的语法和内置方法
  * 添加了生硬的异常机制，以及对应的 `throw(msg:Str):None` 异常抛出函数

- 6-2
  * 重新设计了函数的语法解析方式 (parser)，**实现了函数柯里化**
  * 重写了字节码的解析
  * 增加了少量内置函数

- 6-1
  * 增加了 lambda 类型的词法分析支持
  * 底层寻址机制重写，运行速度显著提升
  * 添加了 Debug 模式下的计时功能，更改 格式化细节
  * 增加了 返回值类型检查 (本来就该有的，一直忘了加

### 2021-5
- 5-30
  * 第三次底层重写，合并了主流程和函数流程，代码总量降低
  * 扩展了 lambda 的存在形式，更改了函数的底层数据结构
  * 重量级特性：**正确的闭包支持**
  * ~~写的时候没注意，妈的，Lambda寻址是错的，根本不支持递归~~
  * 更改了寻址方案，性能略微降低 (未来可能会优化)
  * 依然略有混乱 (但有助于编码) 的错误信息

- 5-29
  * 更多的内置函数 (输入处理，字符串解析)
  * 部分底层重写，分离主流程和函数流程
  * 重量级特性：**初步支持lambda演算**，**lambda懒加载** (我可是一开始就做了懒加载呢吼吼吼~！)
  * 更加诡异的报错信息
  * ~~*由于当前底层为引用类型，在闭包的使用中可能会出现一些诡异的现象，暂时不要过分相信Kula的闭包*~~ (已修复)
  * *暂未支持函数柯里化，未来的版本会对Parser进行一些调整来支持柯里化*

- 5-26
  * 较为完善的基本语法
  * 动态强类型系统
  * 足够基本使用的内置函数
  * 不完全封装的错误提示

- 5-23
  * 测试级的语法
  * 逻辑基本不能使用
  * 不够完善的错误提示

------

## 项目其他信息
### 主要负责人
> 1. [会唱歌的花枝丸 - Hana Yabuki on @github.com](https://github.com/HanaYabuki)

### 参与贡献代码的方式
> 1. 直接和[花枝丸](https://github.com/HanaYabuki)击剑
> 2. 参与到相关申必组织
> 3. *PlaceHolder*

### 开源协议
> [GPL3.0](./LICENSE)

### FAQ
#### **Q1** :     
Kula 语言是什么？我怎么没听说过？我需要学习他吗？        
  
#### **A1** :     
Kula 暂时只是一个玩具语言，不具有 **很强的工程性 或 学习价值**，仅作为个人习编译原理的一个中间产物。  
但是，如果您对这个项目感兴趣，并且 **愿意对语言加以研究** 或 **改进语言的一些细节设计**，鄙人感激不尽。  

#### **Q2** :     
为什么叫 *Kula* 语言？    

#### **A2** :     
因为 立项的时候 Hana 在打拳皇。   
和 "为什么 Java 叫 Java" 同理 (🍀
