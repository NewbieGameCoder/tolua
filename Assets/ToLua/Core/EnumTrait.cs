using System;

namespace LuaInterface
{
    public static class EnumTrait<T> where T : struct, System.Enum
    {
        internal static Func<int, T> IntToEnumTransfer;
        internal static Func<T, int> EnumToInt;

        static Type type = typeof(T);

        internal static bool CheckType(IntPtr L, int pos)
        {
            return TypeChecker.CheckEnumType(type, L, pos);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int CompareTo(IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 2);

                T arg0 = ToLua.CheckObject<T>(L, 1);
                int result0 = EnumToInt(arg0);
                int result1 = 0;

                if (TypeChecker.CheckTypes<T>(L, 2))
                {
                    T arg1 = ToLua.CheckObject<T>(L, 2);
                    result1 = EnumToInt(arg1);
                }
                else if (TypeChecker.CheckTypes<int>(L, 2))
                {
                    result1 = (int)LuaDLL.lua_tointeger(L, 1);
                }

                int o = result0.CompareTo(result1);
                LuaDLL.lua_pushinteger(L, o);
                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int ToString(IntPtr L)
        {
            try
            {
                int count = LuaDLL.lua_gettop(L);

                if (count == 1)
                {
                    T obj = ToLua.CheckObject<T>(L, 1);
                    string o = obj.ToString();
                    LuaDLL.lua_pushstring(L, o);
                    return 1;
                }
                else if (count == 2)
                {
                    T obj = ToLua.CheckObject<T>(L, 1);
                    string arg0 = ToLua.CheckString(L, 2);
                    string o = obj.ToString(arg0);
                    LuaDLL.lua_pushstring(L, o);
                    return 1;
                }
                else
                {
                    return LuaDLL.luaL_throw(L, "invalid arguments to method: System.Enum.ToString");
                }
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int Equals(IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 2);

                T arg0 = ToLua.CheckObject<T>(L, 1);
                int result0 = EnumToInt(arg0);
                int result1 = 0;

                if (TypeChecker.CheckTypes<T>(L, 2))
                {
                    T arg1 = ToLua.CheckObject<T>(L, 2);
                    result1 = EnumToInt(arg1);
                }
                else if (TypeChecker.CheckTypes<int>(L, 2))
                {
                    result1 = (int)LuaDLL.lua_tointeger(L, 1);
                }

                LuaDLL.lua_pushboolean(L, result0 == result1);
                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int ToInt(IntPtr L)
        {
            try
            {
                T arg0 = ToLua.CheckObject<T>(L, 1);
                int ret = EnumToInt(arg0);
                LuaDLL.lua_pushinteger(L, ret);
                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        internal static int IntToEnum(IntPtr L)
        {
            try
            {
                int arg0 = (int)LuaDLL.lua_tointeger(L, 1);
                T o = IntToEnumTransfer(arg0);
                ToLua.PushValue(L, o);
                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }
    }
}