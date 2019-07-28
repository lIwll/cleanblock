#encoding : utf-8

#import os
import sys
#import shutil
#import time

#fimport traceback, multiprocessing, functools

def writeFile(addF, addList, delF, delList, revertF, revertList):
	with open(addF, 'w') as f:
		for file in addList:
			f.write("svn add -q \"" + file +"@\"\n")
	with open(delF, 'w') as f:
		for file in delList:
			f.write("svn del -q \"" + file +"@\"\n")
	with open(revertF, 'w') as f:
		for file in revertList:
			f.write("svn revert -q \"" + file +"@\"\n")

def parserLine(line, addList, delList, revertList):
	type = line[0]
	path = line[8:]
	pathPrx = path[:6].lower()
	if pathPrx == "client" or pathPrx == "editor":
		if type == "!":
			delList.append(path)
		elif type == "?":
			addList.append(path)
		elif type == "M" and pathPrx == "editor" and path[-5:].lower() == "unity" and path.rfind('\\') == 20 and path[:20].lower() == "editor\\assets\\levels":
			revertList.append(path)
			
def writeCommitFile(commitF, infoF):
	version = "unknow"
	with open(infoF, 'r') as svnF:
		for line in svnF:
			if line[:9] == "Revision:":
				version = line.strip('\n')
			
	with open(commitF, 'w') as f:
		f.write("svn commit Client Editor -m \"-auto exported: " + version + "\"\n")
		f.write("if %errorlevel% == 1 (\n")
		f.write("IF DEFINED CCNetBuildCondition (\n")
		f.write("EXIT /B 1\n")
		f.write(")\n")
		f.write("	pause\n")
		f.write(")\n")
		f.write("EXIT /B 0\n")
	

if __name__ == '__main__':
	base_path = sys.argv[1]
	svnFile = sys.argv[2]
	svnInfoFile = sys.argv[3]

	base_path = base_path.rstrip("\\")
	base_path = base_path.rstrip("/")
	
	addList = list()
	delList = list()
	revertList = list()

	print('开始准备svn数据... ...')
	
	with open(base_path + '\\' + svnFile, 'r') as svnF:
		for line in svnF:
			parserLine(line.strip('\n'), addList, delList, revertList)
	
	writeFile(base_path + "\\add.bat", addList, base_path + "\\del.bat", delList, base_path + "\\revert.bat", revertList)
	
	print('准备svn数据完成')
	
	writeCommitFile(base_path + "\\commit.bat", base_path + "\\" + svnInfoFile)
