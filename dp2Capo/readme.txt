
dp2Capo -- 为 dp2library 提供点对点接口，以允许 dp2 V3 项目模块访问 dp2library

2016/1/30 启动这个模块的开发。

为兼容 Windows XP 环境，dp2library 只能维持采用 .NET Framework 4.0，但由于希望在
.NET Framework 4.5 版本下写新的模块，所以只好把 dp2Capo 独立出来成为一个模块。

如果不是这个缘故，其实本来 dp2Capo 也有可能会成为 dp2library 内置的一个功能。

~~~

为了可以访问 dp2library，首先需要从 .NET 4.0 dp2 代码中把 DigitalPlatform.LibraryClient 库复制过来。

~~~

要允许 dp2capo 使用多个实例。每个实例有一个独立的数据目录，里面建立一个 capo.xml 配置文件，负责连接一个 dp2library server 和 一个 dp2mserver server

为了简化管理，这些数据目录放在一个一级目录下面。

而一级目录名配置在 settings 里面就可以了。如果按照 dp2library 等的习惯，这些信息是要配置到 registry 里面的，但 registry 方法不利于软件绿色安装。

可以开发一个对话框配置界面让控制台程序调用它。

~~~

注册 Windows Service 的时候，需要一并开辟好 Event Log 的 source