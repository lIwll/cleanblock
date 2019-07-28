@echo off
cd ..
svn status > svn.txt
svn info > svninfo.txt
cd Design
pylib\python\python.exe pylib\commit.py .. svn.txt svninfo.txt
cd ..
call add.bat
echo 添加完成...
call del.bat
echo 删除完成...
call revert.bat
echo 还原完成...
call commit.bat
SET ERR=%errorlevel%
del svn.txt
del svninfo.txt
del add.bat
del del.bat
del revert.bat
del commit.bat

IF DEFINED CCNetBuildCondition (
  if %ERR% == 1 (
    EXIT /B 1
  )
)