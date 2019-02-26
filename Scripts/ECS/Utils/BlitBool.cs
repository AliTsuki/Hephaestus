public struct BlitBool
{
    public readonly byte Value;

    public BlitBool(byte value)
    {
        this.Value = value;
    }

    public BlitBool(bool value)
    {
        this.Value = value ? (byte)1 : (byte)0;
    }

    public static implicit operator bool(BlitBool bb)
    {
        return bb.Value != 0;
    }

    public static implicit operator BlitBool(bool b)
    {
        return new BlitBool(b);
    }
}