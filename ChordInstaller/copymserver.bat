md mserver_app
cd mserver_app

del *.* /Q
xcopy ..\..\dp2mserver\bin\debug\*.dll /Y

xcopy ..\..\dp2mserver\bin\debug\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\dp2mserver\bin\debug\dp2mserver.exe.config /Y

cd ..

..\ziputil mserver_app mserver_app.zip -t
