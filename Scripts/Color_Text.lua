--<< Colored script by DarkPower v0.9 >>
--it's my first script for ensage.
require("libs.ScriptConfig")
require("libs.SideMessage")
local config = ScriptConfig.new()
config:SetParameter("Rand", false)
config:SetParameter("RUS", false)
config:Load()

local rand = config.Rand
local rus = config.RUS
local sayt = false
_colors = {
                0x00FF00FF,
                0xFF00FFFF,
                0x6E8B3DFF,
                0xFF6EB4FF,
                0xFF0000FF,
                0xFF4500FF,
                0xFFA500FF,
                0x54FF9FFF,
                0x0000FFFF,
                0xC1CdC1FF,
                0xFF9700FF,
				0x6B8E23FF,
				0x00FFFFFF
        }
local c={"","","","","","","","","","","","",""}
local cr = {"","","","","","","","","","","","","","","","",""}
local t1={"0","1","2","3","4","5","6","7","8","9"}
local t3={")","!","@","#","$","%","^","&","*","("}
local t5={"ф","и","с","в","у","а","п","р","ш","о","л","д","ь","т","щ","з","й","к","ы","е","г","м","ц","ч","н","я"}
local t6={"Ф","И","С","В","У","А","П","Р","Ш","О","Л","Д","Ь","Т","Щ","З","Й","К","Ы","Е","Г","М","Ц","Ч","Н","Я"}
local t2={"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"}
local t4={"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"}
local param=1
local test = "TECT"
local record = false
local r=1
-- Config
local x = 50
local y = 50

-- Code
registered = false
font = drawMgr:CreateFont("Colorfont","Arial",14,500)
defaultText = "Open Chat and record text"
text = drawMgr:CreateText(x,y,-1,defaultText,font)

function GenerateSideMessage(msg)
	local sidemsg = sideMessage:CreateMessage(200,60,0x111111C0,0x00FFFFFF,150,1000)
	sidemsg:AddElement(drawMgr:CreateText(85,20,0xFFFF00FF,"  " .. msg,drawMgr:CreateFont("F17","Tahoma",17,550)))
end

function Inc_r()
	if r == 17 then r = 1
	else r =r +1
	end
end

function Key(msg,code)
	if msg ~= KEY_DOWN or not client.connected or client.loading then
		return
	end
	if record then
		if not client.chat then
			text.color = 0xFF0000FF
			text.text = "Open chat"
			GenerateSideMessage("Open chat first")
			record=false
			client:ExecuteCmd("bind enter say")
			return
		end
		if code >= 97 and code <= 109 then
			test=test..c[code-97+1]
			text.color = _colors[code-97+1]
		end
		if code == 111 then
			test=""
		elseif code == 110 then
			if rus then
				GenerateSideMessage("ENG TEXT")
				rus=false
			else
				GenerateSideMessage("RUS TEXT")
				rus=true
			end
		elseif code == 9 then
			if rus then
				GenerateSideMessage("ENG TEXT")
				rus=false
			else
				GenerateSideMessage("RUS TEXT")
				rus=true
			end
		elseif code == 187 then
			if IsKeyDown(16) then
				test=test.."+"
			else
				test=test.."="
			end
		elseif code == 27 then 
			record=false
			test = defaultText
			record = false
			client:ExecuteCmd("bind enter say")
		elseif code == 222 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Э"
				else
					test=test.."э"
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 186 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Ж"
				else
					test=test.."ж"
				end
			else
				if IsKeyDown(16) then
					test=test..":"
				else
					test=test..";"
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 188 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Б"
				else
					test=test.."б"
				end
			else
				if IsKeyDown(16) then
					test=test.."<"
				else
					test=test..","
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 189 then
			if IsKeyDown(16) then
				test=test.."_"
			else
				test=test.."-"
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 190 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Ю"
				else
					test=test.."ю"
				end
			else
				if IsKeyDown(16) then
					test=test..">"
				else
					test=test.."."
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 219 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Х"
				else
					test=test.."х"
				end
			else
				if IsKeyDown(16) then
					test=test.."{"
				else
					test=test.."["
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 221 then
			if rus then
				if IsKeyDown(16) then
					test=test.."Ъ"
				else
					test=test.."ъ"
				end
			else
				if IsKeyDown(16) then
					test=test.."}"
				else
					test=test.."]"
				end
			end
		if rand then test=test..cr[r] Inc_r() end
		elseif code == 191 then
			if IsKeyDown(16) then
				test=test.."?"
			else
				test=test.."/"
			end
		elseif code == 32 then
			test=test.." "
		elseif code == 8 then
			if #test > 0 then 
				test = string.sub (test, 1, #test-1)
				if rand then test = string.sub (test, 1, #test-2) end
			end
		end
		if code >= 48 and code <= 57 then
			if IsKeyDown(16) then
				test=test..t3[code-48+1]
			else
				test=test..t1[code-48+1]
			end	
			if rand then test=test..cr[r] Inc_r() end
		end
		if code >= 65 and code <= 90 then
			if rus then
				if IsKeyDown(16) then
					test=test..t6[code-65+1]
				else
					test=test..t5[code-65+1]
				end	
			else
				if IsKeyDown(16) then
					test=test..t4[code-65+1]
				else
					test=test..t2[code-65+1]
				end
			end
			if rand then test=test..cr[r] Inc_r() end
		end
		text.text = test
	end
	if code == 13 then
		if record then
			record = false
			text.text = defaultText
			if sayt then
				client:ExecuteCmd("say_team "..test)
			else
				client:ExecuteCmd("say "..test)
			end
			client:ExecuteCmd("bind enter say")
		else
			if IsKeyDown(16) then sayt = false
			else sayt = true
			end
			test = " "
			text.text = "Record"
			record = true
			client:ExecuteCmd("unbind enter")
		end
		
	end	
end

function Close()
	text.text = defaultText
	text.visible = false
	record = false
	registered = false
end

function Load()
	if registered then return end
	text.visible = true
	registered = true
	param=1
	record = false
end

script:RegisterEvent(EVENT_CLOSE,Close)
script:RegisterEvent(EVENT_LOAD,Load)
script:RegisterEvent(EVENT_KEY,Key)

if client.connected and not client.loading then
	Load()
end
