@echo off

IF "%1" == "" (
	set DATA_SRC=./datasrc
) ELSE (
	set DATA_SRC=%1
)

java -Xmx2048m -classpath ..\Server\lib\kutil.jar;..\Server\lib\gs.jar;..\Server\lib\poi-3.9-20121203.jar;..\Server\lib\poi-ooxml-3.9-20121203.jar;..\Server\lib\poi-ooxml-schemas-3.9-20121203.jar;..\Server\lib\log4j-1.2.16.jar;..\Server\lib\dom4j-1.6.1.jar;..\Server\lib\xmlbeans-2.3.0.jar i3k.gtool.GameDataTool --srcdir=%DATA_SRC%

echo It's %~xn0 above.

pause
