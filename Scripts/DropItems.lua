--<< This script Drop and Up any items v0.3 >>

--===================--
--     LIBRARIES     --
--===================--
require("libs.Utils")

--===================--
--      CONFIG       --
--===================--
local DOWNKEY = 56
local UPKEY = 55
local turnflag = false
local NoDrop = {"item_arcane_boots"} -- No Drop arcane boots

function DropItems(im)
	for i,v in ipairs(im.items) do	
		local bonusStrength = v:GetSpecialData("bonus_strength")
		local bonusMana = v:GetSpecialData("bonus_mana")
		local bonusHealth = v:GetSpecialData("bonus_health")
		local bonusIntellect = v:GetSpecialData("bonus_intellect")
		local bonusAll = v:GetSpecialData("bonus_all_stats")
		if bonusStrength or bonusMana or bonusHealth or bonusIntellect or bonusAll then
			for j,x in ipairs(NoDrop) do
				if v.name ~= NoDrop[j] then
					entityList:GetMyPlayer():DropItem(v,im.position)
				end
			end
		end		
	end
end

function UpItems(im)
	local DownItems = entityList:FindEntities({type=LuaEntity.TYPE_ITEM_PHYSICAL})
	for i,v in ipairs(DownItems) do
		entityList:GetMyPlayer():TakeItem(v,turnflag)
	end
end
		
function Key(msg,code)
	if client.chat or client.console or client.loading then return end
	if IsKeyDown(DOWNKEY) then
		local me = entityList:GetMyHero()
		DropItems(me)
	end
	if IsKeyDown(UPKEY) then
		local me = entityList:GetMyHero()
		UpItems(me)
	end
end

script:RegisterEvent(EVENT_KEY,Key)
