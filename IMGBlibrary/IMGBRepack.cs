using BinaryReaderEx;
using StreamExtension;
using System;
using System.IO;
using System.Linq;

namespace IMGBlibrary
{
    public class IMGBRepack
    {
        public static void RepackIMGBType1(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir)
        {
            var gtexPos = IMGBMethods.GetGTEXChunkPos(imgHeaderBlockFile);
            if (gtexPos == 0)
            {
                Console.WriteLine("Unable to find GTEX chunk. skipped to next file.");
                return;
            }

            var imgbVars = new IMGBVariables();
            imgbVars.GtexStartVal = gtexPos;

            IMGBMethods.GetImageInfo(imgHeaderBlockFile, imgbVars);

            if (!IMGBVariables.GtexImgFormatValuesArray.Contains(imgbVars.GtexImgFormatValue))
            {
                Console.WriteLine("Detected unknown image format. skipped to next file.");
                return;
            }

            if (!IMGBVariables.GtexImgTypeValuesArray.Contains(imgbVars.GtexImgTypeValue))
            {
                Console.WriteLine("Detected unknown image type. skipped to next file.");
                return;
            }


            // Open the IMGB file and start extracting
            // the images according to the image type
            using (var imgbStream = new FileStream(outImgbFile, FileMode.Open, FileAccess.Write))
            {

                switch (imgbVars.GtexImgTypeValue)
                {
                    // Classic or Other type
                    // Type 0 is Classic
                    // Type 4 is Other
                    case 0:
                    case 4:
                        RepackClassicType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;

                    // Cubemap type 
                    // Type 5 is for PS3
                    case 1:
                    case 5:
                        RepackCubemapType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;

                    // Stacked type (LR only)
                    // PC version wpd may or may not use
                    // this type.
                    case 2:
                        if (imgbVars.GtexImgMipCount > 1)
                        {
                            Console.WriteLine("Detected more than one mip in this stack type image. skipped to next file.");
                            return;
                        }
                        RepackStackType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;
                }
            }
        }



        // Classic type
        static void RepackClassicType1(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileNameWithoutExtension(imgHeaderBlockFile);
            var currentDDSfile = Path.Combine(extractedIMGBdir, imgHeaderBlockFileName + ".dds");

            if (!File.Exists(currentDDSfile))
            {
                Console.WriteLine("Missing image file. skipped to next file.");
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {
                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                    var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);

                    using (var ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                    {
                        using (var ddsReader = new BinaryReader(ddsStream))
                        {
                            IMGBMethods.GetExtImgInfo(ddsReader, imgbVars);
                            var isValidImg = CheckExtImgInfo(imgbVars);

                            if (!isValidImg)
                            {
                                return;
                            }

                            using (var tempDDSstream = new MemoryStream())
                            {
                                ddsStream.Seek(128, SeekOrigin.Begin);
                                ddsStream.CopyTo(tempDDSstream);


                                uint mipStart = 0;
                                uint nextMipStart = 0;
                                uint totalMipSize = 0;
                                uint readStart = imgbVars.GtexStartVal + mipOffsetsStartPos;

                                for (int m = 0; m < imgbVars.GtexImgMipCount; m++)
                                {
                                    gtexReader.BaseStream.Position = readStart;
                                    var copyMipAt = gtexReader.ReadBytesUInt32(true);

                                    gtexReader.BaseStream.Position = readStart + 4;
                                    var mipSize = gtexReader.ReadBytesUInt32(true);

                                    imgbStream.Seek(copyMipAt, SeekOrigin.Begin);
                                    tempDDSstream.ExCopyTo(imgbStream, mipStart, mipSize);

                                    gtexReader.BaseStream.Position = readStart + 8;
                                    readStart = (uint)gtexReader.BaseStream.Position;

                                    nextMipStart = mipSize + totalMipSize;
                                    mipStart = nextMipStart;
                                    totalMipSize = nextMipStart;
                                }
                            }
                        }
                    }

                    Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");
                }
            }
        }


