//*********************************************************************
//xToolkit
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xarial.XToolkit.Reflection
{
    public static class TypeExtension
    {
        public static bool TryGetAttribute<TAtt>(this Type type, out TAtt att, bool searchInParentTypes = false)
            where TAtt : Attribute
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            att = null;

            var custAtts = type.GetCustomAttributes(typeof(TAtt), true);

            if (custAtts != null && custAtts.Any())
            {
                att = custAtts.First() as TAtt;
                return true;
            }
            else
            {
                if (searchInParentTypes)
                {
                    var baseType = type.BaseType;

                    if (baseType != null && baseType != typeof(object))
                    {
                        if (baseType.TryGetAttribute(out att, searchInParentTypes))
                        {
                            return true;
                        }
                    }

                    var interfaces = type.GetInterfaces();

                    foreach (var parent in interfaces)
                    {
                        if (parent.TryGetAttribute(out att, searchInParentTypes))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public static bool IsAssignableToGenericType(this Type thisType, Type genericType)
        {
            return thisType.TryFindGenericType(genericType) != null;
        }

        public static Type[] GetArgumentsOfGenericType(this Type thisType, Type genericType)
        {
            var type = thisType.TryFindGenericType(genericType);

            if (type != null)
            {
                return type.GetGenericArguments();
            }
            else
            {
                return Type.EmptyTypes;
            }
        }

        public static Type TryFindGenericType(this Type thisType, Type genericType)
        {
            var interfaceTypes = thisType.GetInterfaces();

            bool CanCastFunc(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType;

            foreach (var it in interfaceTypes)
            {
                if (CanCastFunc(it))
                {
                    return it;
                }
            }

            if (CanCastFunc(thisType))
            {
                return thisType;
            }

            var baseType = thisType.BaseType;

            if (baseType != null)
            {
                return baseType.TryFindGenericType(genericType);
            }

            return null;
        }

        public static Enum[] GetEnumFlags(this Type enumType)
        {
            if (!enumType.IsEnum) 
            {
                throw new Exception("Only flag enums are supported");
            }

            var flags = new List<Enum>();

            var flag = 0x1;

            foreach (Enum value in Enum.GetValues(enumType))
            {
                var bits = Convert.ToInt32(value);

                if (bits != 0)
                {
                    while (flag < bits)
                    {
                        flag <<= 1;
                    }
                    if (flag == bits)
                    {
                        flags.Add(value);
                    }
                }
            }

            return flags.ToArray();
        }
    }
}
