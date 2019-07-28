#encoding : utf-8

import os
import sys
import datetime
#import shutil
#import time

#fimport traceback, multiprocessing, functools
assertDic = dict()

def paserAssertFile(date, resVersion, beginInnerVersion, endInnerVersion):
	global assertDic
	currentDir = os.getcwd() + "/" + date
	if not os.path.isdir(currentDir):
		return
	listfile=os.listdir(currentDir)
	currentTag = ""
	currentError = None
	needRead = False
	key = ""
	currentDict = None
	for filename in listfile:
		with open(currentDir + "/" + filename, 'r') as file:
			line = file.readline()
			while line:
				if line.startswith("-=-=-UTC"):
					needWrite = True
					if (currentDict != None) and (currentError != None):
						currentDict["error"] = currentError
					currentTag = file.readline().rstrip()
					tagInfos = currentTag.split('|')
					if (beginInnerVersion > 0) and (int(tagInfos[-1]) < beginInnerVersion):
						needWrite = False
					if (endInnerVersion > 0) and (int(tagInfos[-1]) > endInnerVersion):
						needWrite = False
					if (resVersion > 0) and tagInfos[-2].isdigit() and (int(tagInfos[-2]) != 0) and (int(tagInfos[-2]) != resVersion):
						needWrite = False
					currentTag = currentTag + line
					line = file.readline().strip('\n')
					key = line[18:]
					if needWrite:
						if key in assertDic:
							needRead = False
							currentError = None
							currentDict = None
							assertDic[key]["tags"] = assertDic[key]["tags"] + currentTag
							assertDic[key]["count"] += 1
						else:
							currentDict = dict()
							needRead = True
							currentError = ""
							assertDic[key] = currentDict
							currentDict["tags"] = currentTag
							currentDict["count"] = 1
					else:
						needRead = False
						currentError = None
						currentDict = None
				line = file.readline()
				if needRead:
					currentError = currentError + line
			if (currentDict != None) and (currentError != None):
				currentDict["error"] = currentError


def writeOutputFile():
	global assertDic
	toSortedDic = dict()
	for key in assertDic:
		toSortedDic[key] = assertDic[key]["count"]
	sortedKeys = sorted(toSortedDic.items(), key=lambda item:item[1], reverse=True)
	with open(os.getcwd() + "/" + "assert.txt", 'w') as f:
		for item in sortedKeys:
			f.write(item[0] + "  :" + str(item[1]) + "\n")
		for item in sortedKeys:
			f.write(item[0] + "\n")
			f.write(assertDic[item[0]]["error"])
			f.write("\n-----\n")
			f.write(assertDic[item[0]]["tags"])
			f.write("\n===============================\n")

if __name__ == '__main__':
	beginDateStr = sys.argv[1]
	endDateStr = sys.argv[2]
	resVersion = 0
	beginInnerVersion = 0
	endInnerVersion = 0
	if len(sys.argv) >= 3:
		resVersion = int(sys.argv[3])
		if len(sys.argv) >= 5:
			beginInnerVersion = int(sys.argv[4])
			endInnerVersion = int(sys.argv[5])
	beginDate = datetime.datetime.strptime(beginDateStr, "%Y-%m-%d")
	endDate = datetime.datetime.strptime(endDateStr, "%Y-%m-%d")
	
	currentDay = 0
	endDay = (endDate - beginDate).days
	while currentDay <= endDay:
		paserAssertFile((beginDate + datetime.timedelta(currentDay)).strftime("%Y-%m-%d"), resVersion, beginInnerVersion, endInnerVersion)
		currentDay += 1
	
	writeOutputFile()
	