md router_app
cd router_app

del *.* /Q
xcopy ..\..\dp2router\bin\debug\*.dll /Y

xcopy ..\..\dp2router\bin\debug\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\dp2router\bin\debug\dp2router.exe.config /Y

cd ..

..\ziputil router_app router_app.zip -t
