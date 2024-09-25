namespace IMGBlibrary.Support
{
    public class IMGBVariables
    {
        #region Process variables
        public bool ShowLog { get; set; }
        public string ImgHeaderBlockFileName { get; set; }
        public bool IsWin32Imgb { get; set; }
        public bool IsPs3Imgb { get; set; }
        public bool IsX360Imgb { get; set; }
        #endregion

        #region GTEX variables
        public static readonly byte[] GtexImgFormatValues = new byte[] { 3, 4, 24, 25, 26 };  
        public static readonly byte[] GtexImgTypeValues = new byte[] { 0, 4, 1, 5, 2 };
        public uint GtexStartVal { get; set; }
        public byte GtexImgFormatValue { get; set; }
        public byte GtexImgMipCount { get; set; }
        public byte GtexImgTypeValue { get; set; }
        public string GtexImgType { get; set; }
        public ushort GtexImgWidth { get; set; }
        public ushort GtexImgHeight { get; set; }
        public ushort GtexImgDepth { get; set; }
        #endregion

        #region DDS to GTEX variables
        public uint OutImgWidth { get; set; }
        public uint OutImgHeight { get; set; }
        public uint OutImgMipCount { get; set; }       
        public byte OutImgFormatValue { get; set; }
        #endregion
    }
}