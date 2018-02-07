using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
//using System.Web.Script.Serialization;

namespace DigiChamps.Common
{
    public class CommonHelper
    {
        //private static readonly int _maxiSizeOfAnyDimensionOfThumbnailImage = Convert.ToInt16(CoreManager.GetAllAppConfigSettings().Where(i => i.AppKey == "MaxiSizeOfAnyDimensionOfThumbnailImage").FirstOrDefault() == null ? "0" : CoreManager.GetAllAppConfigSettings().Where(i => i.AppKey == "MaxiSizeOfAnyDimensionOfThumbnailImage").FirstOrDefault().AppVaue);
        //public static event RenderEvent BeforeRender;
        //private const string REGEX_GROUP_SELECTOR = "selector";
        //private const string REGEX_GROUP_STYLE = "style";
        //private const string STYLE_DEFAULT_TYPE = "style";
        ////amazing regular expression magic
        //private const string REGEX_GET_STYLES = @"(?<selector>[^\{\s]+\w+(\s\[^\{\s]+)?)\s?\{(?<style>[^\}]*)\}";

           

        public static void MakeGetWebRequest(string Url)
        {
            var getRequest = (HttpWebRequest)WebRequest.Create(Url);
            getRequest.Method = "GET";
            var getResponse = (HttpWebResponse)getRequest.GetResponse();
        }

        public static List<EnumKeyValueViewModel> GetEnumToList(Type enumType)
        {
            Array values = Enum.GetValues(enumType);
            return (from int value in values let e = Enum.ToObject(enumType, value) let display = ((Enum)e).GetDisplayName() select new EnumKeyValueViewModel { Key = value, Name = display }).OrderBy(x => x.Name).ToList();
        }

        public static List<EnumKeyValueViewModel> GetEnumToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Select(e => new EnumKeyValueViewModel
            {
                Key = Convert.ToInt32(e),
                Name = EnumHelper<T>.GetEnumDescription(e.ToString())
            }).ToList();
        }

        public static List<EnumKeyValueViewModel> GetEnumByCategoryToList(Type enumType, string category)
        {
            Array values = Enum.GetValues(enumType);
            List<EnumKeyValueViewModel> list = (from int value in values let e = Enum.ToObject(enumType, value) let display = ((Enum)e).GetDisplayName() let cat = ((Enum)e).GetCategoryName() select new EnumKeyValueViewModel { Key = value, Name = display, Category = cat }).ToList();
            return list.Where(e => e.Category == category).ToList();

        }



       
       

    }
    public class EnumKeyValueViewModel
    {
        public int Key { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }
    }
}