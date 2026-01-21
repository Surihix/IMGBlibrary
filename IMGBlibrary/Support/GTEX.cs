namespace IMGBlibrary.Support
{
    internal class GTEX
    {
        public string ImageName;
        public bool IsValid;
        public uint GTEXOffset;
        public byte Version;
        public byte UnkFlag;
        public byte Format;
        public byte MipCount;
        public byte UnkFlag2;
        public byte Type;
        public ushort Width;
        public ushort Height;
        public ushort Depth;
        public uint MipInfoTableOffset;
    }
}