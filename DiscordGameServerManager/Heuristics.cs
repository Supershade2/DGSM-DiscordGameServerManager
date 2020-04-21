using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
namespace DiscordGameServerManager
{
    public class Heuristics
    {
        public static string newline = Environment.NewLine;
        
        public static string produceString(string input)
        {
            bool IsCompatible = false;
            if(CultureInfo.CurrentCulture.Name.ToLower() == "en-us") 
            {
                IsCompatible = true;
            }
            if (IsCompatible) 
            {
                string pattern = "[ABCDEFGHIJKLMNOPQRSTUVWXYZ]";
                //bool CapitalAssigned = false;
                bool SentenceEnd = true;
                //bool wasNumber = false;
                string output = "";
                char[] stringPart = input.ToCharArray();
                for (int ci = 0; ci < stringPart.Length; ci++)
                {
                    if ((int)stringPart[ci] > 64 && (int)stringPart[ci] < 91 || (int)stringPart[ci] > 96 && (int)stringPart[ci] < 123)
                    {
                        Match m = Regex.Match(stringPart[ci].ToString(), pattern);
                        int cha = ci == 0 && m.Success == false ? (int)stringPart[ci] - 32 : SentenceEnd == true && m.Success == false ? (int)stringPart[ci] - 32 : m.Success == true && SentenceEnd == true ? (int)stringPart[ci] : m.Success == false ? (int)stringPart[ci] : (int)stringPart[ci] + 32;
                        //CapitalAssigned = cha == (int)e - 32 && SentenceEnd == false;
                        output += (char)cha;
                        SentenceEnd = false;
                    }
                    else
                    {
                        SentenceEnd = stringPart[ci] == '.' || stringPart[ci] == '!' && stringPart[ci + 1 < stringPart.Length ? ci + 1 : ci] != ',';
                        output += stringPart[ci];
                        //manualIndex = manualIndex+1<numbers.Length ? manualIndex+1:manualIndex;
                    }
                    //manualIndex = manualIndex < numbers.Length ? manualIndex + 1 : manualIndex;
                }
                return output;
            }
            return input;
        }
    }
}
