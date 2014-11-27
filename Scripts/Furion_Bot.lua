--<< Automatically play Nature's Prophet v0.2 >>

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
config:SetParameter("Midas", false) --deleted
config:SetParameter("MaxNotFindTarget", 3)
config:SetParameter("Ult", 1) -- 1 = CD; 2 = none
config:Load()
local levels = {2,3,2,5,2,4,2,5,3,5,4,5,5,5,5,4,5,5,3,3,1,1,1,1,5}
local purchaseStartingItems = {27, 45, 16} -- Ring of regen, courier, branches
--===================--
--       CODE        --
--===================--
local NotFindTarget = 0
local Minuta = 0
local TimeUseTree = 0
local currentLevel = 0
local state = 1
local inPosition = false
local BuyItem1 = {6,12,93,11} -- First buy
local BuyItem2 = {25,2,150}
local BuyItem3 = {28,20,14,74}
local BuyItem4 = {8}
local BuyItem5 = {8}
local BuyItem6 = {167}
local StepsOfBuy={BuyItem1,BuyItem2,BuyItem3,BuyItem4,BuyItem5,BuyItem6}
local StepOfPrice = {1825,1550,810,1600,1600,900}

function BuyItems(im)
	if state >= 5 then
		local playerEntity = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]
		local gold = playerEntity:GetGold(im.playerId)
		for i, Step in ipairs(StepsOfBuy) do
			if gold >= StepOfPrice[i] and state == i*2+3 then 
				for j, itemID in ipairs(Step) do
					entityList:GetMyPlayer():BuyItem(itemID)
				end
				Sleep(200)
				DeliverByCourier(5)
				state = state+1
			end
		end
	end
end
		
function IsInPos(im,Pos)
	local x = im.position.x
	local y = im.position.y
	if im:GetAbility(3).level >= 1 and im.hero then--and im.team == 3 
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
		NotFindTarget = 0
		Minuta = 0
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
		
		local kyra = me:FindItem("item_courier")
		if kyra ~= nil then
				me:CastAbility(kyra)
				--print("Кура есть")
		end
		
		local courier = entityList:FindEntities({classId = CDOTA_Unit_Courier,team = me.team,alive = true})[1]
		if courier then
			if courier.courState ~= LuaEntityCourier.STATE_DELIVER then  
				if DoIHaveItemsInStash() or MyItemsInCurier(me,courier) then client:ExecuteCmd("dota_courier_deliver") end
			end
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
		
		if me.health >= me.maxHealth-100 and inPosition == false and me:GetAbility(2).state == -1 and state >= 3 and not me:IsChanneling() then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			inPosition = true
			Sleep(500)
			return
		end
		
		inPosition = IsInPos(me,FarmPos)
		
		if me:GetAbility(3).level >= 1 and me:GetAbility(3).state == -1 and client.gameTime >= TimeUseTree and inPosition and not me:IsChanneling() then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(3), me.position)
			TimeUseTree = client.gameTime+4*60
		end

		--[[if state >= 4 and config.Midas then
			local midas = me:FindItem("item_hand_of_midas")
			if midas ~= nil then
				if   midas:CanBeCasted() and me:CanUseItems() then 
					target = FindTarget()
					if target ~= nil then me:CastAbility(midas,target) end
					return
				end
			end
		end]]
		if inPosition and state >= 3 and not isAttacking(me) and not me:IsChanneling() then
			target = FindTarget()
			if target ~= nil then 
				entityList:GetMyPlayer():Attack(target)
				if me.health <= 400 and me:GetAbility(3).level >= 1 then 
					entityList:GetMyPlayer():Move(Vector(3900, -1392, 1))
				end
				Sleep(600)
				MaxNotFindTarget = 0
			else
				entityList:GetMyPlayer():Move(FarmPos)
				if GetSeconds() >=5 and GetSeconds() <=10 and GetMinuts() ~= Minuta then
					Minuta = GetMinuts()
					if (NotFindTarget == 0) then
						if me:GetAbility(3).level >= 1 and me:GetAbility(3).state == -1 and not me:IsChanneling() then
							entityList:GetMyPlayer():UseAbility(me:GetAbility(3), me.position)
							TimeUseTree = client.gameTime+4*60
						end
					end
					NotFindTarget = NotFindTarget+1
					if (NotFindTarget == config.MaxNotFindTarget) and state >= 3 then
						--ChangeFarmPos(FarmPos)
						print("block spawn neutrals")
					end
				end
			end
		end
		
		local playerEntity = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]
		local gold = playerEntity:GetGold(me.playerId)
		if config.Midas then
			--[[if gold >= 2300 and state == 3 then
				entityList:GetMyPlayer():BuyItem(64)
				entityList:GetMyPlayer():BuyItem(25)
				Sleep(200)
				DeliverByCourier(5)
				state = 4
				return
			end]]
		elseif state == 3 then state = 5
		end
		
		BuyItems(me)
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
			Sleep(500)
			if inPosition == false then
				entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			end
			state = 3
		end
	end
end

function DoIHaveItemsInStash() 
	for i = 7, 12 do
		if entityList:GetMyHero():HasItem(i) then
			return true
		end
	end
	return false	
end

function MyItemsInCurier(im,cur) 
	for i = 1, 6 do
        local item = cur:GetItem(i)
		if item then
			s = item.owner
			if item.owner == im then
				print("Есть")
				return true
			--else print("Name = "..s.name.."; item = "..item.name)
			end
		end
    end
    return false
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

function GetSeconds() 
	return math.floor(client.gameTime-math.floor(client.gameTime/60)*60)
end

function GetMinuts() 
	return math.floor(client.gameTime/60)
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
