using IMGBlibrary.Support;
using System.IO;
using System.Linq;

namespace IMGBlibrary.Repack
{
    /// <summary>
    /// Use for repacking images in WPD type files or for images that 
    /// require the pixel format, mipcount and dimensions to be 
    /// same as the original.
    /// </summary>
    public class IMGBRepack1
    {
        public static void RepackIMGBType1(string imgHeaderBlockFile, string outImgbFile, string extractedIMGBdir, bool showLog)
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
                    case 5:
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