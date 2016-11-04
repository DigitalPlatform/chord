using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("DigitalPlatform.MessageServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("DigitalPlatform.MessageServer")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。  如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("7009a4ea-bed7-4176-86b0-75ee940983b4")]

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
[assembly: AssemblyVersion("1.6.*")]
[assembly: AssemblyFileVersion("1.6.0.0")]

// 1.1 (2016/10/12) 增强 ReConnected 事件处理时，获取 ClientIP 时的异常捕获能力
// 1.2 (2016/10/13) 增加 echo() API
// 1.3 (2016/10/15) 为表示通道信息的内存结构增加 Notes 成员。一般被用于存储前端 GetConnection() 的 name 参数。
// 1.4 (2016/10/18) 改进 echo() API，为其增加验证 ConnectionInfo 的功能。所有 API 在 GetConnection() 阶段均能发现和自动纠正 ConnectionInfo 缺失的问题(通过调用前端 close() 实现)
// 1.5 (2016/10/22) 为点对点 API 增加了 LoginInfo 机制
// 1.6 (2016/11/3) 对 SearchTable 和 ConnectionTable 都设立了极限个数。后台线程每五分钟清理一次超时的 SearchInfo 对象，并在日志中记下清除以后集合内剩余的对象数量，从这里可以看出是否出现了对象数量暴涨失控的情况

