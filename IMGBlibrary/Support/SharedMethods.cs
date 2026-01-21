using System;
using System.IO;
using System.Linq;
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

        public static GTEX GetGTEXInfo(string inImgHeaderBlockFile)
        {
            var gtex = new GTEX()
            {
                ImageName = Path.GetFileNameWithoutExtension(inImgHeaderBlockFile)
            };

            var offsetFound = GetGTEXChunkOffset(inImgHeaderBlockFile);

            if (offsetFound == -1)
            {
                gtex.IsValid = false;
                return gtex;
            }

            gtex.IsValid = true;
            gtex.GTEXOffset = (uint)offsetFound;

            GetGTEXData(inImgHeaderBlockFile, gtex);

            return gtex;
        }

        private static int GetGTEXChunkOffset(string inImgHeaderBlockFile)
        {
            int offset = -1;
            const string gtexChunkMagic = "GTEX";

            var readBuffer = new byte[4];
            var headerBlockBuffer = File.ReadAllBytes(inImgHeaderBlockFile);
            var limitPos = headerBlockBuffer.Length - 3;

            for (int i = 0; i < headerBlockBuffer.Length; i++)
            {
                if (i != limitPos)
                {
                    Array.ConstrainedCopy(headerBlockBuffer, i, readBuffer, 0, 4);

                    if (Encoding.ASCII.GetString(readBuffer) == gtexChunkMagic)
                    {
                        offset = i;
                        break;
                    }
                }
            }

            return offset;
        }

        private static void GetGTEXData(string inImgHeaderBlockFile, GTEX gtex)
        {
            using (var gtexReader = new BinaryReader(File.Open(inImgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                _ = gtexReader.BaseStream.Position = gtex.GTEXOffset + 4;
                gtex.Version = gtexReader.ReadByte();
                gtex.UnkFlag = gtexReader.ReadByte();
                gtex.Format = gtexReader.ReadByte();
                gtex.MipCount = gtexReader.ReadByte();
                gtex.UnkFlag2 = gtexReader.ReadByte();
                gtex.Type = gtexReader.ReadByte();
                gtex.Width = gtexReader.ReadBytesUInt16(true);
                gtex.Height = gtexReader.ReadBytesUInt16(true);
                gtex.Depth = gtexReader.ReadBytesUInt16(true);
                gtex.MipInfoTableOffset = gtexReader.ReadBytesUInt32(true);
            }
        }


        private static readonly byte[] GTEXFormatValues = new byte[] { 3, 4, 24, 25, 26 };
        private static readonly byte[] GTEXTypeValues = new byte[] { 0, 4, 1, 5, 2 };
        public static bool CheckGTEXFormatAndType(GTEX gtex)
        {
            if (GTEXFormatValues.Contains(gtex.Format) && GTEXTypeValues.Contains(gtex.Type))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}