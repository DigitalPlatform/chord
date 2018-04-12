﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2Router")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2Router")]
[assembly: AssemblyCopyright("Copyright ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。  如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("7210c4da-4ef2-4142-ae88-da021f51b9f1")]

// 程序集的版本信息由下面四个值组成: 
//
//      主版本
//      次版本 
//      生成号
//      修订号
//
// 可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值，
// 方法是按如下所示使用“*”: 
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.9.*")]
[assembly: AssemblyFileVersion("1.9.0.0")]

// 1.1 (2016/9/16) 第一个可以被 chordInstaller 安装的版本
// 1.2 (2016/10/13) 在日志中记载 HTTP request，对方的 IP。以观察 CPU 耗用过高情况的原因
// 1.3 (2016/10/15) 修正 GetConnection() 时 name 参数的 bug。自动清理(空闲时间超过一个小时的)空闲通道
// 1.4 (2016/10/30) 请求的 HTTP 中携带 _dp2router_clientip 头字段
// 1.5 (2016/11/13) 改用 Logger 写入错误日志
// 1.6 (2016/11/14) 对进入的 HTTP 请求做了 headers 行数、每行字符数、Content-Length 配额限制。
//                  对进入的请求，和响应都改造为 XXXAsync 形式。定时自动清理闲置的 HttpChannel
// 1.7 (2016/11/18) 使用 MessageConnectionCollection 中的通道 UseCount 机制，让并发的请求分散使用 MessageConnection 通道
// 1.8 (2016/11/20) 改进 ReadLineAsync() 为一次尽可能多地读入。自动在错误日志中定时写入 CPU 使用率。
// 1.9 (2016/11/20) 捕获了全局异常，写入错误日志文件。增加了崩溃后自动重启的 Recovery 属性设置(安装 Commit 阶段)
//                  能处理 HTTP 协议的 Connection: Keep-Alive 了。验证请求的 User-Agent 头字段值必须为 dp2LibraryClient，否则拒绝请求。
