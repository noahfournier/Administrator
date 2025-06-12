using System.Collections.Generic;
using Life.Network;

namespace Administrator.Helpers
{
    public static class Colors
    {
        private static readonly Dictionary<string, string> _colorMap = new Dictionary<string, string>
        {
            { "blue", LifeServer.COLOR_BLUE },
            { "red", LifeServer.COLOR_RED },
            { "green", LifeServer.COLOR_GREEN },
            { "orange", LifeServer.COLOR_ORANGE },
            { "pink", LifeServer.COLOR_ME },
            { "yellow", "yellow" },
            { "purple", "purple" },
            { "white", "#FFFFFF" },
            { "black", "#000000" },
            { "grey", "grey" }
        };

        public static string GetColorHex(string colorName)
        {
            if (_colorMap.TryGetValue(colorName.ToLower(), out string hex))
            {
                return hex;
            }
            else
            {
                return "#FFFFFF";
            }
        }

        public static string BlueColor => "blue";
        public static string RedColor => "red";
        public static string GreenColor => "green";
        public static string OrangeColor => "orange";
        public static string PinkColor => "pink";
        public static string YellowColor => "yellow";
        public static string PurpleColor => "purple";
        public static string WhiteColor => "white";
        public static string BlackColor => "black";
        public static string GreyColor => "grey";

        public static string Format(string colorName, string message)
        {
            string hex = GetColorHex(colorName);
            return $"<color={hex}>{message}</color>";
        }
    }
}