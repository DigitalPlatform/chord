using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2Capo")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2Capo")]
[assembly: AssemblyCopyright("Copyright © 数字平台(北京)软件有限责任公司 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。  如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("e6066657-9063-44ea-b3a8-cb7bb604d2cf")]

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

// 1.1 (2016/6/26) 首次使用了版本号
// 1.2 (2016/9/14) 管理线程中会不断重试连接 dp2mserver，并将此情况写入日志
// 1.3 (2016/9/15) 修正连接 dp2mserver 时重叠调用 BeginConnect() 的问题
// 1.4 (2016/10/10) 增加 _cancel, 让退出更敏捷
// 1.5 (2016/10/12) 增加了防止 MessageConnection.CloseConnection() 函数重入的机制
// 1.6 (2016/10/13) 为后台线程增加了 echo 机制，并详细化了日志信息
