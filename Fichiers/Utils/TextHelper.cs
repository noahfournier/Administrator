namespace Administrator.Helpers
{
    public static class TextHelper
    {
        public static string Title(string title, string description, string colorName = "blue")
        {
            string hexColor = Colors.GetColorHex(colorName);
            return $"<b><color={hexColor}>{title}</color></b><br><size=13>{description}</size>";
        }

        public static string TabInfo(string title, string description, string colorName = "white")
        {
            string hexColor = Colors.GetColorHex(colorName);
            string titleFormat = $"<b><color={hexColor}>{title}</color></b>";
            return $"{titleFormat}<br><size=13>{description}</size>";
        }
    }
}
