--<< Сourier_Deliver v0.2 >>
require("libs.ScriptConfig")

local config = ScriptConfig.new()
config:SetParameter("HotKey", "L", config.TYPE_HOTKEY)
config:SetParameter("WaitT", 3)
config:Load()

HotKey = config.HotKey
Work = false
WaitT = config.WaitT
Wait = 0
registered = false

function GenerateSideMessage(msg)
	local sidemsg = sideMessage:CreateMessage(200,60,0x111111C0,0x00FFFFFF,150,1000)
	sidemsg:AddElement(drawMgr:CreateText(85,20,-1," " .. msg,drawMgr:CreateFont("F17","Tahoma",17,550)))
end
function Key(msg,code)
	if msg ~= KEY_DOWN or not client.connected or client.loading or client.chat then
		return
	end
	if code == HotKey then
		if work then 
			work = false
			GenerateSideMessage("Cur Dissabled")
		else 
			Wait = 0
			work =true
			GenerateSideMessage("Cur Enabled")
		end
	end	
end

function Tick(tick)
	if not work then
		return
	end
	if Wait == WaitT then 
		client:ExecuteCmd("dota_courier_deliver")
		Wait = 0
	else 
		Wait = Wait + 1
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
