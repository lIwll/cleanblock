local require = require;

require "lua/functions"
require "../client/Client_Data/StreamingAssets/script/i3k_gdb"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_dungeon_base"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_spawn_area"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_npc"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_mineral"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_towerDefence"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_trap_model"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_mainTask"

--神翼
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_wing"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_wing_base"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_wing_skin"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_wing_bless"
--神兵
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_weapons"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_weaponLevelUp"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_weaponStarUp"

require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_elementSoul"

require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_pet"
require "../client/Client_Data/StreamingAssets/script/gamedb/i3k_db_petUpStar"

local Joypie			= CS.Joypie;
local UCoreUtil			= Joypie.UCoreUtil;
local UDataMgr			= Joypie.UDataMgr;
local UXMLParser		= Joypie.UXMLParser;
local UXMLSerializer	= Joypie.UXMLSerializer;
local UFileReaderProxy	= Joypie.UFileReaderProxy;

local i3k_log	= Joypie.ULogger.Info;

local i3k_db_dungeon_base = i3k_db_dungeon_base;
local i3k_db_spawn_area = i3k_db_spawn_area;

local i3k_db_wing 			= i3k_db_wing
local i3k_db_wing_base 		= i3k_db_wing_base
local i3k_db_wing_skin 		= i3k_db_wing_skin
local i3k_db_wing_bless 	= i3k_db_wing_bless

local i3k_db_weapons 		= i3k_db_weapons
local i3k_db_weaponLevelUp 	= i3k_db_weaponLevelUp
local i3k_db_weaponStarUp 	= i3k_db_weaponStarUp

local i3k_db_elementSoul 	= i3k_db_elementSoul

local i3k_db_pet 			= i3k_db_pet
local i3k_db_petUpStar 		= i3k_db_petUpStar