        // Cubemap type
        static void RepackCubemapType1(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileNameWithoutExtension(imgHeaderBlockFile);

            var isMissingAnImg = IMGBMethods.CheckImgFilesBatch(6, extractedIMGBdir, imgHeaderBlockFileName, imgbVars);
            if (isMissingAnImg)
            {
                Console.WriteLine("Missing one or more cubemap type image files. skipped to next file.");
                return;
            }

            var isAllValidImg = CheckExtImgInfoBatch(6, extractedIMGBdir, imgHeaderBlockFileName, imgbVars);
            if (!isAllValidImg)
            {
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {

                    var cubeMapCount = 1;
                    uint readStart = 0;
                    var file1 = true;

                    for (int c = 0; c < 6; c++)
                    {
                        var currentDDSfile = Path.Combine(extractedIMGBdir, imgHeaderBlockFileName + imgbVars.GtexImgType + cubeMapCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                        {
                            using (var tempDDSstream = new MemoryStream())
                            {
                                ddsStream.Seek(128, SeekOrigin.Begin);
                                ddsStream.CopyTo(tempDDSstream);


                                uint mipStart = 0;
                                uint nextMipStart = 0;
                                uint totalMipSize = 0;

                                if (file1)
                                {
                                    readStart = imgbVars.GtexStartVal + 24;
                                }

                                for (int m = 0; m < imgbVars.GtexImgMipCount; m++)
                                {
                                    gtexReader.BaseStream.Position = readStart;
                                    uint copyMipAt = gtexReader.ReadBytesUInt32(true);

                                    gtexReader.BaseStream.Position = readStart + 4;
                                    uint mipSize = gtexReader.ReadBytesUInt32(true);

                                    imgbStream.Seek(copyMipAt, SeekOrigin.Begin);
                                    tempDDSstream.ExCopyTo(imgbStream, mipStart, mipSize);

                                    gtexReader.BaseStream.Position = readStart + 8;
                                    readStart = (uint)gtexReader.BaseStream.Position;

                                    nextMipStart = mipSize + totalMipSize;
                                    mipStart = nextMipStart;
                                    totalMipSize = nextMipStart;
                                }
                            }
                        }

                        file1 = false;

                        Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");

                        cubeMapCount++;
                    }
                }
            }
        }


        // Stack type
        static void RepackStackType1(string imgHeaderBlockFile, string extractedIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileNameWithoutExtension(imgHeaderBlockFile);

            var isMissingAnImg = IMGBMethods.CheckImgFilesBatch(imgbVars.GtexImgDepth, extractedIMGBdir, imgHeaderBlockFileName, imgbVars);
            if (isMissingAnImg)
            {
                Console.WriteLine("Missing one or more stack type image files. skipped to next file.");
                return;
            }

            var isAllValidImg = CheckExtImgInfoBatch(imgbVars.GtexImgDepth, extractedIMGBdir, imgHeaderBlockFileName, imgbVars);
            if (!isAllValidImg)
            {
                return;
            }

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {
                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                    var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);

                    var mipOffsetsReadStartPos = imgbVars.GtexStartVal + mipOffsetsStartPos;

                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos;
                    var copyMipAt = gtexReader.ReadBytesUInt32(true);

                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos + 4;
                    var mipSize = gtexReader.ReadBytesUInt32(true);


                    var stackCount = 1;
                    uint mipStart = 0;

                    for (int s = 0; s < imgbVars.GtexImgDepth; s++)
                    {
                        var currentDDSfile = Path.Combine(extractedIMGBdir, imgHeaderBlockFileName + imgbVars.GtexImgType + stackCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Open, FileAccess.Read))
                        {
                            using (var ddsReader = new BinaryReader(ddsStream))
                            {
                                IMGBMethods.GetExtImgInfo(ddsReader, imgbVars);

                                using (var tempDDSstream = new MemoryStream())
                                {
                                    ddsStream.Seek(128, SeekOrigin.Begin);
                                    ddsStream.CopyTo(tempDDSstream);

                                    imgbStream.Seek(copyMipAt, SeekOrigin.Begin);
                                    tempDDSstream.ExCopyTo(imgbStream, mipStart, mipSize);

                                    var NextStackImgStart = mipStart + mipSize;
                                    mipStart = NextStackImgStart;
                                }
                            }
                        }

                        Console.WriteLine("Repacked " + currentDDSfile + " data to IMGB.");

                        stackCount++;
                    }
                }
            }
        }



        // Common methods
        static bool CheckExtImgInfo(IMGBVariables imgbVars)
        {
            var isValidImg = true;
            if (imgbVars.GtexImgMipCount != imgbVars.OutImgMipCount)
            {
                Console.WriteLine("Current image's mip count does not match the original image's mip count. skipped to next file.");
                isValidImg = false;
            }

            if (imgbVars.GtexImgWidth != imgbVars.OutImgWidth)
            {
                Console.WriteLine("Current image's width does not match the original image's width. skipped to next file.");
                isValidImg = false;
            }

            if (imgbVars.GtexImgHeight != imgbVars.OutImgHeight)
            {
                Console.WriteLine("Current image's height does not match the original image's height. skipped to next file.");
                isValidImg = false;
            }

            if (imgbVars.GtexImgFormatValue != imgbVars.OutImgFormatValue)
            {
                Console.WriteLine("Detected unknown image file format. skipped to next file.");
                isValidImg = false;
            }

            return isValidImg;
        }


        static bool CheckExtImgInfoBatch(int fileAmount, string extractImgbDir, string imgHeaderBlockFileName, IMGBVariables imgbVars)
        {
            var isAllValidImg = true;
            var imgFileCount = 1;

            for (int i = 0; i < fileAmount; i++)
            {
                var fileToCheck = Path.Combine(extractImgbDir, imgHeaderBlockFileName + imgbVars.GtexImgType + imgFileCount + ".dds");

                using (var ddsFileToCheck = new FileStream(fileToCheck, FileMode.Open, FileAccess.Read))
                {
                    using (var ddsFileReader = new BinaryReader(ddsFileToCheck))
                    {
                        IMGBMethods.GetExtImgInfo(ddsFileReader, imgbVars);
                        var isValidImg = CheckExtImgInfo(imgbVars);

                        if (!isValidImg)
                        {
                            isAllValidImg = false;
                        }
                    }
                }

                imgFileCount++;
            }

            return isAllValidImg;
        }
    }
}