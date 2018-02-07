using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DigiChamps.Common
{
    public static class EnumHelper<T>
    {
        public static string GetEnumDescription(string value)
        {
            Type type = typeof(T);
            if (value.Equals(null))
                value = string.Empty;
            var name = Enum.GetNames(type).Where(f => f.Equals(value, StringComparison.CurrentCultureIgnoreCase)).Select(d => d).FirstOrDefault();

            if (name.Equals(null))
            {
                return string.Empty;
            }
            var field = type.GetField(name);
            var customAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : name;
        }

        public static string GetEnumDisplayDescription(string value)
        {
            Type type = typeof(T);
            if (value.Equals(null))
                value = string.Empty;
            var name = Enum.GetNames(type).Where(f => f.Equals(value, StringComparison.CurrentCultureIgnoreCase)).Select(d => d).FirstOrDefault();

            if (name.Equals(null))
            {
                return string.Empty;
            }
            var field = type.GetField(name);
            var customAttribute = field.GetCustomAttributes(typeof(DisplayAttribute), false);
            return customAttribute.Length > 0 ? ((DisplayAttribute)customAttribute[0]).Name : name;
        }

        public static string GetEnumNameByDescription(string value)
        {
            Type type = typeof(T);
            string enumName = string.Empty;
            if (value.Equals(null))
                value = string.Empty;

            var name = Enum.GetNames(type);
            foreach (string enm in name)
            {
                var field = type.GetField(enm);
                var customAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string desc = customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : enm;
                if (desc.Equals(value))
                {
                    enumName = enm;
                }
            }
            return enumName;
        }
      
        public static T ParseEnum(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T GetValueFromDescription(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            //Not Found try t return none
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == "")
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        public static IEnumerable<T> ToEnumerable()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static List<KeyValuePair<string, string>> GetEnumToList<O>()
        {
            return Enum.GetValues(typeof(O)).Cast<O>().Select(e => new KeyValuePair<string, string>(e.GetHashCode().ToString(), EnumHelper<O>.GetEnumDescription(e.ToString()))).ToList();
        }
        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }
    }
}