function Run()
	local monsters = { };
	local npcs = { };
	local mines = { };
	
	local map_monsters = { };
	local map_npcs = { };
	local map_mines = { };
	local map_players = { };
	
	local teleportSpawns = {};
	local teleports = { };
	local mapTeleports = { };
	
	local zones = { };

	local traps = { };

	for k, v in pairs(i3k_db_dungeon_base) do
		i3k_log("process dungeon {0}, map id {1}", v.ID, v.MapID);
		local cNpcs = {}
		local cMonsters = {}
		local cMine = {}
		local cTeleportSpawns = {}
		local cTeleports = {}
		local cPlayers = {}
		local cZone = {}
		local cTraps = {}

		traps[v.ID] = cTraps
		npcs[v.ID] = cNpcs
		monsters[v.ID] = cMonsters
		mines[v.ID] = cMine
		teleportSpawns[v.ID] = cTeleportSpawns
		mapTeleports[v.ID] = cTeleports
		map_players[v.ID] = cPlayers
		zones[v.ID] = cZone

		local cfgPath = string.format("../client/Client_Data/StreamingAssets/data/xml/level/level_%d.xml", v.ID);

		local text = UFileReaderProxy.ReadStringFile(cfgPath);
		if text then
			local xml = UXMLParser.LoadXML(text);
			if nil ~= xml then
				local entities_n = xml:SearchForChildByTag("entities");
				if entities_n then
					UCoreUtil.Foreach(entities_n.Children, function(agent_n)
						local types = agent_n:Attribute("type"):split('.');

						local atype = types[#types]

						if atype == "SpawnPlayer" then
							local cfgIndex = tonumber(agent_n:SearchForChildByTag("ID").Text);
							local pos_n = agent_n:SearchForChildByTag("position");
							local rot_n = agent_n:SearchForChildByTag("rotation");

							if pos_n then
								local data = pos_n:SearchForChildByTag("data");
								local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
								if rot_n then
									data = rot_n:SearchForChildByTag("data");
									local rot = CS.Joypie.UMathf.UQuaternion(tonumber(data:Attribute("x")), tonumber(data:Attribute("y")), tonumber(data:Attribute("z")), tonumber(data:Attribute("w")));
									rotation = rot:ToEuler();
									rotation.x = rotation.x / 180 * math.pi
									rotation.y = rotation.y / 180 * math.pi
									rotation.z = rotation.z / 180 * math.pi
								end
								table.insert(map_players[v.ID], {pos = pos, rotation = rotation}) 
							end
						elseif atype == "SpawnMonster" then
							local cfgID = tonumber(agent_n:SearchForChildByTag("ID").Text);

							local cfg = i3k_db_spawn_area[cfgID];
							if cfg then
								local pos_n = agent_n:SearchForChildByTag("position");
								if pos_n then
									local data = pos_n:SearchForChildByTag("data");

									local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
									
									cMonsters[cfgID] = { pos = pos }
									for _, mid in pairs(cfg.Monsters) do
										if not map_monsters[mid] then
											map_monsters[mid] = { mapID = v.ID, pos = pos ,point = {[1] = cfgID}};
										else
											table.insert(map_monsters[mid].point,cfgID)
										end
									end
								end
							else
								i3k_log("warning: can not found spawn_area id " .. cfgID .. " in dungeon " .. v.ID)
							end
						elseif atype == "NPC" then
							local cfgID = tonumber(agent_n:SearchForChildByTag("ID").Text);
							local cfg = i3k_db_npc[cfgID];
							if cfg then
								local pos_n = agent_n:SearchForChildByTag("position");
								local rotation_n = agent_n:SearchForChildByTag("rotation");
								if pos_n then
									local data = pos_n:SearchForChildByTag("data");

									local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
									local rotation = {x = 0, y = 0, z = 0}
									if rotation_n then
										data = rotation_n:SearchForChildByTag("data");
										local rot = CS.Joypie.UMathf.UQuaternion(tonumber(data:Attribute("x")), tonumber(data:Attribute("y")), tonumber(data:Attribute("z")), tonumber(data:Attribute("w")));
										rotation = rot:ToEuler();
										rotation.x = rotation.x / 180 * math.pi
										rotation.y = rotation.y / 180 * math.pi
										rotation.z = rotation.z / 180 * math.pi
									end
									
									cNpcs[cfgID] = {pos = pos, rotation = rotation }
									if not map_npcs[cfgID] then
										map_npcs[cfgID] = {mapID = v.ID, pos = pos, rotation = rotation };
									end
								end
							else
								i3k_log ("warning: can not found NPC id " .. cfgID .. " in dungeon " .. v.ID)
							end
						elseif atype == "Mine" then
							local cfgID = tonumber(agent_n:SearchForChildByTag("MineID").Text);
							local pointID = tonumber(agent_n:SearchForChildByTag("PointID").Text);
							local cfg = i3k_db_mineral[cfgID];
							if cfg then
								local pos_n = agent_n:SearchForChildByTag("position");
								local rotation_n = agent_n:SearchForChildByTag("rotation");
								if pos_n then
									local data = pos_n:SearchForChildByTag("data");

									local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
									local rotation = {x = 0, y = 0, z = 0}
									if rotation_n then
										data = rotation_n:SearchForChildByTag("data");
										local rot = CS.Joypie.UMathf.UQuaternion(tonumber(data:Attribute("x")), tonumber(data:Attribute("y")), tonumber(data:Attribute("z")), tonumber(data:Attribute("w")));
										rotation = rot:ToEuler();
										rotation.x = rotation.x / 180 * math.pi
										rotation.y = rotation.y / 180 * math.pi
										rotation.z = rotation.z / 180 * math.pi
									end
									
									if not cMine[cfgID] then
										cMine[cfgID] = { id = cfgID, poses = {}}
									end
									cMine[cfgID].poses[pointID] = {pointID = pointID, pos = pos, rotation = rotation}
									if not map_mines[cfgID] then
										map_mines[cfgID] = { pointID = pointID, mapID = v.ID, pos = pos, rotation = rotation };
									end
								end
							else
								i3k_log("warning: can not found mine id " .. cfgID .. " in dungeon " .. v.ID)
							end
						elseif atype == "TeleportSpawn" then
							local id = tonumber(agent_n:SearchForChildByTag("ID").Text);
							local pos_n = agent_n:SearchForChildByTag("position");
							if pos_n then
								local data = pos_n:SearchForChildByTag("data");
								local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
								cTeleportSpawns[id] = pos
							end
						elseif atype == "Teleport" then
							local id = tonumber(agent_n:SearchForChildByTag("ID").Text);
							local pos_n = agent_n:SearchForChildByTag("position");
							if pos_n then
								local data = pos_n:SearchForChildByTag("data");
								local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
								teleports[id] = pos
								table.insert(cTeleports, id)
							end
						elseif atype == "Trap" then
							local trapID = tonumber(agent_n:SearchForChildByTag("ID").Text);
							local trapCfg = i3k_db_trap_model[trapID]
							local pos_n = agent_n:SearchForChildByTag("position");
							local rotation_n = agent_n:SearchForChildByTag("rotation");
							if pos_n then
								local data = pos_n:SearchForChildByTag("data");
								local pos = { x = data:Attribute("x"), y = data:Attribute("y"), z = data:Attribute("z") };
								local rotation = {x = 0, y = 0, z = 0}
								if rotation_n then
									data = rotation_n:SearchForChildByTag("data");
									local rot = CS.Joypie.UMathf.UQuaternion(tonumber(data:Attribute("x")), tonumber(data:Attribute("y")), tonumber(data:Attribute("z")), tonumber(data:Attribute("w")));
									rotation = rot:ToEuler();
									rotation.x = rotation.x / 180 * math.pi
									rotation.y = rotation.y / 180 * math.pi
									rotation.z = rotation.z / 180 * math.pi
								end
								local _trap = {pos = pos, rotation = rotation }
								if cTraps[trapID] then
									table.insert(cTraps[trapID],_trap)
								else
									cTraps[trapID] = {_trap}
								end
							end 
						end
					end);
				end
			end
		end
	end
	SetMainTaskKey()
	SaveMonsters(monsters)
	SaveMonsterMap(map_monsters);
	SaveNPCs(npcs)
	SaveNPCMap(map_npcs)
	SaveMines(mines)
	SaveMineMap(map_mines)
	SaveTeleport(teleportSpawns, teleports, mapTeleports)
	SavePlayerMap(map_players)
	--SaveZoneMap(zones)
	SetTowerDefendData()
	SetTraps(traps)
	SaveWingUnlockBaseData()
	SaveWeaponUnlockBaseData()
	SaveElementUnlockBaseData()
	SavePetUnlockBaseData()
end