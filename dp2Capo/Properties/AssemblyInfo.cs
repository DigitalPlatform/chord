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
[assembly: AssemblyVersion("1.40.*")]
[assembly: AssemblyFileVersion("1.40.0.0")]

// 1.1 (2016/6/26) 首次使用了版本号
// 1.2 (2016/9/14) 管理线程中会不断重试连接 dp2mserver，并将此情况写入日志
// 1.3 (2016/9/15) 修正连接 dp2mserver 时重叠调用 BeginConnect() 的问题
// 1.4 (2016/10/10) 增加 _cancel, 让退出更敏捷
// 1.5 (2016/10/12) 增加了防止 MessageConnection.CloseConnection() 函数重入的机制
// 1.6 (2016/10/13) 为后台线程增加了 echo 机制，并详细化了日志信息
// 1.7 (2016/10/14) 增加了 ping 机制
// 1.8 (2016/10/15) 对 dp2Capo 调用 echo 的方法进行了加固，增加了超时返回机制，进行了异常捕获
// 1.9 (2016/10/16) search() API 的 getSystemParameter 功能增加了 QueryWord 为 _lock 和 _capoVersion 两种参数
// 1.10 (2016/10/17) 在 Connection 处于连接状态的情况下，轮询检查为十分钟一轮；处于未连接状态下，一分钟检查一轮。这样日志输出文字量大大减少了
// 1.11 (2016/10/18) SetMessageAsync() 调用后不直接 ResetConnection()，因为 dp2mserver 新版具备自动 Reset Connection 功能了。每分钟调用一次 echo() 能做 ConnectionInfo 丢失自动修复
// 1.12 详细了日志信息，handlers 做了释放
// 1.13 (2016/10/22) 点对点 API 增加了 LoginInfo 机制
// 1.14 (2016/10/28) 加回去了两处 TryResetConnection(result.String);
// 1.15 (2016/10/31) ConnectAsync() 和 CloseConnection() 改为采用整数防止重入。实际上是发现重入以后会直接跳出。另外每隔二十分钟自动清除 ChannelPool 中闲置的 LibraryChannel，此举主要是考虑到微信公众号模块采用读者身份进行各种操作了，因而申请的 LibraryChannel 可能数量较多。
// 1.16 (2016/11/2) ConnectAsync() 和 CloseConection() 又改回 lock 方法。注释掉 Timer 相关的语句。ServerConnection 中的 virtual ConnectAsync() 也被注释掉了。为 dp2Capo 的 getSystemParameter 功能增加了日志语句
// 1.17 (2016/11/3) echo 的时候也可能会遇到  异常 Microsoft.AspNet.SignalR.Client.HttpClientException:StatusCode: 401, 增加了此时 TryResetConnection() 动作。另外原来的 TryResetConnection() 只有在特定 code 情况下才做动作，这次注释掉这个 if，都做动作了
// 1.18 (2016/11/4) MessageConnection 增加 Connection_ConnectionSlow()，开始重新连接
// 1.19 (2016/11/5) 在 MessageConnection 的 CloseConnection() 中恢复 Connection.Stop() 语句。TryResetConnection() 里面采用 CloseConnection()
// 1.20 (2016/11/7) 当 SetMessage() 出错的时候，要根据错误码判定是否重置连接，而不是一味重置连接。后者这种处理方法可能会(在 SetMessage() 因为其他原因出错的时候)造成网络繁忙
// 1.21 (2016/11/8) 在 ConnectAsync() 中释放以前通道的过程中，加回来曾经被去掉的 Connection.Stop() 调用
// 1.22 (2016/11/12) 加入 ServicePointManager.DefaultConnectionLimit = 12;
// 1.23 (2016/11/12) 增加 LifeThread，自动检测死锁并自动退出。安装时自动为 dp2CapoService 配置恢复特性为自动重新启动，可解决故障恢复问题
// 1.24 (2016/11/17) 点对点 API Circulation() 增加 VerifyBarcode 功能。
// 1.25 (2016/11/30) 增强了 SetMessage() API，可以分次发送大于 4096 bytes 的 MessageRecord.data 内容
// 1.26 (2017/4/25) 点对点 API Circulation() 中的 VerifyBarcode 功能中增加了 TransformBarcode 子功能。
// 1.27 (2017/5/10) 点对点 API Search()中的 SearchRequest 结构增加了 Filter 成员。dp2capo 支持带有馆代码限定的 SearchBiblio() 检索了
// 1.28 (2018/9/27) Z39.50 Service 基本稳定
// 1.29 (2018/9/28) Z39.50 Service 中，进一步加强了对 Present 请求参数的检查，避免获取记录阶段出现巨量的循环
// 1.30 (2018/10/2) 修正了点对点 GetRes() API 中当 param.Data 为 Length 0 的时候的一处无法返回响应导致超时的 bug
// 1.31 (2018/10/11) Z39.50 服务器 TCP 通道能在检索中途感知到前端切断通道的动作，并及时让 LibraryChannel Abort()。清理全局结果集按照分批进行，保证每批请求的规模不至于过大
// 1.32 (2018/10/22) 自动检测高内存耗用(超过 1G)，触发自动重启 dp2capo.exe
// 1.33 (2018/10/30) SIP Service 对于 ScStatus 缓存 dp2library get hangup 结果。大约每分钟才真正去 dp2library 获取一次。ScStatus 动作也不再进入 Info 日志
// 1.34 (2018/11/13) Z39.50 Service 中消除了 BuildPartTree() 中出现死循环的 bug。遇到有问题的 BER 包，会记入错误日志
// 1.35 (2021/3/3) SIP Service 对 ItemInfomation ItemStatusUpdate Checkout Checkin Renew 请求都要求正确使用 AO 字段 
// 1.36 (2021/4/17)
// 1.37 (2022/2/28) SIP Login() API 可以接纳 CP 字段，若其内容以感叹号开头，还会用于 dp2library Login() API 的 parameters location=xxx 子参数中
//                  SIP Return() API 可以接纳 AP 字段，用于 dp2library Return() API 的 strStyle 子参数 currentLocation=xxxx
// 1.38 (2022/3/4) SIP 账户配置增加了 bookUiiStrict 参数，用于定义是否严格要求图书号码为 UII 形态。影响到 checkin checkout renew iteminfo 四个 API
// 1.39 (2022/3/22) SIP 账户配置增加了 maxChannels 参数，用于定义该账户允许最多使用的 TCP 通道的数量。实际上前端连接 TCP 通道数可以超过这个数量，但登录成功的通道数量会被限制在这个数量之类，超出的登录会失败
// 1.40 (2022/3/28) 改进优化 SIP 匿名登录功能。所谓匿名登录就是不登录也可以使用 SIP 服务器。当然此时再登录也是可以的，登录则按照原来登录方式处理
//                  ScStatus 功能修正了以前版本返回状态不准确的 bug