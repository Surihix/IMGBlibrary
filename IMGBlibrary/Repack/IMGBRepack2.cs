using IMGBlibrary.Support;
using System.IO;

namespace IMGBlibrary.Repack
{
    /// <summary>
    /// Provides a type of method for repacking image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBRepack2
    {
        /// <summary>
        /// Use for repacking images that do not require the pixel format, 
        /// mipcount and dimensions to be same as the original image.
        /// </summary>
        /// <param name="imgHeaderBlockFile">Header Block file path. should have the GTEX chunk.</param>
        /// <param name="outImgbFile">IMGB file path. not mandatory for the file to be present.</param>
        /// <param name="extractedIMGBdir">Path to the directory where the image files are present.</param>
        /// <param name="imgbPlatform">Platform of the header block file.</param>
        public static void RepackIMGBType2(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir, IMGBFlags.Platforms imgbPlatform)
        {
            if (imgbPlatform == IMGBFlags.Platforms.ps3)
            {
                SharedMethods.DisplayLogMessage("Detected ps3 version image file. skipped image repacking.", true);
                return;
            }

            if (imgbPlatform == IMGBFlags.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Detected xbox 360 version image file. skipped image repacking.", true);
                return;
            }

            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockFile);

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image repacking.", true);
                return;
            }

            // Open the IMGB file and start repacking
            // the images according to the image type
            using (var imgbStream = new FileStream(outImgbFile, FileMode.Append, FileAccess.Write))
            {
                IMGBRepack2Types.ProcessImage(extractedIMGBdir, imgHeaderBlockFile, gtex, imgbStream);
            }
        }
    }
}