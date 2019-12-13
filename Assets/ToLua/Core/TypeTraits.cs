﻿/*
Copyright (c) 2015-2021 topameng(topameng@qq.com)
https://github.com/topameng/tolua

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System;
using System.Collections;

namespace LuaInterface
{
    public static class TypeTraitsBase
    {
        static Il2cppType il2cpp = new Il2cppType();
        
        public static Func<IntPtr, int, bool> GetDefaultCheck(bool IsValueType, Type type)
        {
            int nilType = -1;
            return (IntPtr L, int pos) =>
            {
                LuaTypes luaType = LuaDLL.lua_type(L, pos);
                switch (luaType)
                {
                    case LuaTypes.LUA_TUSERDATA:
                        return IsUserData(L, pos, type);
                    case LuaTypes.LUA_TNIL:
                        if (nilType != -1)
                        {
                            return nilType != 0;
                        }

                        nilType = IsNilType(IsValueType, type) ? 1 : 0;
                        return nilType != 0;      
                    case LuaTypes.LUA_TTABLE:
                        return IsUserTable(L, pos, type);
                    default:
                        return false;
                }            
            };
        }

        public static Func<IntPtr, int> GetLuaReference(Type type)
        {
            int metaref = -1;
            return (IntPtr L )=>
            {
#if MULTI_STATE
                return LuaStatic.GetMetaReference(L, type);
#else
                if (metaref > 0)
                {                
                    return metaref;
                }

                metaref = LuaStatic.GetMetaReference(L, type);

                if (metaref > 0)
                {
                    LuaState.Get(L).OnDestroy += () => { metaref = -1; };
                }

                return metaref; 
#endif

            };
        }

        public static bool IsNilType(bool IsValueType, Type type)
        {
            if (!IsValueType)
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == il2cpp.TypeofGenericNullObject)
            {
                return true;
            }

            return false;            
        }

        static bool IsUserData(IntPtr L, int pos, Type type)
        {
            int udata = LuaDLL.tolua_rawnetobj(L, pos);

            if (udata != -1)
            {
                ObjectTranslator translator = ObjectTranslator.Get(L);
                Type eleType = translator.CheckOutNodeType(udata);
                return eleType == null ? udata == 1 : eleType == type || type.IsAssignableFrom(eleType);
            }

            return false;
        }

        static bool IsUserTable(IntPtr L, int pos, Type type)
        {            
            if (type == TypeTraits<LuaTable>.type)
            {
                return true;
            }
            else if (type.IsArray)
            {
                if (type.GetElementType().IsArray || type.GetArrayRank() > 1)
                {
                    return false;
                }

                return true;
            }
            else if (LuaDLL.tolua_isvptrtable(L, pos))
            {
                return IsUserData(L, pos, type);
            }

            return false;
        }
    }

    public static class TypeTraits<T>
    {        
        static public Func<IntPtr, int, bool> Check;
        static readonly public Func<IntPtr, int> GetLuaReference;
        static readonly public Type type = typeof(T);
        static readonly public bool IsValueType = type.IsValueType;
        static readonly public bool IsArray = type.IsArray;

        static string typeName = string.Empty;                

        static TypeTraits()
        {
            Check = TypeTraitsBase.GetDefaultCheck(IsValueType, type);
            GetLuaReference = TypeTraitsBase.GetLuaReference(type);
        }

        static public void Init(Func<IntPtr, int, bool> check)
        {            
            if (check != null)
            {
                Check = check;
            }
        }

        static public string GetTypeName()
        {
            if (typeName == string.Empty)
            {
                typeName = LuaMisc.GetTypeName(type);
            }

            return typeName;
        }
    }    

    public static class DelegateTraits<T>
    {        
        static DelegateFactory.DelegateCreate _Create = null;
        static readonly public Type _type = TypeTraits<T>.type;

        static public void Init(DelegateFactory.DelegateCreate func)
        {
            _Create = func;            
        }

        static public Delegate Create(LuaFunction func)
        {
#if UNITY_EDITOR
            if (_Create == null)
            {
                throw new LuaException(string.Format("Delegate {0} not register", TypeTraits<T>.GetTypeName()));
            }
#endif
            if (func != null)
            {
                LuaState state = func.GetLuaState();
                LuaDelegate target = state.GetLuaDelegate(func);

                if (target != null)
                {
                    return Delegate.CreateDelegate(_type, target, target.method);
                }
                else
                {
                    Delegate d = _Create(func, null, false);
                    target = d.Target as LuaDelegate;
                    state.AddLuaDelegate(target, func);
                    return d;
                }
            }

            return _Create(null, null, false);            
        }

        static public Delegate Create(LuaFunction func, LuaTable self)
        {
#if UNITY_EDITOR
            if (_Create == null)
            {
                throw new LuaException(string.Format("Delegate {0} not register", TypeTraits<T>.GetTypeName()));
            }
#endif
            if (func != null)
            {
                LuaState state = func.GetLuaState();
                LuaDelegate target = state.GetLuaDelegate(func, self);

                if (target != null)
                {
                    return Delegate.CreateDelegate(_type, target, target.method);
                }
                else
                {
                    Delegate d = _Create(func, self, true);
                    target = d.Target as LuaDelegate;
                    state.AddLuaDelegate(target, func, self);
                    return d;
                }
            }

            return _Create(null, null, true);            
        }
    }

    public static class StackTraits<T>
    {
        static public Action<IntPtr, T> Push = SelectPush();
        static public Func<IntPtr, int, T> Check = DefaultCheck;
        static public Func<IntPtr, int, T> To = ToLua.ToGenericObject<T>;               

        static public void Init(Action<IntPtr, T> push, Func<IntPtr, int, T> check, Func<IntPtr, int, T> to)
        {
            if (push != null)
            {
                Push = push;
            }

            if (to != null)
            {
                To = to;
            }

            if (check != null)
            {
                Check = check;
            }            
        }

        static Action<IntPtr, T> SelectPush()
        {
            if (TypeChecker.IsValueType(TypeTraits<T>.type))
            {
                return PushValue;
            }
            else if (TypeTraits<T>.IsArray)
            {
                return PushArray;
            }
            else
            {
                return PushObject;
            }
        }

        static void PushValue(IntPtr L, T o)
        {
            ToLua.PushData(L, o);
        }

        static void PushObject(IntPtr L, T o)
        {
            ToLua.PushObject(L, o);
        }

        static void PushArray(IntPtr L, T array)
        {
            if (array == null)
            {
                LuaDLL.lua_pushnil(L);
            }
            else
            {
                int arrayMetaTable = LuaStatic.GetArrayMetatable(L);
                ToLua.PushUserData<object>(L, array, arrayMetaTable);
            }
        }

        static T DefaultCheck(IntPtr L, int stackPos)
        {
            int udata = LuaDLL.tolua_rawnetobj(L, stackPos);            

            if (udata != -1)
            {
                ObjectTranslator translator = ObjectTranslator.Get(L);
                Type eleType = translator.CheckOutNodeType(udata);
                if (eleType != null)
                {
                    bool bValid = eleType == TypeTraits<T>.type || TypeTraits<T>.type.IsAssignableFrom(eleType);
                    if (bValid)
                    {
                        return translator.GetObject<T>(udata);
                    }
                    else LuaDLL.luaL_argerror(L, stackPos, string.Format("{0} expected, got {1}", TypeTraits<T>.GetTypeName(), eleType != null ? eleType.FullName : "null")); 
                }
                else if (udata == 1) return default(T);
            }
            else if (TypeTraitsBase.IsNilType(TypeTraits<T>.IsValueType, TypeTraits<T>.type) && LuaDLL.lua_isnil(L, stackPos))
            {
                return default(T);
            }

            LuaDLL.luaL_typerror(L, stackPos, TypeTraits<T>.GetTypeName());
            return default(T);            
        }
    }
}
