--<< Bot on Furion beta version >>
require("libs.Utils")
require("libs.ScriptConfig")

LVL = 0
state = 1
inpos = false
local config = ScriptConfig.new()
config:SetParameter("Test", "L", config.TYPE_HOTKEY)
config:SetParameter("minHealth", 150)
config:SetParameter("Radius", 200)
config:SetParameter("Midas", true)
config:Load()

local Hotkey = config.Test
local minHealth = config.minHealth
local BuyMidas = config.Midas
levels = {2,5,2,5,2,4,2,5,5,5,4,5,5,3,5,4,5,3,3,3,1,1,1,1,5}
function InRangeX_Y(im)
	x = im.position.x
	y = im.position.y
	local me = entityList:GetMyHero()
	if me.team == 2 then
	Xc = -1422
	Yc = -4503
    else
	--Xc = -1294
	--Yc = 2356
	Xc = -1422
	Yc = -4503
	end
	r = config.Radius
	if (((x - Xc) * (x - Xc) + (y - Yc) * (y - Yc)) < r * r) then
		return true
	elseif (((x - Xc) * (x - Xc) + (y - Yc) * (y - Yc)) == r * r) then
		return true
	else
		return false
	end
end
--1243.15234375; Y=2308.2580566406;
function StartBuy(im)
	level = im.level
	if level == 1 then
		entityList:GetMyPlayer():BuyItem(182)
		entityList:GetMyPlayer():BuyItem(182)
		entityList:GetMyPlayer():BuyItem(16)
		entityList:GetMyPlayer():BuyItem(16)
		--entityList:GetMyPlayer():BuyItem(27)
		--entityList:GetMyPlayer():BuyItem(16)
		--entityList:GetMyPlayer():BuyItem(93)
	end
	state = 2
end

function Tick( tick )
	if client.loading then return end
	if not SleepCheck() then return end Sleep(200)
	if client.gameState == Client.STATE_PICK then 
		client:ExecuteCmd("dota_select_hero npc_dota_hero_furion")
		LVL = 0
		state = 1
		return
	end
	local me = entityList:GetMyHero()
	if PlayingGame() and me.alive then
		if LVL == 0 then
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
	
		if me:GetAbility(4).level >= 1 and me:GetAbility(4).state == -1 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(4), FarmPos)
			return
		end
		
		if me.health <= minHealth and me:GetAbility(2).state == -1 then
			inpos = false
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), SpawnPos)
			return
		end
		
		if me.health == me.maxHealth and inpos == false and me:GetAbility(2).state == -1 and state >= 3 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			inpos = true
			return
		end
		
		local player = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]
		
		if state >= 4 and BuyMidas then
			local midas = me:FindItem("item_hand_of_midas")
			if midas ~= nil then
				if   midas:CanBeCasted() and me:CanUseItems() then 
					target = FindTarget()
					if target ~= nil then me:CastAbility(midas,target) end
					return
				end
			end
		end
		if InRangeX_Y(me) then 
			inpos = true
		else 
			inpos = false
		end
		
		if inpos and state >= 3 then
			target = FindTarget()
			if target ~= nil then entityList:GetMyPlayer():Attack(target) end
		end
		
		if LVL ~= me.level then		
			local ability = me.abilities
			local prev = SelectUnit(me)
			entityList:GetMyPlayer():LearnAbility(me:GetAbility(levels[me.level]))
			SelectBack(prev)
		end
		
		local gold = player:GetGold(me.playerId)
		if BuyMidas then
			if gold >= 2300 and state == 3 then
				entityList:GetMyPlayer():BuyItem(64)
				entityList:GetMyPlayer():BuyItem(25)
				Sleep(200)
				CurDeliver()
				state = 4
				return
			end
		else
			if gold >= 2375 and state == 3 then
				entityList:GetMyPlayer():BuyItem(3)
				entityList:GetMyPlayer():BuyItem(2)
				entityList:GetMyPlayer():BuyItem(148)
				Sleep(200)
				CurDeliver()
				state = 4
				return
			end
		end
		if gold >= 1600 and state == 7 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			CurDeliver()
			state = 8
			return
		end
		if gold >= 900 and state == 9 then
			entityList:GetMyPlayer():BuyItem(167)
			Sleep(200)
			CurDeliver()
			state = 10
			return
		end
		if gold >= 1600 and state == 5 then
			entityList:GetMyPlayer():BuyItem(8)
			Sleep(200)
			CurDeliver()
			state = 6
			return
		end
		
		if state >= 4 and state % 2 == 0 then 
			client:ExecuteCmd("dota_courier_deliver")
			client:ExecuteCmd("dota_courier_burst")
			client:ExecuteCmd("dota_courier_deliver")			
			state = state + 1 
		end
		LVL = me.level
		
		if state == 1 then
			StartBuy(me)
			client:ExecuteCmd("dota_player_units_auto_attack 1")
		end
		
		if state == 2 and me:GetAbility(2).state == -1  then
			if inpos == false then
				entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			end
			state = 3
		end
	end
end

function CurDeliver()
	local me = entityList:GetMyHero()
	local Kyras = entityList:FindEntities({type=LuaEntity.TYPE_NPC,alive=true,visible=true,team = me.team})
	for i,v in ipairs(Kyras) do
		if kyra == nil then
			kyra = v
		end
	end
	client:ExecuteCmd("dota_courier_deliver")
	if kyra:IsFlying() then
		client:ExecuteCmd("dota_courier_burst")
	elseif state == 3 or state == 4 then
		entityList:GetMyPlayer():BuyItem(84)
	end
end

function FindTarget(Tick)
	local me = entityList:GetMyHero()
	local lowenemy = nil
	local neutrals = entityList:FindEntities({classId=CDOTA_BaseNPC_Creep_Neutral,alive=true,visible=true})
	for i,v in ipairs(neutrals) do
		distance = GetDistance2D(me,v)
		if distance <= 800 and v.alive and v.visible and v.spawned then 
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
	local me = entityList:GetMyHero()
	if IsKeyDown(Hotkey) then
		client:ExecuteCmd("say state = "..state.." inpos = "..(inpos and 1 or 0))
		print("X="..client.mousePosition.x.."; Y="..client.mousePosition.y.."; Team="..me.team)
	end
end

script:RegisterEvent(EVENT_TICK,Tick)
script:RegisterEvent(EVENT_KEY,Key)
