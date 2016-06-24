md capo_app
cd capo_app

del *.* /Q
xcopy ..\..\dp2capo\bin\debug\*.dll /Y

xcopy ..\..\dp2capo\bin\debug\*.exe /Y

del *.vshost.exe /Q

xcopy ..\..\dp2capo\bin\debug\dp2capo.exe.config /Y

cd ..

..\ziputil capo_app capo_app.zip -t
