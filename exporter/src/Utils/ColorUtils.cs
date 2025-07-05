using System.Drawing;

public static class ColorUtils
{
	public static string ColorToRGB(Color color)
	{
		return $"0x{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	public static int ColorToArgb(Color color)
	{
		return color.ToArgb();
	}
}
