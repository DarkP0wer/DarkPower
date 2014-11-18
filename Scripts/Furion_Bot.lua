--<< Automatically play Nature's Prophet (beta) >>

--===================--
--     LIBRARIES     --
--===================--
require("libs.Utils")
require("libs.ScriptConfig")

--===================--
--      CONFIG       --
--===================--
local config = ScriptConfig.new()
config:SetParameter("Test", "L", config.TYPE_HOTKEY)
config:SetParameter("minHealth", 150)
config:SetParameter("Radius", 200)
config:SetParameter("Midas", true)
config:SetParameter("Ult", 1) -- 1 = CD; 2 = still; 3 = none
config:Load()
local currentLevel = 0
local state = 1
local inPosition = false
local levels = {2,5,2,5,2,4,2,5,5,5,4,5,5,3,5,4,5,3,3,3,1,1,1,1,5}
local purchaseStartingItems = {27, 16, 16, 16, 16} -- Ring of regen, 4x iron branches
--===================--
--       CODE        --
--===================--
function InRangeX_Y(im)
	local x = im.position.x
	local y = im.position.y
	local Xc = -1422
	local Yc = -4503
	if im.team == 2 then
		local Xc = -1422
		local Yc = -4503
    else
	--Xc = -1294
	--Yc = 2356
		local Xc = -1422
		local Yc = -4503
	end
	local r = config.Radius
	local z = ((x - Xc) * (x - Xc) + (y - Yc) * (y - Yc));
	return (z < r * r) or (z == r * r)
end

function StartBuy(im)
	level = im.level
	if level == 1 then
		for i, itemID in ipairs(purchaseStartingItems) do
			entityList:GetMyPlayer():BuyItem(itemID)
		end
	end
	state = 2
end

function Tick( tick )
	if client.loading then return end
	if not SleepCheck() then return end Sleep(200)
	if client.gameState == Client.STATE_PICK then 
		client:ExecuteCmd("dota_select_hero npc_dota_hero_furion")
		currentLevel = 0
		state = 1
		return
	end
	local me = entityList:GetMyHero()
	if PlayingGame() and me.alive then
		if currentLevel == 0 then
			FarmPos = Vector(-1422,-4503,496)
			SpawnPos = Vector(-7077,-6780,496)
			
			if me.team == 2 then
				FarmPos = Vector(-1422,-4503,496)
				SpawnPos = Vector(-7077,-6780,496)
			elseif me.team == 3 then
				--FarmPos = Vector(-1294,2356,496)
				FarmPos = Vector(-1422,-4503,496)
				SpawnPos = Vector(7145,6344,496)
			else print("error team = "..me.team)
			end
		end
		
		if currentLevel ~= me.level then		
			local ability = me.abilities
			local prev = SelectUnit(me)
			entityList:GetMyPlayer():LearnAbility(me:GetAbility(levels[me.level]))
			SelectBack(prev)
		end
	
		if me:GetAbility(4).level >= 1 and me:GetAbility(4).state == -1 and config.Ult == 1 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(4), FarmPos)
			return
		end
		
		if me.health <= config.minHealth and me:GetAbility(2).state == -1 then
			inPosition = false
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), SpawnPos)
			return
		end
		
		if me.health == me.maxHealth and inPosition == false and me:GetAbility(2).state == -1 and state >= 3 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			inPosition = true
			Sleep(500)
			return
		end
		
		inPosition = InRangeX_Y(me)

		if state >= 4 and config.Midas then
			local midas = me:FindItem("item_hand_of_midas")
			if midas ~= nil then
				if   midas:CanBeCasted() and me:CanUseItems() then 
					target = FindTarget()
					if target ~= nil then me:CastAbility(midas,target) end
					return
				end
			end
		end
		
		if inPosition and state >= 3 and not isAttacking(me) then
			target = FindTarget()
			if target ~= nil then 
				entityList:GetMyPlayer():Attack(target)
				Sleep(1000)
			end
		end
		
		local playerEntity = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]
		local gold = playerEntity:GetGold(me.playerId)
		if config.Midas then
			if gold >= 2300 and state == 3 then
				entityList:GetMyPlayer():BuyItem(64)
				entityList:GetMyPlayer():BuyItem(25)
				Sleep(200)
				DeliverByCourier()
				state = 4
				return
			end
		elseif state == 3 then state = 5
		end
		
		if gold >= 1620 and state == 5 then
				entityList:GetMyPlayer():BuyItem(3)
				entityList:GetMyPlayer():BuyItem(93)
				Sleep(200)
				DeliverByCourier()
				state = 6
				return
			end
		
		if gold >= 800 and state == 7 then
			entityList:GetMyPlayer():BuyItem(2)
			entityList:GetMyPlayer():BuyItem(34)
			entityList:GetMyPlayer():BuyItem(35)
			Sleep(200)
			DeliverByCourier()
			state = 8
			return
		end
		if gold >= 500 and state == 9 then
			entityList:GetMyPlayer():BuyItem(148)
			Sleep(200)
			DeliverByCourier()
			state = 10
			return
		end
		
		if gold >= 1600 and state == 11 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			DeliverByCourier()
			state = 12
			return
		end		
			
		if gold >= 1600 and state == 13 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			DeliverByCourier()
			state = 14
			return
		end
		if gold >= 900 and state == 15 then
			entityList:GetMyPlayer():BuyItem(167)
			Sleep(200)
			DeliverByCourier()
			state = 16
			return
		end
		
		if state >= 4 and state % 2 == 0 then 
			client:ExecuteCmd("dota_courier_deliver")
			client:ExecuteCmd("dota_courier_burst")
			client:ExecuteCmd("dota_courier_deliver")			
			state = state + 1 
		end
		currentLevel = me.level
		
		if state == 1 then
			StartBuy(me)
			client:ExecuteCmd("dota_player_units_auto_attack 1")
		end
		
		if state == 2 and me:GetAbility(2).state == -1  then
			if inPosition == false then
				entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			end
			state = 3
		end
	end
