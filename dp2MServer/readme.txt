
*** 安装 ***
1) 确保机器上安装了 .NET 4.5 环境；
2) 将所有文件拷贝到一个子目录，例如 c:\program files\digitalplatform\dp2mserver；
3) 用 Windows 命令提示符进入这个子目录，执行 dp2mserver install。这一步注册 Windows Service
4) 用 Windows “服务”观察 dp2 message server 是否正常启动了。

*** 升级 ***
1) 用 Windows 命令提示符进入安装了 dp2mserver 的子目录，执行 dp2mserver uninstall。这一步是注销 Windows Service
2) 将更新的文件拷贝到这个子目录。注意不要拷入 dp2mserver.exe.config 这个文件，因为这个文件存储了一些配置信息，可能相对于安装文件已经修改过了。如果覆盖了这个文件，可能要重新进行配置
3) 执行 dp2mserver install。这一步注册 Windows Service
4) 用 Windows “服务”观察 dp2 message server 是否正常启动了。

*** 配置 ***
可以修改 dp2mserver 的监听主机名和服务器路径两项参数。
用 dp2mserver setting 命令，按照提示修改即可。如果在要求输入某项值的时候直接输入回车，表示此项值不做修改。

http://*:8083 这个值中，星号表示监听这台机器的所有域名和 IP。如果只希望监听某个域名，可以这样配置 http://dp2003.com:8083

*** 管理员身份 ***
dp2mserver 会自动要求以管理员身份运行。为了避免每次都点 UAC 按钮，可以在启动 Windows 命令提示符的时候用“以管理员身份运行”。


---
将 SignalR 的 Group 设置等同为用户管理角度的 Group，这样分发消息就方便了。不然就要用 id 列表来发送了

---
用户获取自己能操作的群组列表。
API 要设计成可以获取全部群组，也可以获取某个用户能操作的群组。GetGroup()
还要一个 SetGroup() 可以创建和修改群组
管理界面如何设计? 可以用一个特殊的群组来表示对话?

---
绑定 SSL 证书
netsh http add sslcert ipport=0.0.0.0:8083 certhash=872801bbd93e75f327b34ae76366fa05b229433b appid={ca84fb79-db4f-4e06-b833-1603913c091e}

