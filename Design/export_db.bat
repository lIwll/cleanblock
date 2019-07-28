rd /s /q ..\Client\Client_Data\StreamingAssets\script\gamedb

del ..\Client\Client_Data\StreamingAssets\script\i3k_db_type.lua

echo current_adv_type = i3k_global.enum_def.adv_type.eAdvType_Oversea >> ..\Client\Client_Data\StreamingAssets\script\i3k_db_type.lua

UExcelExporter dir:./datasrc script:../Client/Client_Data/StreamingAssets/script/gamedb res:../Config/res_build.xml dungeon:../Config/dungeon_def.xml

ULuaApp ../client/Client_Data/StreamingAssets/ lua/main.lua 

export_db_only_server.bat
