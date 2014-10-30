require("libs.ScriptConfig")

config = ScriptConfig.new()
config:SetParameter("PosX", -100)
config:SetParameter("PosY", 0)
config:SetParameter("Font", 13)
config:Load()

local F14 = drawMgr:CreateFont("F14","Calibri",config.Font,500)
local hero = {}
 
function math_round( roundIn , roundDig )
     local mul = math.pow( 10, roundDig )
     return ( math.floor( ( roundIn * mul ) + 0.5 )/mul )
end
 
function Tick( tick )
	local me = entityList:GetMyHero()
	local enemies = entityList:GetEntities({type=LuaEntity.TYPE_HERO,team = 5-me.team})
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

function Close()
	for i,v in ipairs(enemies) do
		if hero[hand] then
			hero[hand].visible = false
		end
	end
end

script:RegisterEvent(EVENT_CLOSE,Close)
script:RegisterEvent(EVENT_TICK,Tick)
