namespace AiTranslator;

public static class UIConstants
{
    // Modern Color Scheme
    public static class Colors
    {
        public static readonly Color Primary = Color.FromArgb(0, 120, 215);      // Modern blue
        public static readonly Color PrimaryHover = Color.FromArgb(0, 99, 177);  // Darker blue
        public static readonly Color Secondary = Color.FromArgb(118, 185, 0);    // Green
        public static readonly Color Background = Color.FromArgb(243, 243, 243); // Light gray
        public static readonly Color Surface = Color.White;
        public static readonly Color Error = Color.FromArgb(232, 17, 35);        // Red
        public static readonly Color TextPrimary = Color.FromArgb(32, 32, 32);   // Dark gray
        public static readonly Color TextSecondary = Color.FromArgb(96, 96, 96); // Medium gray
        public static readonly Color Border = Color.FromArgb(200, 200, 200);     // Light border
        public static readonly Color Highlight = Color.FromArgb(255, 251, 230);  // Light yellow
    }

    // Spacing
    public const int PaddingSmall = 5;
    public const int PaddingMedium = 10;
    public const int PaddingLarge = 20;

    // Border Radius (for custom controls)
    public const int BorderRadius = 4;

    // Fonts
    public static readonly Font HeaderFont = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font NormalFont = new("Segoe UI", 9F);
    public static readonly Font SmallFont = new("Segoe UI", 8F);
    public static readonly Font LargeFont = new("Segoe UI", 11F);
}

