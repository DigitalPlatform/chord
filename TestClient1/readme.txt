

2015/12/20
TODO: 让 TextBox 随内容自动撑高
TODO: XML 字符串在 TextBox 要显示为缩进的格式

TextBox 已经改为用 WebBrowser 了

~~~

实现 SetReaderInfo() API。目标场景是创建、修改读者记录。

实现 GetEntities() SetEntities() API。目标场景是册登记。

实现 Borrow() Return() API。目标场景是借书、还书、续借。

实现 Reservation() API。目标场景是预约。

~~~

响应点对点功能性 API 的模块，是否需要单独做一个？或者写在 dp2library 模块一起?

测试 testclient1.exe 对于 dp2mserver 中途退出后重新启动的重连接功能。

~~~

管理点对点账户的功能，应该是有 supervisor 权限的人才能做。不过这个账户自身由谁来创立？在安装的时候创立？
或者特殊地写入 xml 配置文件？