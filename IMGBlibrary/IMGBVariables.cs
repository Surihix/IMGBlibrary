namespace IMGBlibrary
{
    public class IMGBVariables
    {
        public static readonly string[] ImgHeaderBlockExtns = new string[]
        {
            ".txbh",
            ".txb",
            ".vtex"
        };

        public uint GtexStartVal { get; set; }

        public byte GtexImgFormatValue { get; set; }
        public static readonly byte[] GtexImgFormatValuesArray = new byte[]
        {
            3, 4, 24, 25, 26
        };

        public byte GtexImgMipCount { get; set; }

        public byte GtexImgTypeValue { get; set; }
        public static readonly byte[] GtexImgTypeValuesArray = new byte[]
        {
            0, 4, 1, 5, 2
        };

        public string GtexImgType { get; set; }

        public ushort GtexImgWidth { get; set; }

        public ushort GtexImgHeight { get; set; }

        public ushort GtexImgDepth { get; set; }

        public bool GtexIsPs3Imgb { get; set; }

        public bool GtexIsX360Imgb { get; set; }

        public uint OutImgWidth { get; set; }

        public uint OutImgHeight { get; set; }

        public uint OutImgMipCount { get; set; }
        
        public byte OutImgFormatValue { get; set; }
    }
}