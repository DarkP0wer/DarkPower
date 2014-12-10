--<< This script Drop and Up any items v0.2 >>

--===================--
--     LIBRARIES     --
--===================--
require("libs.Utils")
require("libs.ScriptConfig")
--===================--
--      CONFIG       --
--===================--
local config = ScriptConfig.new()
config:SetParameter("DOWNKEY", "56", config.TYPE_HOTKEY)
config:SetParameter("UPKEY", "55", config.TYPE_HOTKEY)
config:SetParameter("turnflag", false)
config:Load()

local DropItems = {"item_wraith_band","item_veil_of_discord","item_ultimate_orb","item_staff_of_wizardry","item_soul_booster","item_shivas_guard","item_sheepstick",
"item_rod_of_atos","item_robe","item_ring_of_aquila","item_point_booster","item_orchid","item_null_talisman","item_necronomicon_3","item_necronomicon_2","item_necronomicon",
"item_mystic_staff","item_mekansm","item_mantle","item_manta","item_magic_wand","item_sphere","item_branches","item_headdress","item_ghost","item_force_staff",
"item_skadi","item_cyclone","item_ethereal_blade","item_energy_booster","item_ancient_janggo","item_dagon_5","item_dagon_4","item_dagon_3","item_dagon_2","item_dagon",
"item_circlet","item_buckler","item_bracer","item_blade_mail","item_ultimate_scepter"}

function DropItem(im)
	for i, items in ipairs(DropItems) do
		local IsItem = im:FindItem(DropItems[i])
		if IsItem ~= nil then
			entityList:GetMyPlayer():DropItem(IsItem,im.position,config.turnflag)	
		end
	end
end

function UpItems(im)
	local DownItems = entityList:FindEntities({type=LuaEntity.TYPE_ITEM_PHYSICAL})
	for i,v in ipairs(DownItems) do
		entityList:GetMyPlayer():TakeItem(v,config.turnflag)
	end
end
		
function Key(msg,code)
	if client.chat or client.console or client.loading then return end
	if IsKeyDown(config.DOWNKEY) then
		local me = entityList:GetMyHero()
		DropItem(me)
	end
	if IsKeyDown(config.DOWNKEY) then
		local me = entityList:GetMyHero()
		UpItems(me)
	end
end

script:RegisterEvent(EVENT_KEY,Key)
