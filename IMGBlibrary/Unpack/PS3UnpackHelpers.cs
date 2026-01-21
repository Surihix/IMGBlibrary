using IMGBlibrary.Support;
using System;
using System.IO;

namespace IMGBlibrary.Unpack
{
    internal class PS3UnpackHelpers
    {
        public static void ProcessPS3ImageData(ref bool doneCopying, GTEX gtex, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            // If the conditions match a swizzled ps3 image,
            // then unswizzle the image data, color correct the data,
            // and copy the unswizzled image data to the final dds file.
            var isSwizzled = false;
            if (gtex.Format == 4 && gtex.Type == 4)
            {
                isSwizzled = true;
            }
            if (!doneCopying && isSwizzled)
            {
                var swizzledArray = new byte[mipSize];
                imgbStream.Read(swizzledArray, 0, swizzledArray.Length);

                var unSwizzledArray = MortonUnswizzle(gtex, swizzledArray);
                var correctedColorArray = ColorAsBGRA(unSwizzledArray);

                ddsWriter.Write(correctedColorArray);
                doneCopying = true;
            }

            // If the conditions match a ps3 pixel format 4 image
            // "without the swizzle type flag", then color correct the
            // data and copy the data to the final dds file.
            var format4NoSwizzleFlag = false;
            if (gtex.Format == 4 && gtex.Type == 0)
            {
                format4NoSwizzleFlag = true;
            }
            if (!doneCopying && format4NoSwizzleFlag)
            {
                var colorDataToCorrectArray = new byte[mipSize];
                imgbStream.Read(colorDataToCorrectArray, 0, colorDataToCorrectArray.Length);

                var correctedColorArray = ColorAsBGRA(colorDataToCorrectArray);

                ddsWriter.Write(correctedColorArray);
                doneCopying = true;
            }

            // If the conditions match a ps3 pixel format 3 or 4 image,
            // then color correct the data and copy the data to the 
            // final dds file.
            if (!doneCopying && gtex.Format == 3 || !doneCopying && gtex.Format == 4)
            {
                var colorDataToCorrectArray = new byte[mipSize];
                imgbStream.Read(colorDataToCorrectArray, 0, colorDataToCorrectArray.Length);

                var correctedColorArray = ColorAsBGRA(colorDataToCorrectArray);

                ddsWriter.Write(correctedColorArray);
                doneCopying = true;
            }
        }

        private static byte[] MortonUnswizzle(GTEX gtex, byte[] swizzledBufferVar)
        {
            int widthVar = gtex.Width;
            int heightVar = gtex.Height;

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
    }
}