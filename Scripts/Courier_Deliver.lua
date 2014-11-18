--<< Сourier_Deliver v0.3 >>
require("libs.ScriptConfig")
require("libs.SideMessage")

local config = ScriptConfig.new()
config:SetParameter("HotKey", "L", config.TYPE_HOTKEY)
config:SetParameter("SaveKey", "K", config.TYPE_HOTKEY)
config:SetParameter("WaitT", 3)
config:Load()
local WaitT = config.WaitT
local Work = 0
local Wait = 0
registered = false

function GenerateSideMessage(msg)
	local sidemsg = sideMessage:CreateMessage(200,60,0x111111C0,0x00FFFFFF,150,1000)
	sidemsg:AddElement(drawMgr:CreateText(85,20,0xFFFF00FF," " .. msg,drawMgr:CreateFont("F17","Tahoma",17,550)))
end
function Key(msg,code)
	if msg ~= KEY_DOWN or not client.connected or client.loading or client.chat then
		return
	end
	if code == 109 then
		if WaitT > 1 then 
			WaitT = WaitT - 1
		else 
			WaitT = 1
		end
		GenerateSideMessage("Wait Time: "..WaitT)
	end
	if code == 107 then
		if WaitT < 8 then 
			WaitT = WaitT + 1
		else 
			WaitT = 8
		end
		GenerateSideMessage("Wait Time: "..WaitT)
	end
	if code == config.HotKey then
		if work == 1 then 
			work = 0
			GenerateSideMessage("Deliver OFF")
		else 
			Wait = 0
			work = 1
			GenerateSideMessage("Deliver ON")
		end
	elseif code == config.SaveKey then
		if work == 2 then
			work = 0 
			GenerateSideMessage("Save Cur OFF")
		else
			wait = 0
			work = 2
			GenerateSideMessage("Save Cur ON")
		end
	end
end

function Tick(tick)
	if work == 1 then
		if Wait >= WaitT then 
			client:ExecuteCmd("dota_courier_deliver")
			Wait = 0
		else 
			Wait = Wait + 1
		end
	elseif work == 2 then
		if Wait >= WaitT then
			local me = entityList:GetMyHero()
			local cour = entityList:FindEntities({classId = CDOTA_Unit_Courier,team = me.team,alive = true})[1]
			if cour ~= nil and cour.alive then
				cour:CastAbility(cour:GetAbility(1))
				if cour:GetAbility(6).state == LuaEntityAbility.STATE_READY then
					cour:CastAbility(cour:GetAbility(6))
				end
			end
			Wait = 0
		else 
			Wait = Wait + 1
		end
	end
end

function Close()
	registered = false
	Wait = 0
end

function Load()
	if registered then return end
	registered = true
	Wait = 0
end

script:RegisterEvent(EVENT_CLOSE,Close)
script:RegisterEvent(EVENT_LOAD,Load)
script:RegisterEvent(EVENT_KEY,Key)
script:RegisterEvent(EVENT_TICK,Tick)

if client.connected and not client.loading then
	Load()
end
