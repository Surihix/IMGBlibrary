using System;
using System.IO;
using System.Text;

namespace IMGBlibrary.Support
{
    internal class SharedMethods
    {
        public static void DisplayLogMessage(string message, bool showMsg)
        {
            if (showMsg)
            {
                Console.WriteLine(message);
            }
        }


        public static uint GetGTEXChunkPos(string inImgHeaderBlockFile)
        {
            uint gtexPos = 0;
            var gtexChunkString = "GTEX";
            var gtexChunkStringArray = new byte[4];
            var imgHeaderBlockFileData = File.ReadAllBytes(inImgHeaderBlockFile);

            for (int g = 0; g < imgHeaderBlockFileData.Length; g++)
            {
                if ((char)imgHeaderBlockFileData[g] == gtexChunkString[0])
                {
                    Buffer.BlockCopy(imgHeaderBlockFileData, g, gtexChunkStringArray, 0, 4);
                    var gtex = Encoding.ASCII.GetString(gtexChunkStringArray, 0, 4);

                    if (gtex == gtexChunkString)
                    {
                        gtexPos = (uint)g;
                        break;
                    }
                }
            }

            return gtexPos;
        }


        public static void GetImageInfo(string inImgHeaderBlockFile, IMGBVariables imgbVars)
        {
            using (var gtexStream = new FileStream(inImgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {
                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 6;
                    imgbVars.GtexImgFormatValue = gtexReader.ReadByte();
                    imgbVars.GtexImgMipCount = gtexReader.ReadByte();

                    imgbVars.GtexImgMipCount = imgbVars.GtexImgMipCount.Equals(0) ? (byte)1 : imgbVars.GtexImgMipCount;

                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 9;
                    imgbVars.GtexImgTypeValue = gtexReader.ReadByte();
                    imgbVars.GtexImgWidth = gtexReader.ReadBytesUInt16(true);
                    imgbVars.GtexImgHeight = gtexReader.ReadBytesUInt16(true);
                    imgbVars.GtexImgDepth = gtexReader.ReadBytesUInt16(true);

                    switch (imgbVars.GtexImgTypeValue)
                    {
                        case 1:
                        case 5:
                            imgbVars.GtexImgType = "_cbmap_";
                            break;

                        case 2:
                            imgbVars.GtexImgType = "_stack_";
                            break;
                    }
                }
            }
        }


        public static void GetExtImgInfo(BinaryReader ddsReader, IMGBVariables imgbVars)
        {
            ddsReader.BaseStream.Position = 12;
            imgbVars.OutImgHeight = ddsReader.ReadUInt32();
            imgbVars.OutImgWidth = ddsReader.ReadUInt32();

            ddsReader.BaseStream.Position = 28;
            imgbVars.OutImgMipCount = ddsReader.ReadUInt32();

            ddsReader.BaseStream.Position = 84;
            var imgFormatString = Encoding.ASCII.GetString(ddsReader.ReadBytes(4)).Replace("\0", "");

            switch (imgFormatString)
            {
                case "":
                    if (imgbVars.OutImgMipCount > 1)
                    {
                        imgbVars.OutImgFormatValue = 3;
                    }
                    else
                    {
                        imgbVars.OutImgFormatValue = 4;
                    }
                    break;

                case "DXT1":
                    imgbVars.OutImgFormatValue = 24;
                    break;

                case "DXT3":
                    imgbVars.OutImgFormatValue = 25;
                    break;

                case "DXT5":
                    imgbVars.OutImgFormatValue = 26;
                    break;

                default:
                    imgbVars.OutImgFormatValue = 0;
                    break;
            }
        }


        public static bool CheckImgFilesBatch(int fileAmount, string extractImgbDir, string imgHeaderBlockFileName, IMGBVariables imgbVars)
        {
            var isMissingAnImg = false;
            var imgFileCount = 1;

            for (int i = 0; i < fileAmount; i++)
            {
                var fileToCheck = Path.Combine(extractImgbDir, imgHeaderBlockFileName + imgbVars.GtexImgType + imgFileCount + ".dds");

                if (!File.Exists(fileToCheck))
                {
                    isMissingAnImg = true;
                }

                imgFileCount++;
            }

            return isMissingAnImg;
        }
    }
}