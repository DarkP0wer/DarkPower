--<< This script Drop any items v0.1 >>

--===================--
--     LIBRARIES     --
--===================--
require("libs.Utils")

--===================--
--      CONFIG       --
--===================--
local HOTKEY = 56

local manaitems = {"item_wraith_band","item_veil_of_discord","item_ultimate_orb","item_staff_of_wizardry","item_soul_booster","item_shivas_guard","item_sheepstick",
"item_rod_of_atos","item_robe","item_ring_of_aquila","item_point_booster","item_orchid","item_null_talisman","item_necronomicon_3","item_necronomicon_2","item_necronomicon",
"item_mystic_staff","item_mekansm","item_mantle","item_manta","item_magic_wand","item_sphere","item_branches","item_headdress","item_ghost","item_force_staff",
"item_skadi","item_cyclone","item_ethereal_blade","item_energy_booster","item_ancient_janggo","item_dagon_5","item_dagon_4","item_dagon_3","item_dagon_2","item_dagon",
"item_circlet","item_buckler","item_bracer","item_blade_mail","item_ultimate_scepter"}

function DropItems(im)
	for i, items in ipairs(manaitems) do
		local IsItem = im:FindItem(manaitems[i])
		if IsItem ~= nil then
			entityList:GetMyPlayer():DropItem(IsItem,im.position)	
		end
	end
end
		
function Key(msg,code)
	if client.chat or client.console or client.loading then return end
	if IsKeyDown(HOTKEY) then
		local me = entityList:GetMyHero()
		DropItems(me)
	end
end

script:RegisterEvent(EVENT_KEY,Key)
