using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
internal struct UIntFloat
{
    [FieldOffset(0)]
    public float floatValue;
    [FieldOffset(0)]
    public uint intValue;
}