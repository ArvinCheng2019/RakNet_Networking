public static class FixedFloat
{
    public static float GetFloat(this ushort _short, ushort precision = 10)
    {
        return (_short / (float)precision);
    }

    public static ushort SetFloat(this float _float, ushort precision = 10)
    {
        return (ushort)(_float * precision);
    }

    public static float GetFloat(this byte _byte, ushort precision = 10)
    {
        return (_byte / (float)precision);
    }

    public static byte SetFloat(this float _float, byte precision = 10)
    {
        return (byte)(_float * precision);
    }
}
