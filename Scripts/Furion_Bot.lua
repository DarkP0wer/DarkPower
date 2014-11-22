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

local levels = {2,3,2,3,2,4,2,5,3,5,4,5,5,5,5,4,5,5,5,3,1,1,1,1,5}
local purchaseStartingItems = {27, 12, 16} -- Ring of regen, Ring of Protection, branches
--===================--
--       CODE        --
--===================--
local TimeUseTree = 0
local currentLevel = 0
state = 1
local inPosition = false

function IsInPos(im,Pos)
	local x = im.position.x
	local y = im.position.y
	if im:GetAbility(3).level >= 2 and im.hero then--and im.team == 3 
		Pos.x = 3898
		Pos.y = -1196
		FarmPos = Vector(Pos.x,Pos.y,1)
	end
	local r = config.Radius
	if not im.hero then r = 120 end
	local z = ((x - Pos.x) * (x - Pos.x) + (y - Pos.y) * (y - Pos.y));
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
		TimeUseTree = 0
		return
	end
	local me = entityList:GetMyHero()
	if PlayingGame() and me.alive then
		if currentLevel == 0 then
			FarmPos = Vector(-1422,-4503,1)
			SpawnPos = Vector(-7077,-6780,1)
			
			if me.team == 2 then
				FarmPos = Vector(-1422,-4503,1)
				SpawnPos = Vector(-7077,-6780,1)
				BuyPos = Vector(-4535,1508,1)
			elseif me.team == 3 then
				FarmPos = Vector(-1422,-4503,1)
				SpawnPos = Vector(7145,6344,1)
				BuyPos = Vector(3253, 431,1)
			else print("error team = "..me.team)
			end
		end
		if me:IsChanneling() then return end
		
		if currentLevel ~= me.level then		
			local ability = me.abilities
			local prev = SelectUnit(me)
			entityList:GetMyPlayer():LearnAbility(me:GetAbility(levels[me.level]))
			SelectBack(prev)
		end
	
		if me:GetAbility(4).level >= 1 and me:GetAbility(4).state == -1 then
			if config.Ult == 1 then
				entityList:GetMyPlayer():UseAbility(me:GetAbility(4), FarmPos)
			end
		end
		
		if me.health <= config.minHealth and me:GetAbility(2).state == -1 then
			inPosition = false
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), SpawnPos)
			return
		end
		
		if me.health == me.maxHealth and inPosition == false and me:GetAbility(2).state == -1 and state >= 3 and not me:IsChanneling() then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			inPosition = true
			Sleep(500)
			return
		end
		
		inPosition = IsInPos(me,FarmPos)
		
		if me:GetAbility(3).level >= 2 and me:GetAbility(3).state == -1 and client.gameTime >= TimeUseTree and inPosition and not me:IsChanneling() then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(3), me.position)
			TimeUseTree = client.gameTime+4*60
			return
		end

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
		
		if inPosition and state >= 3 and not isAttacking(me) and not me:IsChanneling() then
			target = FindTarget()
			if target ~= nil then 
				entityList:GetMyPlayer():Attack(target)
				if me.health <= 350 and me:GetAbility(3).level >= 2 then 
					entityList:GetMyPlayer():Move(Vector(3900, -1392, 1))
				end
				Sleep(1000)
			elseif target == nil and me:GetAbility(3).level >= 2 then
				entityList:GetMyPlayer():Move(FarmPos)
			end
		end
		
		local playerEntity = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]
		local gold = playerEntity:GetGold(me.playerId)
		if config.Midas then
			if gold >= 2300 and state == 3 then
				entityList:GetMyPlayer():BuyItem(64)
				entityList:GetMyPlayer():BuyItem(25)
				Sleep(200)
				DeliverByCourier(5)
				state = 4
				return
			end
		elseif state == 3 then state = 5
		end
		
		if gold >= 1400 and state == 5 then
			entityList:GetMyPlayer():BuyItem(6)
			entityList:GetMyPlayer():BuyItem(93)
			state = 6
			DeliverByCourier(2) 
			return
		end
		
		if gold >= 1550 and state == 7 then
			entityList:GetMyPlayer():BuyItem(25)
			entityList:GetMyPlayer():BuyItem(2)
			entityList:GetMyPlayer():BuyItem(150)
			state = 8
			DeliverByCourier(2)
			return
		end
		if gold >= 810 and state == 9 then
			entityList:GetMyPlayer():BuyItem(28)
			entityList:GetMyPlayer():BuyItem(20)
			entityList:GetMyPlayer():BuyItem(14)
			entityList:GetMyPlayer():BuyItem(74)
			Sleep(200)
			DeliverByCourier(5)
			state = 10
			return
		end
		
		if gold >= 1600 and state == 11 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			DeliverByCourier(5)
			state = 12
			return
		end		
			
		if gold >= 1600 and state == 13 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			DeliverByCourier(5)
			state = 14
			return
		end
		if gold >= 900 and state == 15 then
			entityList:GetMyPlayer():BuyItem(167)
			Sleep(200)
			DeliverByCourier(5)
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

function DeliverByCourier(SkillSlot)
	local me = entityList:GetMyHero()
	local cour = entityList:FindEntities({classId = CDOTA_Unit_Courier,team = me.team,alive = true})[1]
	if cour then
		if SkillSlot == 2 then cour:CastAbility(cour:GetAbility(2))
		elseif SkillSlot == 5 then client:ExecuteCmd("dota_courier_deliver")
		end
		if cour.flying then
			if cour:GetAbility(6).state == LuaEntityAbility.STATE_READY then
				cour:CastAbility(cour:GetAbility(6))
			end
		elseif not cour.flying then
			entityList:GetMyPlayer():BuyItem(84)
		end
	end
end

function FindTarget(Tick)
	local me = entityList:GetMyHero()
	local lowenemy = nil
	local dist = 0
	local neutrals = entityList:FindEntities({classId=CDOTA_BaseNPC_Creep_Neutral,alive=true,visible=true})
	for i,v in ipairs(neutrals) do
		distance = GetDistance2D(me.position,v.position)
		if distance <= 650 and v.alive and v.visible and v.spawned then 
			if lowenemy == nil then
				lowenemy = v
			elseif (lowenemy.health) > (v.health) then
				lowenemy = v
			end
		end
	end
	return lowenemy
end

--[[function GetDistance3D(Pos1,Pos2)
	local AB
	AB = math.sqrt ((Pos2.x-Pos1.x)^2+(Pos2.y-Pos1.y)^2+(Pos2.z-Pos1.z)^2)
	return AB
end]]

--test work status of script
function Key(msg,code)
	if client.chat or client.console or client.loading then return end
	if IsKeyDown(57) then
		local me = entityList:GetMyHero()
		Tests = math.floor(client.gameTime/60)
		Tests = client.gameTime-Tests*60
		client:ExecuteCmd("say "..Tests)
		print("X="..me.position.x.."; Y="..me.position.y.."; Team="..me.team)
	end
	if IsKeyDown(config.Test) then
		local me = entityList:GetMyHero()
		client:ExecuteCmd("say state = "..state.." inPosition = "..(inPosition and 1 or 0).."TIME ="..client.gameTime)
		print("X="..client.mousePosition.x.."; Y="..client.mousePosition.y.."; Team="..me.team)
	end
end

script:RegisterEvent(EVENT_TICK,Tick)
script:RegisterEvent(EVENT_KEY,Key)
