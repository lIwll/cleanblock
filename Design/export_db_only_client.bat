@echo off
IF "%1" == "" (
	set DATA_SRC=./datasrc
) ELSE (
	set DATA_SRC=%1
)

IF "%2" == "" (
	set ADV_TYPE=eAdvType_Oversea
) ELSE (
	set ADV_TYPE=%2
)

rd /s /q ..\Client\Client_Data\StreamingAssets\script\gamedb

del ..\Client\Client_Data\StreamingAssets\script\i3k_db_type.lua

echo current_adv_type = i3k_global.enum_def.adv_type.%ADV_TYPE% >> ..\Client\Client_Data\StreamingAssets\script\i3k_db_type.lua

UExcelExporter dir:%DATA_SRC% script:../Client/Client_Data/StreamingAssets/script/gamedb res:../Config/res_build.xml dungeon:../Config/dungeon_def.xml varnamefile:%DATA_SRC%/varName.xml

ULuaApp ../client/Client_Data/StreamingAssets/ lua/main.lua 

echo It's %~xn0 above.

if NOT DEFINED CCNetBuildCondition (pause)