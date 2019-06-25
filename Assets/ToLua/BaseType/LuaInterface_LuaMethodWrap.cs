﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using System.Runtime.InteropServices;
using LuaInterface;

public class LuaInterface_LuaMethodWrap
{
	public static void Register(LuaState L)
	{
		IntPtr lazyWrapFunc = Marshal.GetFunctionPointerForDelegate((LuaCSFunction)LazyWrap);
		L.BeginClass(typeof(LuaInterface.LuaMethod), typeof(System.Object));
		L.RegLazyFunction("Destroy", lazyWrapFunc);
		L.RegLazyFunction("Call", lazyWrapFunc);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Destroy(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaMethod obj = (LuaMethod)ToLua.CheckObject(L, 1, typeof(LuaMethod));
			obj.Destroy();
            ToLua.Destroy(L);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Call(IntPtr L)
	{
		try
		{			
			LuaMethod obj = (LuaMethod)ToLua.CheckObject(L, 1, typeof(LuaMethod));            
			return obj.Call(L);						
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LazyWrap(IntPtr L)
	{
		try
		{
			bool lazy = LuaDLL.luaL_checkboolean(L, LuaDLL.lua_upvalueindex(5));
			string key = LuaDLL.lua_tostring(L, LuaDLL.lua_upvalueindex(4));

			switch (key)
			{
				case "Destroy":
					return ToLua.LazyRegisterFunc(lazy, "Destroy", Destroy, L);
				case "Call":
					return ToLua.LazyRegisterFunc(lazy, "Call", Call, L);
			}
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

