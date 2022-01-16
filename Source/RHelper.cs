using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SNTSS22
{
    public class RHelper
    {
        static readonly Random m_R = new Random();
#pragma warning disable IDE0052 // Remove unread private members
        /// <summary>Unit testing.</summary>
        static readonly RHelper m_Dbg = new RHelper();
#pragma warning restore IDE0052 // Remove unread private members
        public RHelper() {

        }
        public static T Choice<T>(T[] aArray) {
            return aArray[m_R.Next(0, -1 + aArray.Length)];
        }
        public static T Choice<T>(List<T> aArray) {
            return Choice(aArray.ToArray());
        }
        public static bool Roll(int aMax) { return Roll(0, aMax); }
        public static bool Roll(int aMin, int aMax) { return 0 == m_R.Next(aMin, aMax); }
        public static string RandStr(int aLength, string aChars) {
            string vR = "";
            for (int vI = 0; vI < aLength; vI++)
                vR += aChars[m_R.Next(0, aChars.Length - 1)];
            return vR;
        }
        public static string RandBase64(int aLength, string aSymbols = "+/", string aNumbers= "0123456789", string aLetters1= "abcdefghijklmnopqrstuvwxyz", string aLetters2= "ABCDEFGHIJKLMNOPQRSTUVWXYZ") {
            string vChars = aSymbols + aNumbers + aLetters1 + aLetters2;
            return RandStr(aLength, vChars);
        }
        public static void Shuffle<T>(IList<T> aList) {
            int vN = aList.Count;
            while (vN > 1) {
                vN--;
                int vK = m_R.Next(vN + 1);
                T vItem = aList[vK];
                aList[vK] = aList[vN];
                aList[vN] = vItem;
            }
        }
        static bool GetRandomStringByIdHelper(int aID, string aChars, ref string aRet, ref int aIterator) {
            if (aIterator++ == aID) aRet = RandStr(1, aChars);
            else if (aIterator++ == aID) aRet = RandStr(2, aChars);
            else if (aIterator++ == aID) aRet = RandStr(3, aChars);
            else if (aIterator++ == aID) aRet = RandStr(4, aChars);
            else if (aIterator++ == aID) aRet = RandStr(8, aChars);
            else if (aIterator++ == aID) aRet = RandStr(16, aChars);
            else if (aIterator++ == aID) aRet = RandStr(32, aChars);
            else if (aIterator++ == aID) aRet = RandStr(64, aChars);
            else if (aIterator++ == aID) aRet = RandStr(128, aChars);
            return aRet != "";
        }
        /* [JavaScript]
         * This is used to get documentation for GetRandomStringById
         * Add "else " for first if then paste rest else ifs
         * Then process the code in js console
         `paste if-else-if`.split("\n").map(x=>x.trim().slice(40,-33)).map((x,i)=>{
return x + " Range(" + (i*9) + " to " + (i*9-1+9) +  ")";
}).join("\r\n");
         */
        public static string GetRandomStringById(string aId) {
            /*
                aId is hex, max is FF (255)
                vHex                Range(  0 to   8)
                vNum                Range(  9 to  17)
                vPassword           Range( 18 to  26)
                vAlphaNumBoth       Range( 27 to  35)
                vDashUSAlphaNumBoth Range( 36 to  44)
                vDashAlphaNumBoth   Range( 45 to  53)
                vUSAlphaNumBoth     Range( 54 to  62)
                vAlphaNumLC         Range( 63 to  71)
                vAlphaNumUC         Range( 72 to  80)
                vDashUSAlphaNumLC   Range( 81 to  89)
                vDashUSAlphaNumUC   Range( 90 to  98)
                vDashAlphaNumLC     Range( 99 to 107)
                vDashAlphaNumUC     Range(108 to 116)
                vUSAlphaNumLC       Range(117 to 125)
                vUSAlphaNumUC       Range(126 to 134)
            */
            int vID = Convert.ToInt32(aId, 16), vI = 0;
            string vRet = "", vNum = "0123456789", vHex = "0123456789ABCDEF",
                   vAlphaNumLC = "0123456789abcdefghijklmnopqrstuvwxyz",
                   vAlphaNumUC = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                   vAlphaNumBoth = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
                   vPassword = "!@#$%^&*_-+=0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
                   vDashUSAlphaNumLC = "-_0123456789abcdefghijklmnopqrstuvwxyz",
                   vDashUSAlphaNumUC = "-_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                   vDashUSAlphaNumBoth = "-_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
                   vDashAlphaNumLC = "-0123456789abcdefghijklmnopqrstuvwxyz",
                   vDashAlphaNumUC = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                   vDashAlphaNumBoth = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
                   vUSAlphaNumLC = "_0123456789abcdefghijklmnopqrstuvwxyz",
                   vUSAlphaNumUC = "_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                   vUSAlphaNumBoth = "_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",
                   _ = "";
            if (GetRandomStringByIdHelper(vID, vHex, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vNum, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vPassword, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vAlphaNumBoth, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashUSAlphaNumBoth, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashAlphaNumBoth, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vUSAlphaNumBoth, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vAlphaNumLC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vAlphaNumUC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashUSAlphaNumLC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashUSAlphaNumUC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashAlphaNumLC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vDashAlphaNumUC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vUSAlphaNumLC, ref vRet, ref vI)) return vRet;
            else if (GetRandomStringByIdHelper(vID, vUSAlphaNumUC, ref vRet, ref vI)) return vRet;
            return "";
        }
        /// <summary>
        /// Transforms unmagickal url query params into magickal url query params
        /// </summary>
        /// <param name="aURLQuery">#xx is valid format per magickal.</param>
        /// <returns></returns>
        public static string GetMagickalQuery(string aURLQuery) {
            // Parse params by splitting with ampersand first
            var vParts = aURLQuery.Split('&');
            // Loop through each
            for (var vI = 0; vI < vParts.Length; vI++) {
                var vPart = vParts[vI];
                // Parse param name value by splitting by eq
                var vParts2 = vPart.Split('=');
                // First value always exists, unescape it
                vParts2[0] = Uri.UnescapeDataString(vParts2[0]);
                // Get Magickals
                if (1 == vParts2.Length)
                    vParts[vI] = GetMagickalEscaped(vParts2[0]);
                else {
                    vParts2[1] = Uri.UnescapeDataString(vParts2[1]);
                    vParts[vI] = GetMagickalEscaped(vParts2[0]) + "=" + GetMagickalEscaped(vParts2[1]);
                    
                }
            }
            return string.Join("&", vParts);
        }
        public static string GetMagickalString(string aString) {
            var vRegEx = new Regex(@"#[0-9a-fA-F]{2}");
            return vRegEx.Replace(aString, vMatch => {
                return GetRandomStringById(vMatch.Value.Substring(1));
            });
        }
        public static string GetMagickalEscaped(string aString) {
            return Uri.EscapeDataString(GetMagickalString(aString));
        }
        public static string GetIP() {
            return m_R.Next(100, 233) + "." + m_R.Next(100, 233) + "." + m_R.Next(100, 233) + "." + m_R.Next(100, 233);
        }
    }
}
