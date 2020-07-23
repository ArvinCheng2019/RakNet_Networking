using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
internal struct UIntDouble
{
    [FieldOffset(0)]
    public double doubleValue;

    [FieldOffset(0)]
    public ulong longValue;
}