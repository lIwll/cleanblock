# -*- coding: utf-8 -*-
# @Author: Hao Ming
# @Date:   2018-06-12 15:48:58
# @Last Modified by:   Hao Ming
# @Last Modified time: 2018-06-12 16:47:47
#encoding : utf-8

import sys


if __name__ == '__main__':
	fileHandle = open(sys.path[0] + '\\commitMsgTemplate.txt',mode='r', encoding='UTF-8')
	commitMsgContent=fileHandle.read()
	print(commitMsgContent)
	fileHandle.close()
	svnMsgFile=sys.argv[2];
	svnMsgFileHandle = open(svnMsgFile,mode="w",encoding='UTF-8')
	svnMsgFileHandle.write(commitMsgContent)
	svnMsgFileHandle.close()
