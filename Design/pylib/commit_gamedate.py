import sys
import zipfile

def zipdata():
    svnInfoFile = sys.argv[1]
    version = 'null'
    with open(svnInfoFile, 'r') as svnF:
        for line in svnF:
            if line[:9] == "Revision:":
                version = line[10:].strip('\n')

    path = '\\\\192.168.1.150\\共享文件夹\\ModMud引擎组共享\\uengine\\打包机数据\\gamedata_' + version + '.zip'
    zf = zipfile.ZipFile(path, "w",zipfile.ZIP_DEFLATED)
    zf.write('F:/ns_android/Server/bin/gamedata.dat','gamedata.dat')
    zf.close()
    print('[Log]: gamedata 已经成功压缩到150网盘')

if __name__ == '__main__':
    zipdata()