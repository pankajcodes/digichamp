using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DigiChamps.Common
{
    public static class EnumExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum value)
        {

            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            var results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();

            var members = type.GetMember(value.ToString());
            if (members.Length == 0)
            {
                return Enum.GetName(type, value);
            }

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length == 0)
            {
                return Enum.GetName(type, value);
            }

            var attribute = (DisplayAttribute)attributes[0];
            return attribute.GetName();
        }
        public static string GetCategoryName(this Enum value)
        {
            var type = value.GetType();

            var members = type.GetMember(value.ToString());
            if (members.Length == 0)
            {
                return "";
            }

            var member = members[0];
            var attributes = member.GetCustomAttributes(typeof(CategoryAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }

            var attribute = (CategoryAttribute)attributes[0];
            return attribute.Category;
        }
    }
}
