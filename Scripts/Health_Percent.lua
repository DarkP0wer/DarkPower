--<< Health and Percent of health and PA enemy v0.2 >>
require("libs.ScriptConfig")
require("libs.Utils")
require("libs.SideMessage")

local config = ScriptConfig.new()
config:SetParameter("PosX", -100)
config:SetParameter("PosY", 0)
config:SetParameter("Font", 13)
config:Load()

_Colors = {0xFF0000FF, 0xFF0080FF, 0xFF8000FF} 
local F14 = drawMgr:CreateFont("F14","Calibri",config.Font,500)
local hero = {}

local Bro = nil
function math_round( roundIn , roundDig )
     local mul = math.pow( 10, roundDig )
     return ( math.floor( ( roundIn * mul ) + 0.5 )/mul )
end
 
function Tick( tick )
	if PlayingGame() then
		local me = entityList:GetMyHero()
		if not me then return end
		local enemies = entityList:GetEntities({type=LuaEntity.TYPE_HERO,team = 5-me.team})
		local EnemyH = true
		
		if me.healthbarOffset ~= -1 and not me:IsIllusion() and me.name == "npc_dota_hero_phantom_assassin"then
			if not Bro then
				Bro = drawMgr:CreateText(20,0-45, 0xFFFFFF99, " NONE",F14) 
				Bro.visible = false 
				Bro.entity = me 
				Bro.entityPosition = Vector(config.PosX,config.PosY,me.healthbarOffset)
			end
			Bro.visible = true
			local my_modifiers = me.modifiers
			if my_modifiers and #my_modifiers > 0 and me.alive and me:GetAbility(3).level ~= 0 then
				for i, z in ipairs(my_modifiers) do
					if z.name == "modifier_phantom_assassin_blur_active" then 
						EnemyH = false
					end
				end
				if EnemyH then
					Bro.text = "ENEMY"
					Bro.visible = true
					Bro.color = _Colors[math.random(1,3)]
				else 
					Bro.text = "NONE"
					Bro.color = 0xffffffff
				end
			end
		end
		
		for i,v in ipairs(enemies) do
			if v.healthbarOffset ~= -1 and not v:IsIllusion() then
				local hand = v.handle
				if not hero[hand] then
					hero[hand] = drawMgr:CreateText(20,0-45, 0xFFFFFF99, "",F14) 
					hero[hand].visible = false 
					hero[hand].entity = v 
					hero[hand].entityPosition = Vector(config.PosX,config.PosY,v.healthbarOffset)
				end
				hero[hand].visible = false
				if v.visible then 
					hero[hand].visible = true
					procent = math_round(v.health/v.maxHealth*100,1)
					hero[hand].text = " "..v.health.."("..procent.."%)"
					if procent <= 25.0 then
						hero[hand].color = 0xFF0000FF
					elseif procent <= 50.0 and procent >= 25.0 then
						hero[hand].color = 0xFF8C00FF
					elseif procent <= 75.0 and procent >= 50.0 then
						hero[hand].color = 0xFFFF00FF
					else 
						hero[hand].color = 0x00FF00FF
					end
				end
			end
		end
	end
 
end

script:RegisterEvent(EVENT_TICK,Tick)
