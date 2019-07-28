#encoding : utf-8

import sys

#fimport traceback, multiprocessing, functools
			
def writeMoveFile(moveF, webupdateName, webSharePath, infoF):
	version = "unknow"
	with open(infoF, 'r') as svnF:
		for line in svnF:
			if line[:9] == "Revision:":
				version = line[10:].strip('\n')

	newWebShareName = webupdateName + "-" + version
	oldWebSharePath = "Game\\publish\\" + webupdateName
	with open(moveF, 'w') as f:
		f.write("move " + oldWebSharePath + " " + webSharePath + "\n")
		f.write("ren " + webSharePath + "\\" + webupdateName + " " + newWebShareName)
	

if __name__ == '__main__':
	base_path = sys.argv[1]
	webupdateName = sys.argv[2]
	webSharePath = sys.argv[3]
	svnInfoFile = sys.argv[4]
	
	base_path = base_path.rstrip("\\")
	base_path = base_path.rstrip("/")
	
	writeMoveFile(base_path + "\\moveWebshare.bat", webupdateName, webSharePath, base_path + "\\" + svnInfoFile)
