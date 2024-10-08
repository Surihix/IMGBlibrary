using IMGBlibrary.Support;
using System.IO;
using System.Linq;

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
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
        public static void RepackIMGBType1(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir, IMGBEnums.Platforms imgbPlatform, bool showLog)
        {
            var imgbVars = new IMGBVariables
            {
                ShowLog = showLog,
                GtexStartVal = SharedMethods.GetGTEXChunkPos(imgHeaderBlockFile)
            };

            if (imgbVars.GtexStartVal == 0)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped to next file.", showLog);
                return;
            }

            SharedMethods.GetImageInfo(imgHeaderBlockFile, imgbVars);

            if (!IMGBVariables.GtexImgFormatValues.Contains(imgbVars.GtexImgFormatValue))
            {
                SharedMethods.DisplayLogMessage("Detected unknown image format. skipped to next file.", showLog);
                return;
            }

            if (!IMGBVariables.GtexImgTypeValues.Contains(imgbVars.GtexImgTypeValue))
            {
                SharedMethods.DisplayLogMessage("Detected unknown image type. skipped to next file.", showLog);
                return;
            }

            if (imgbPlatform == IMGBEnums.Platforms.ps3)
            {
                SharedMethods.DisplayLogMessage("Detected ps3 version image file. imgb repacking is not supported.", showLog);
                return;
            }

            if (imgbPlatform == IMGBEnums.Platforms.x360)
            {
                SharedMethods.DisplayLogMessage("Detected xbox 360 version image file. imgb repacking is not supported.", showLog);
                return;
            }


            // Open the IMGB file and start repacking
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
                        IMGBRepack1Types.RepackClassicType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;

                    // Cubemap type 
                    // Type 5 is for PS3
                    case 1:
                        IMGBRepack1Types.RepackCubemapType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;

                    // Stacked type (LR only)
                    // PC version wpd may or may not use
                    // this type.
                    case 2:
                        if (imgbVars.GtexImgMipCount > 1)
                        {
                            SharedMethods.DisplayLogMessage("Detected more than one mip in this stack type image. skipped to next file.", showLog);
                            return;
                        }
                        IMGBRepack1Types.RepackStackType1(imgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;
                }
            }
        }
    }
}