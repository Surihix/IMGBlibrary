using IMGBlibrary.Support;
using System;
using System.IO;

namespace IMGBlibrary.Unpack
{
    internal class IMGBUnpackTypes
    {
        #region Classic type
        public static void UnpackClassic(string imgHeaderBlockFile, string extractIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileName(imgHeaderBlockFile);

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {

                    var currentDDSfile = Path.Combine(extractIMGBdir, imgHeaderBlockFileName + ".dds");

                    using (var ddsStream = new FileStream(currentDDSfile, FileMode.Append, FileAccess.Write))
                    {
                        using (var ddsWriter = new BinaryWriter(ddsStream))
                        {
                            DDSMethods.BaseHeader(ddsStream, ddsWriter, imgbVars);
                            DDSMethods.PixelFormatHeader(ddsWriter, imgbVars);

                            gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                            var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);


                            uint mipOffsetsReadStartPos = imgbVars.GtexStartVal + mipOffsetsStartPos;
                            for (int m = 0; m < imgbVars.GtexImgMipCount; m++)
                            {
                                gtexReader.BaseStream.Position = mipOffsetsReadStartPos;
                                var mipStart = gtexReader.ReadBytesUInt32(true);

                                gtexReader.BaseStream.Position = mipOffsetsReadStartPos + 4;
                                var mipSize = gtexReader.ReadBytesUInt32(true);

                                imgbStream.Seek(mipStart, SeekOrigin.Begin);
                                var copyMipAt = ddsStream.Length;
                                ddsStream.Seek(copyMipAt, SeekOrigin.Begin);

                                CopyMipToDDS(imgbVars, mipSize, imgbStream, ddsStream, mipStart);

                                mipOffsetsReadStartPos += 8;
                            }
                        }
                    }

                    SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);
                }
            }
        }
        #endregion


        #region Cubemap type
        public static void UnpackCubemap(string imgHeaderBlockFile, string extractIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileName(imgHeaderBlockFile);

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {

                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                    var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);

                    uint mipOffsetsReadStartPos = imgbVars.GtexStartVal + mipOffsetsStartPos;


                    int cubeMapCount = 1;
                    for (int cb = 0; cb < 6; cb++)
                    {
                        var currentDDSfile = Path.Combine(extractIMGBdir, imgHeaderBlockFileName + imgbVars.GtexImgType + cubeMapCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Append, FileAccess.Write))
                        {
                            using (var ddsWriter = new BinaryWriter(ddsStream))
                            {

                                DDSMethods.BaseHeader(ddsStream, ddsWriter, imgbVars);
                                DDSMethods.PixelFormatHeader(ddsWriter, imgbVars);

                                for (int m = 0; m < imgbVars.GtexImgMipCount; m++)
                                {
                                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos;
                                    var mipStart = gtexReader.ReadBytesUInt32(true);

                                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos + 4;
                                    var mipSize = gtexReader.ReadBytesUInt32(true);

                                    var writeMipDataAt = ddsStream.Length;
                                    ddsStream.Seek(writeMipDataAt, SeekOrigin.Begin);

                                    CopyMipToDDS(imgbVars, mipSize, imgbStream, ddsStream, mipStart);

                                    mipOffsetsReadStartPos += 8;
                                }
                            }
                        }

                        SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);

                        cubeMapCount++;

                        gtexReader.BaseStream.Position = mipOffsetsReadStartPos;
                        mipOffsetsReadStartPos = (uint)gtexReader.BaseStream.Position;
                    }
                }
            }
        }
        #endregion


        #region Stack type
        public static void UnpackStack(string imgHeaderBlockFile, string extractIMGBdir, IMGBVariables imgbVars, FileStream imgbStream)
        {
            var imgHeaderBlockFileName = Path.GetFileName(imgHeaderBlockFile);

            using (var gtexStream = new FileStream(imgHeaderBlockFile, FileMode.Open, FileAccess.Read))
            {
                using (var gtexReader = new BinaryReader(gtexStream))
                {

                    gtexReader.BaseStream.Position = imgbVars.GtexStartVal + 16;
                    var mipOffsetsStartPos = gtexReader.ReadBytesUInt32(true);

                    var mipOffsetsReadStartPos = imgbVars.GtexStartVal + mipOffsetsStartPos;

                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos;
                    var mipStart = gtexReader.ReadBytesUInt32(true);

                    gtexReader.BaseStream.Position = mipOffsetsReadStartPos + 4;
                    var mipSize = gtexReader.ReadBytesUInt32(true);


                    int stackCount = 1;
                    mipSize /= 4;
                    for (int st = 0; st < imgbVars.GtexImgDepth; st++)
                    {
                        var currentDDSfile = Path.Combine(extractIMGBdir, imgHeaderBlockFileName + imgbVars.GtexImgType + stackCount + ".dds");

                        using (var ddsStream = new FileStream(currentDDSfile, FileMode.Append, FileAccess.Write))
                        {
                            using (var ddsWriter = new BinaryWriter(ddsStream))
                            {

                                DDSMethods.BaseHeader(ddsStream, ddsWriter, imgbVars);
                                DDSMethods.PixelFormatHeader(ddsWriter, imgbVars);

                                var writeMipDataAt = ddsStream.Length;
                                ddsStream.Seek(writeMipDataAt, SeekOrigin.Begin);

                                CopyMipToDDS(imgbVars, mipSize, imgbStream, ddsStream, mipStart);

                                var nextStackImgStart = mipStart + mipSize;
                                mipStart = nextStackImgStart;
                            }
                        }

                        SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);

                        stackCount++;
                    }
                }
            }
        }
        #endregion


        #region Common methods
        private static void CopyMipToDDS(IMGBVariables imgbVars, uint mipSize, FileStream imgbStream, FileStream ddsStream, uint mipStart)
        {
            // Set a bool to indicate whether to copy 
            // dds data or not
            var doneCopying = false;

            if (imgbVars.IsPs3Imgb)
            {
                imgbStream.Position = mipStart;
                SpecialPS3ImgMethods(ref doneCopying, imgbVars, mipSize, imgbStream, ddsStream);
            }

            // If the condition matches a win32 image file or a pixel format
            // that does not need anything specific done, then copy the data
            // directly to the final dds file.
            if (!doneCopying)
            {
                imgbStream.ExCopyTo(ddsStream, mipStart, mipSize);
            }
        }


        private static void SpecialPS3ImgMethods(ref bool doneCopying, IMGBVariables imgbVars, uint mipSize, FileStream imgbStream, FileStream ddsStream)
        {
            // If the conditions match a swizzled ps3 image,
            // then unswizzle the image data, color correct the data,
            // and copy the unswizzled image data to the final dds file.
            var isSwizzled = false;
            if (imgbVars.GtexImgFormatValue == 4 && imgbVars.GtexImgTypeValue == 4)
            {
                isSwizzled = true;
            }
            if (!doneCopying && isSwizzled)
            {
                var swizzledArray = new byte[mipSize];
                imgbStream.Read(swizzledArray, 0, swizzledArray.Length);

                var unSwizzledArray = MortonUnswizzle(imgbVars, swizzledArray);
                var correctedColorArray = ColorAsBGRA(unSwizzledArray);

                ddsStream.Write(correctedColorArray, 0, correctedColorArray.Length);
                doneCopying = true;
            }

            // If the conditions match a ps3 pixel format 4 image
            // "without the swizzle type flag", then color correct the
            // data and copy the data to the final dds file.
            var format4NoSwizzleFlag = false;
            if (imgbVars.GtexImgFormatValue == 4 && imgbVars.GtexImgTypeValue == 0)
            {
                format4NoSwizzleFlag = true;
            }
            if (!doneCopying && format4NoSwizzleFlag)
            {
                var colorDataToCorrectArray = new byte[mipSize];
                imgbStream.Read(colorDataToCorrectArray, 0, colorDataToCorrectArray.Length);

                var correctedColorArray = ColorAsBGRA(colorDataToCorrectArray);

                ddsStream.Write(correctedColorArray, 0, correctedColorArray.Length);
                doneCopying = true;
            }

            // If the conditions match a ps3 pixel format 3 or 4 image,
            // then color correct the data and copy the data to the 
            // final dds file.
            if (!doneCopying && imgbVars.GtexImgFormatValue == 3 || !doneCopying && imgbVars.GtexImgFormatValue == 4)
            {
                var colorDataToCorrectArray = new byte[mipSize];
                imgbStream.Read(colorDataToCorrectArray, 0, colorDataToCorrectArray.Length);

                var correctedColorArray = ColorAsBGRA(colorDataToCorrectArray);

                ddsStream.Write(correctedColorArray, 0, correctedColorArray.Length);
                doneCopying = true;
            }
        }


        private static byte[] MortonUnswizzle(IMGBVariables imgbVars, byte[] swizzledBufferVar)
        {
            int widthVar = imgbVars.GtexImgWidth;
            int heightVar = imgbVars.GtexImgHeight;

            var unswizzledBufferVar = new byte[widthVar * heightVar * 4];
            var processBufferVar = new byte[4];

            var arrayReadPos = 0;
            for (int m = 0; m < widthVar * heightVar; m++)
            {
                Array.Copy(swizzledBufferVar, arrayReadPos, processBufferVar, 0, 4);

                int val1 = 0;
                int val2 = 0;
                int val3;
                int val4 = (val3 = 1);
                int val5 = m;
                int val6 = widthVar;
                int val7 = heightVar;

                while (val6 > 1 || val7 > 1)
                {
                    if (val6 > 1)
                    {
                        val1 += val4 * (val5 & 1);
                        val5 >>= 1;
                        val4 *= 2;
                        val6 >>= 1;
                    }
                    if (val7 > 1)
                    {
                        val2 += val3 * (val5 & 1);
                        val5 >>= 1;
                        val3 *= 2;
                        val7 >>= 1;
                    }
                }

                var processedPixel = val2 * widthVar + val1;
                int pixelOffset = processedPixel * 4;

                Array.Copy(processBufferVar, 0, unswizzledBufferVar, pixelOffset, 4);

                arrayReadPos += 4;
            }

            return unswizzledBufferVar;
        }


        private static byte[] ColorAsBGRA(byte[] unSwizzledBufferVar)
        {
            var correctedColors = new byte[unSwizzledBufferVar.Length];

            using (MemoryStream unSwizzledStream = new MemoryStream())
            {
                unSwizzledStream.Write(unSwizzledBufferVar, 0, unSwizzledBufferVar.Length);
                unSwizzledStream.Seek(0, SeekOrigin.Begin);

                using (BinaryReader unSwizzledReader = new BinaryReader(unSwizzledStream))
                {

                    using (MemoryStream adjustedColorStream = new MemoryStream())
                    {
                        adjustedColorStream.Write(unSwizzledBufferVar, 0, unSwizzledBufferVar.Length);
                        adjustedColorStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter adjustedColorWriter = new BinaryWriter(adjustedColorStream))
                        {

                            var readPos = 0;
                            var writePos = 0;

                            for (int p = 0; p < unSwizzledBufferVar.Length; p++)
                            {
                                unSwizzledReader.BaseStream.Position = readPos;
                                var alpha = unSwizzledReader.ReadByte();
                                var red = unSwizzledReader.ReadByte();
                                var green = unSwizzledReader.ReadByte();
                                var blue = unSwizzledReader.ReadByte();

                                adjustedColorWriter.BaseStream.Position = writePos;
                                adjustedColorWriter.Write(blue);
                                adjustedColorWriter.Write(green);
                                adjustedColorWriter.Write(red);
                                adjustedColorWriter.Write(alpha);

                                if (readPos < (unSwizzledBufferVar.Length - 4))
                                {
                                    readPos += 4;
                                    writePos += 4;
                                }
                            }

                            adjustedColorStream.Seek(0, SeekOrigin.Begin);
                            adjustedColorStream.Read(correctedColors, 0, correctedColors.Length);
                        }
                    }
                }
            }

            return correctedColors;
        }
        #endregion
    }
}