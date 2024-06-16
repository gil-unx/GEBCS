using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RETAEDOG_GUI
{
    internal static class Language
    {


        static Language()
        {

            



        }


        private static Dictionary<string, Dictionary<string, string>> RootMap = new Dictionary<string, Dictionary<string, string>>();

        private static string LanguageCode = "en";




        public static void SetLanguageCode()
        {


            string culture = System.Globalization.CultureInfo.CurrentCulture.Name;



            if (culture.Contains("zh"))
            {
                LanguageCode = "zh";
            }
            else
            {
                LanguageCode = "en";
            }

        }


        

        public static string Get(string id)
        {

            return RootMap[LanguageCode][id];
        }

    }
}
