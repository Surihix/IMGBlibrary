using IMGBlibrary.Support;
using System.IO;

namespace IMGBlibrary.Unpack
{
    internal class IMGBUnpackTypes
    {
        public static IMGBFlags.Platforms Platform { get; set; }

        #region Classic type
        public static void UnpackClassic(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                var currentDDSfile = Path.Combine(extractIMGBdir, gtex.ImageName + ".dds");

                using (var ddsWriter = new BinaryWriter(File.Open(currentDDSfile, FileMode.Append, FileAccess.Write)))
                {
                    ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                    gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                    for (int i = 0; i < gtex.MipCount; i++)
                    {
                        var mipStart = gtexReader.ReadBytesUInt32(true);
                        var mipSize = gtexReader.ReadBytesUInt32(true);

                        CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);
                    }
                }

                SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);
            }
        }
        #endregion


        #region Cubemap type
        public static void UnpackCubemap(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                var currentDDSfile = Path.Combine(extractIMGBdir, gtex.ImageName + "_cbmap.dds");

                using (var ddsWriter = new BinaryWriter(File.Open(currentDDSfile, FileMode.Append, FileAccess.Write)))
                {
                    ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                    gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;

                    for (int i = 0; i < gtex.MipCount * 6; i++)
                    {
                        var mipStart = gtexReader.ReadBytesUInt32(true);
                        var mipSize = gtexReader.ReadBytesUInt32(true);

                        CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);
                    }
                }

                SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);
            }
        }
        #endregion


        #region Volumemap type
        public static void UnpackVolumemap(string imgHeaderBlockFile, string extractIMGBdir, GTEX gtex, FileStream imgbStream)
        {
            if (gtex.MipCount > 1)
            {
                SharedMethods.DisplayLogMessage("Detected more than one mip. mip 0 alone would be unpacked", true);
            }

            using (var gtexReader = new BinaryReader(File.Open(imgHeaderBlockFile, FileMode.Open, FileAccess.Read)))
            {
                var currentDDSfile = Path.Combine(extractIMGBdir, gtex.ImageName + "_volume.dds");

                using (var ddsWriter = new BinaryWriter(File.Open(currentDDSfile, FileMode.Append, FileAccess.Write)))
                {
                    ddsWriter.Write(DDSHelpers.GetDDSHeader(gtex));

                    gtexReader.BaseStream.Position = gtex.GTEXOffset + gtex.MipInfoTableOffset;
                    var mipStart = gtexReader.ReadBytesUInt32(true);
                    var mipSize = gtexReader.ReadBytesUInt32(true);

                    CopyMipToDDS(gtex, mipStart, mipSize, imgbStream, ddsWriter);
                }

                SharedMethods.DisplayLogMessage("Unpacked " + currentDDSfile, true);
            }
        }
        #endregion


        #region Shared
        private static void CopyMipToDDS(GTEX gtex, uint mipStart, uint mipSize, FileStream imgbStream, BinaryWriter ddsWriter)
        {
            var doneCopying = false;

            if (Platform == IMGBFlags.Platforms.ps3)
            {
                imgbStream.Position = mipStart;
                PS3UnpackHelpers.ProcessPS3ImageData(ref doneCopying, gtex, mipSize, imgbStream, ddsWriter);
            }

            // If the condition matches a win32 image file or a pixel format
            // that does not need anything specific done, then copy the data
            // directly to the final dds file.
            if (!doneCopying)
            {
                var currentMip = new byte[(int)mipSize];

                imgbStream.Seek(mipStart, SeekOrigin.Begin);
                _ = imgbStream.Read(currentMip, 0, (int)mipSize);
                ddsWriter.Write(currentMip);
            }
        }
        #endregion
    }
}