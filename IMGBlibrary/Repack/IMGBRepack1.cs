using IMGBlibrary.Support;
using System.IO;

namespace IMGBlibrary.Repack
{
    /// <summary>
    /// Provides a type of method for repacking image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBRepack1
    {
        /// <summary>
        /// Use for repacking images that require the pixel format, 
        /// mipcount and dimensions to be same as the original.
        /// </summary>
        /// <param name="imgHeaderBlockFile">Header Block file path. should have the GTEX chunk.</param>
        /// <param name="outImgbFile">IMGB file path. the file has to be present.</param>
        /// <param name="extractedIMGBdir">Path to the directory where the image files are present.</param>
        /// <param name="imgbPlatform">Platform of the header block file.</param>
        public static void RepackIMGBType1(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir, IMGBFlags.Platforms imgbPlatform)
        {
            if (imgbPlatform == IMGBFlags.Platforms.ps3)
            {
                SharedMethods.DisplayLogMessage("Detected ps3 version image file. image repacking is not supported.", true);
                return;
            }

            if (imgbPlatform == IMGBFlags.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Detected xbox 360 version image file. image repacking is not supported.", true);
                return;
            }

            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockFile);

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image repacking.", true);
                return;
            }

            if (!SharedMethods.CheckGTEXFormatAndType(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown texture format or texture type. skipped image repacking.", true);
                return;
            }

            // Open the IMGB file and start repacking
            // the images according to the image type
            using (var imgbStream = new FileStream(outImgbFile, FileMode.Open, FileAccess.Write))
            {
                switch (gtex.Type)
                {
                    // Classic type
                    case 0:
                        IMGBRepack1Types.RepackClassic(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;

                    // Cubemap type 
                    case 1:
                        IMGBRepack1Types.RepackCubemap(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;

                    // Volumemap type
                    case 2:
                        IMGBRepack1Types.RepackVolumemap(imgHeaderBlockFile, extractedIMGBdir, gtex, imgbStream);
                        break;
                }
            }
        }
    }
}