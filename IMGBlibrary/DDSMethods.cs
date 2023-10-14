using BinaryWriterEx;
using System;
using System.IO;

namespace IMGBlibrary
{
    public partial class ImageMethods
    {
        public static void BaseHeader(FileStream ddsFileVar, BinaryWriter ddsWriterVar, ImageMethods imgbVars)
        {
            uint mipCountVar = Convert.ToUInt32(imgbVars.GtexImgMipCount);

            for (int h = 0; h < 128; h++)
            {
                ddsFileVar.WriteByte(0);
            }

            // Writes common DDS header flags
            ddsWriterVar.BaseStream.Position = 0;
            ddsWriterVar.WriteBytesUInt32(542327876, false);

            ddsWriterVar.BaseStream.Position = 4;
            ddsWriterVar.WriteBytesUInt32(124, false);

            ddsWriterVar.BaseStream.Position = 12;
            ddsWriterVar.WriteBytesUInt32(imgbVars.GtexImgHeight, false);

            ddsWriterVar.BaseStream.Position = 16;
            ddsWriterVar.WriteBytesUInt32(imgbVars.GtexImgWidth, false);

            ddsWriterVar.BaseStream.Position = 28;
            ddsWriterVar.WriteBytesUInt32(mipCountVar, false);

            ddsWriterVar.BaseStream.Position = 76;
            ddsWriterVar.WriteBytesUInt32(32, false);

            // Writes the mip related flag
            // if mip count is more than one
            ddsWriterVar.BaseStream.Position = 108;

            if (mipCountVar > 1)
            {
                ddsWriterVar.WriteBytesUInt32(4198408, false);
            }
            else
            {
                ddsWriterVar.WriteBytesUInt32(4096, false);
            }
        }

        public static void PixelFormatHeader(BinaryWriter ddsWriterVar, ImageMethods imgbVars)
        {
            uint imgFormat = Convert.ToUInt32(imgbVars.GtexImgFormatValue);
            uint imgWidth = imgbVars.GtexImgWidth;
            uint imgHeight = imgbVars.GtexImgHeight;
            uint mipCount = Convert.ToUInt32(imgbVars.GtexImgMipCount);

            // Computes DDS pitch and writes DXT string
            // chars for BC pixel formats
            uint pitch = 0;

            // Move the basestream position
            // for the pixel format chars
            ddsWriterVar.BaseStream.Position = 84;
            switch (imgFormat)
            {
                case 3:     // R8G8B8A8 (with mips)
                case 4:     // R8G8B8A8
                    pitch = (imgWidth * 32 + 7) / 8;
                    break;

                case 24:     // DXT1
                    pitch = Math.Max(1, ((imgWidth + 3) / 4)) * Math.Max(1, ((imgHeight + 3) / 4)) * 8;
                    ddsWriterVar.WriteBytesUInt32(827611204, false);     // writes DXT1 string
                    break;

                case 25:     // DXT3
                    pitch = Math.Max(1, ((imgWidth + 3) / 4)) * Math.Max(1, ((imgHeight + 3) / 4)) * 16;
                    ddsWriterVar.WriteBytesUInt32(861165636, false);     // writes DXT3 string
                    break;

                case 26:     // DXT5             
                    pitch = Math.Max(1, ((imgWidth + 3) / 4)) * Math.Max(1, ((imgHeight + 3) / 4)) * 16;
                    ddsWriterVar.WriteBytesUInt32(894720068, false);     // writes DXT5 string
                    break;
            }

            ddsWriterVar.BaseStream.Position = 20;
            ddsWriterVar.WriteBytesUInt32(pitch, false);

            // Writes pixel format flags which are
            // common for R8G8B8A8
            if (imgFormat.Equals(3) || imgFormat.Equals(4))
            {
                ddsWriterVar.BaseStream.Position = 80;
                ddsWriterVar.WriteBytesUInt32(65, false);

                ddsWriterVar.BaseStream.Position = 88;
                ddsWriterVar.WriteBytesUInt32(32, false);

                ddsWriterVar.BaseStream.Position = 92;
                ddsWriterVar.WriteBytesUInt32(16711680, false);

                ddsWriterVar.BaseStream.Position = 96;
                ddsWriterVar.WriteBytesUInt32(65280, false);

                ddsWriterVar.BaseStream.Position = 100;
                ddsWriterVar.WriteBytesUInt32(255, false);

                ddsWriterVar.BaseStream.Position = 104;
                ddsWriterVar.WriteBytesUInt32(4278190080, false);

                ddsWriterVar.BaseStream.Position = 8;
                if (mipCount > 1)
                {
                    ddsWriterVar.WriteBytesUInt32(135183, false);
                }
                else
                {
                    ddsWriterVar.WriteBytesUInt32(4111, false);
                }
            }

            // Writes pixel format flags which are
            // common for DXT1, DXT3, and DXT5
            if (imgFormat.Equals(24) || imgFormat.Equals(25) || imgFormat.Equals(26))
            {
                ddsWriterVar.BaseStream.Position = 80;
                ddsWriterVar.WriteBytesUInt32(4, false);

                ddsWriterVar.BaseStream.Position = 8;
                if (mipCount > 1)
                {
                    ddsWriterVar.WriteBytesUInt32(659463, false);
                }
                else
                {
                    ddsWriterVar.WriteBytesUInt32(528391, false);
                }
            }
        }
    }
}