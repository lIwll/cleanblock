-- this script is auto generated by excel file: specialcard.xlsx
--      please don`t modify it.

local tt = {
	a_mt = { __index = i3k_Gdb.mt.translateMT},
	TKT_0 = {name = 1, },
	b_mt = { __index = i3k_Gdb.mt.defaultMT},
	c_mt = { __index = i3k_Gdb.mt.defaultMT},
}
tt.a = setmetatable({ value = 0, }, tt.c_mt)
tt.b = { id = 0, count = 0, }
tt.a_mt.transKeys = tt.TKT_0
tt.b_mt.default = {id = 2011, }
tt.c_mt.default = {id = 2021, }
i3k_db_special_card = 
{
	[1] = setmetatable({ id = 1, name_translate_key = "月卡", attrs = { [1] = setmetatable({ value = 0, }, tt.b_mt), [2] = tt.a, [3] = tt.a, }, dayReward = { [1] = { id = 1, count = 120, }, [2] = { id = 120227, count = 1, }, [3] = { id = 110117, count = 5, }, }, lastTime = 30, offlineAddtion = 0, takeVitAddtion = 0, }, tt.a_mt), 
	[2] = setmetatable({ id = 2, name_translate_key = "周卡", attrs = { [1] = setmetatable({ value = 10, }, tt.b_mt), [2] = setmetatable({ value = 10, }, tt.c_mt), [3] = { id = 2001, value = 80, }, }, dayReward = { [1] = tt.b, [2] = tt.b, [3] = tt.b, }, lastTime = 7, offlineAddtion = 2, takeVitAddtion = 5000, }, tt.a_mt), 
};
