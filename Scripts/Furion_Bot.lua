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

--===================--
--       CODE        --
--===================--
local Hotkey = config.Test
local minHealth = config.minHealth
local buyMidas = config.Midas
local useUlt = config.Ult
local levels = {2,5,2,5,2,4,2,5,5,5,4,5,5,3,5,4,5,3,3,3,1,1,1,1,5}

function InRangeX_Y(im)
	local x = im.position.x
	local y = im.position.y
	local me = entityList:GetMyHero()

	if me.team == 2 then
		local Xc = -1422
		local Yc = -4503
    else
		local Xc = -1422
		local Yc = -4503
	end

	local r = config.Radius
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
		--[[entityList:GetMyPlayer():BuyItem(182)
		entityList:GetMyPlayer():BuyItem(182)
		entityList:GetMyPlayer():BuyItem(16)]]
		entityList:GetMyPlayer():BuyItem(27) -- item_ring_of_regen
		entityList:GetMyPlayer():BuyItem(16) -- item_branches
		entityList:GetMyPlayer():BuyItem(16) -- item_branches
		entityList:GetMyPlayer():BuyItem(16) -- item_branches
		entityList:GetMyPlayer():BuyItem(16) -- item_branches
		--entityList:GetMyPlayer():BuyItem(16)
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

		if me:GetAbility(4).level >= 1 and me:GetAbility(4).state == -1 and useUlt == 1 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(4), FarmPos)
			return
		end

		if me.health <= minHealth and me:GetAbility(2).state == -1 then
			inPosition = false
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), SpawnPos)
			return
		end

		if me.health == me.maxHealth and inPosition == false and me:GetAbility(2).state == -1 and state >= 3 then
			entityList:GetMyPlayer():UseAbility(me:GetAbility(2), FarmPos)
			inPosition = true
			return
		end

		local player = entityList:GetEntities({classId=CDOTA_PlayerResource})[1]

		if state >= 4 and buyMidas then
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
			inPosition = true
		else
			inPosition = false
		end

		if inPosition and state >= 3 then
			target = FindTarget()
			if target ~= nil then entityList:GetMyPlayer():Attack(target) end
		end

		if currentLevel ~= me.level then
			local ability = me.abilities
			local prev = SelectUnit(me)
			entityList:GetMyPlayer():LearnAbility(me:GetAbility(levels[me.level]))
			SelectBack(prev)
		end

		local gold = player:GetGold(me.playerId)
		if buyMidas then
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

function DeliverByCourier()
	local me = entityList:GetMyHero()
	local Kyras = entityList:FindEntities({type=LuaEntity.TYPE_NPC,alive=true,visible=true,team = me.team})
	for i,v in ipairs(Kyras) do
		if kyra == nil then
			kyra = v
		end
	end
	client:ExecuteCmd("dota_courier_deliver")
	if kyra.flying then --IsFlying()
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
	local me = entityList:GetMyHero()
	if IsKeyDown(Hotkey) then
		client:ExecuteCmd("say state = "..state.." inPosition = "..(inPosition and 1 or 0).."TIME ="..client.gameTime)
		print("X="..client.mousePosition.x.."; Y="..client.mousePosition.y.."; Team="..me.team)
	end
end

script:RegisterEvent(EVENT_TICK,Tick)
script:RegisterEvent(EVENT_KEY,Key)