end

function isAttacking(ent)
	return ent.activity == LuaEntityNPC.ACTIVITY_ATTACK or ent.activity == LuaEntityNPC.ACTIVITY_ATTACK1 or ent.activity == LuaEntityNPC.ACTIVITY_ATTACK2
end

function DeliverByCourier()
	local me = entityList:GetMyHero()
	local cour = entityList:FindEntities({classId = CDOTA_Unit_Courier,team = me.team,alive = true})[1]
	if cour then
		client:ExecuteCmd("dota_courier_deliver")
		if cour.flying and cour.alive then
			client:ExecuteCmd("dota_courier_burst")
		elseif not cour.flying then
			entityList:GetMyPlayer():BuyItem(84)
		end
	end
end

function FindTarget(Tick)
	local me = entityList:GetMyHero()
	local lowenemy = nil
	local neutrals = entityList:FindEntities({classId=CDOTA_BaseNPC_Creep_Neutral,alive=true,visible=true})
	for i,v in ipairs(neutrals) do
		distance = GetDistance2D(me,v)
		if distance <= 600 and v.alive and v.visible and v.spawned then 
			if lowenemy == nil then
				lowenemy = v
			elseif (lowenemy.health) > (v.health) then
				lowenemy = v
			end
		end
	end
	return lowenemy
end

--test work status of script
function Key(msg,code)
	if client.chat or client.console or client.loading then return end
	if IsKeyDown(config.Test) then
		local me = entityList:GetMyHero()
		client:ExecuteCmd("say state = "..state.." inPosition = "..(inPosition and 1 or 0).."TIME ="..client.gameTime)
		print("X="..client.mousePosition.x.."; Y="..client.mousePosition.y.."; Team="..me.team)
	end
end

script:RegisterEvent(EVENT_TICK,Tick)
script:RegisterEvent(EVENT_KEY,Key)
