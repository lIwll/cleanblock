echo off

echo sqlite3.exe .svn/wc.db "select * from wc_lock"
echo sqlite3.exe .svn/wc.db "select * from work_queue"
sqlite3.exe .svn/wc.db "delete from wc_lock"
sqlite3.exe .svn/wc.db "delete from work_queue"
