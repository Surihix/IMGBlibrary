using IMGBlibrary.Support;
using System.Collections.Generic;
using System.IO;

namespace IMGBlibrary.Repack
{
    internal class IMGBRepack1Types
    {
        #region Classic type
        public static void RepackClassic(string imgHeaderBlockFile, string extractedIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var currentDDSfile = Path.Combine(extractedIMGBdir, gtex.ImageName + ".dds");

            if (!File.Exists(currentDDSfile))
            {
                SharedMethods.DisplayLogMessage("Missing image file. skipped repacking image." + currentDDSfile, true);
                return;
            }

            var gtexOffsettable = new List<(uint, uint)>();

            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                for (int i = 0; i < gtex.MipCount; i++)
                {
                    var mipStart = gtexReader.ReadBytesUInt32(true);
                    var mipSize = gtexReader.ReadBytesUInt32(true);

                    gtexOffsettable.Add((mipStart, mipSize));
                }
            }

            if (!CheckDDSImage(currentDDSfile, gtex))
            {
                return;
            }

            using (var imgbWriter = new BinaryWriter(imgbStream))
            {
                using (var ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
                {
                    _ = ddsReader.BaseStream.Position += 128;

                    for (int i = 0; i < gtex.MipCount; i++)
                    {
                        _ = imgbWriter.BaseStream.Seek(gtexOffsettable[i].Item1, SeekOrigin.Begin);

                        var currentMipData = ddsReader.ReadBytes((int)gtexOffsettable[i].Item2);
                        imgbWriter.Write(currentMipData);
                    }
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion


        #region Cubemap type
        public static void RepackCubemap(string imgHeaderBlockFile, string extractedIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            var currentDDSfile = Path.Combine(extractedIMGBdir, gtex.ImageName + "_cbmap.dds");

            if (!File.Exists(currentDDSfile))
            {
                SharedMethods.DisplayLogMessage("Missing image file. skipped repacking image." + currentDDSfile, true);
                return;
            }

            var gtexOffsettable = new List<(uint, uint)>();

            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                for (int j = 0; j < gtex.MipCount * 6; j++)
                {
                    var mipStart = gtexReader.ReadBytesUInt32(true);
                    var mipSize = gtexReader.ReadBytesUInt32(true);

                    gtexOffsettable.Add((mipStart, mipSize));
                }
            }

            if (!CheckDDSImage(currentDDSfile, gtex))
            {
                return;
            }

            using (var imgbWriter = new BinaryWriter(imgbStream))
            {
                using (var ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
                {
                    _ = ddsReader.BaseStream.Position += 128;

                    for (int i = 0; i < gtex.MipCount * 6; i++)
                    {
                        _ = imgbWriter.BaseStream.Seek(gtexOffsettable[i].Item1, SeekOrigin.Begin);

                        var currentMipData = ddsReader.ReadBytes((int)gtexOffsettable[i].Item2);
                        imgbWriter.Write(currentMipData);
                    }
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion


        #region Volumemap type
        public static void RepackVolumemap(string imgHeaderBlockFile, string extractedIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            if (gtex.MipCount > 1)
            {
                SharedMethods.DisplayLogMessage("Detected more than one mip. mip 0 alone would be repacked", true);
            }

            var currentDDSfile = Path.Combine(extractedIMGBdir, gtex.ImageName + "_volume.dds");

            if (!File.Exists(currentDDSfile))
            {
                SharedMethods.DisplayLogMessage("Missing image file. skipped repacking image." + currentDDSfile, true);
                return;
            }

            var gtexOffsettable = new List<(uint, uint)>();

            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;
                var mipStart = gtexReader.ReadBytesUInt32(true);
                var mipSize = gtexReader.ReadBytesUInt32(true);

                gtexOffsettable.Add((mipStart, mipSize));
            }

            if (!CheckDDSImage(currentDDSfile, gtex))
            {
                return;
            }

            using (var imgbWriter = new BinaryWriter(imgbStream))
            {
                using (var ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
                {
                    _ = ddsReader.BaseStream.Position += 128;
                    var currentMipData = ddsReader.ReadBytes((int)gtexOffsettable[0].Item2);

                    _ = imgbWriter.BaseStream.Seek(gtexOffsettable[0].Item1, SeekOrigin.Begin);
                    imgbWriter.Write(currentMipData);
                }
            }

            SharedMethods.DisplayLogMessage("Repacked " + currentDDSfile, true);
        }
        #endregion


        #region Shared
        private static bool CheckDDSImage(string currentDDSfile, GTEX gtex)
        {
            if (new FileInfo(currentDDSfile).Length < 128)
            {
                SharedMethods.DisplayLogMessage("DDS size is invalid. skipped repacking image.", true);
                return false;
            }

            using (var ddsReader = new BinaryReader(File.Open(currentDDSfile, FileMode.Open, FileAccess.Read)))
            {
                _ = ddsReader.BaseStream.Position = 12;
                var height = ddsReader.ReadUInt32();
                var width = ddsReader.ReadUInt32();

                if (gtex.Height != height)
                {
                    SharedMethods.DisplayLogMessage("Height in the dds file, does not match with the original image's height. skipped repacking image.", true);
                    return false;
                }

                if (gtex.Width != width)
                {
                    SharedMethods.DisplayLogMessage("Width in the dds file, does not match with the original image's width. skipped repacking image.", true);
                    return false;
                }

                _ = ddsReader.BaseStream.Position += 4;
                var depth = ddsReader.ReadUInt32();
                var mipCount = ddsReader.ReadUInt32();

                if (gtex.Type == 2 && gtex.Depth != depth)
                {
                    SharedMethods.DisplayLogMessage("Volume texture's depth count in the dds file, does not match with the original image's depth count. skipped repacking image.", true);
                    return false;
                }

                if (gtex.MipCount != mipCount)
                {
                    SharedMethods.DisplayLogMessage("Mipcount in the dds file, does not match with the original image's mipcount. skipped repacking image.", true);
                    return false;
                }

                _ = ddsReader.BaseStream.Position += 52;
                var ddsFourCC = ddsReader.ReadBytesString(4, false);

                var isValidPixelFormat = false;

                switch (ddsFourCC)
                {
                    case "":
                        if (mipCount > 1)
                        {
                            isValidPixelFormat = gtex.Format == 3;
                        }
                        else
                        {
                            isValidPixelFormat = gtex.Format == 4;
                        }
                        break;

                    case "DXT1":
                        isValidPixelFormat = gtex.Format == 24;
                        break;

                    case "DXT3":
                        isValidPixelFormat = gtex.Format == 25;
                        break;

                    case "DXT5":
                        isValidPixelFormat = gtex.Format == 26;
                        break;
                }

                if (!isValidPixelFormat)
                {
                    SharedMethods.DisplayLogMessage("Pixel format in the dds file is invalid. skipped repacking image.", true);
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}