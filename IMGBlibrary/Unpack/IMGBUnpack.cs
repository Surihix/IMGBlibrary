using IMGBlibrary.Support;
using System.IO;

namespace IMGBlibrary.Unpack
{
    /// <summary>
    /// Provides a method for unpacking image files from the FINAL FANTASY XIII trilogy.
    /// </summary>
    public class IMGBUnpack
    {
        /// <summary>
        /// Use for unpacking image files.
        /// </summary>
        /// <param name="imgHeaderBlockFile">Header Block file path. should have the GTEX chunk.</param>
        /// <param name="imgbFile">IMGB file path. the file has to be present.</param>
        /// <param name="extractIMGBdir">Path to the directory where the image files should be unpacked.</param>
        /// <param name="imgbPlatform">Platform of the header block file.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static void UnpackIMGB(string imgHeaderBlockFile, string imgbFile, string extractIMGBdir, IMGBFlags.Platforms imgbPlatform, bool showLog)
        {
            var gtex = SharedMethods.GetGTEXInfo(imgHeaderBlockFile);

            if (!gtex.IsValid)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped image extraction.", true);
                return;
            }

            if (imgbPlatform == IMGBFlags.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Platform set to x360. extracted image file(s) will not be unswizzled.", true);
            }

            SharedMethods.DisplayLogMessage($"Image Format Value: {gtex.Format}", showLog);
            SharedMethods.DisplayLogMessage($"Image MipCount: {gtex.MipCount}", showLog);
            SharedMethods.DisplayLogMessage($"Image Type Value: {gtex.Type}", showLog);
            SharedMethods.DisplayLogMessage($"Image Width: {gtex.Width}", showLog);
            SharedMethods.DisplayLogMessage($"Image Height: {gtex.Height}", showLog);

            if (!SharedMethods.CheckGTEXFormatAndType(gtex))
            {
                SharedMethods.DisplayLogMessage("Detected unknown format or type. skipped image extraction.", true);
                return;
            }

            // Open the IMGB file and start extracting
            // the images according to the image type
            using (var imgbStream = new FileStream(imgbFile, FileMode.Open, FileAccess.ReadWrite))
            {
                IMGBUnpackTypes.Platform = imgbPlatform;

                switch (gtex.Type)
                {
                    // Classic type
                    // Type 4 is for console versions
                    case 0:
                    case 4:
                        IMGBUnpackTypes.UnpackClassic(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        break;

                    // Cubemap type 
                    // Type 5 is for console versions
                    case 1:
                    case 5:
                        IMGBUnpackTypes.UnpackCubemap(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        break;

                    // Volumemap type
                    case 2:
                        IMGBUnpackTypes.UnpackVolumemap(imgHeaderBlockFile, extractIMGBdir, gtex, imgbStream);
                        break;
                }
            }
        }
    }
}