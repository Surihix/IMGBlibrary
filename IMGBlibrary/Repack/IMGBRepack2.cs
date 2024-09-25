using IMGBlibrary.Support;
using System.IO;
using System.Linq;

namespace IMGBlibrary.Repack
{
    /// <summary>
    /// Use for repacking images in TRB files or for images that 
    /// does 'not' require the pixel format, mipcount and 
    /// dimensions to be same as the original.
    /// </summary>
    public class IMGBRepack2
    {
        public static void RepackIMGBType2(string tmpImgHeaderBlockFile, string imgHeaderBlockFileName, string outImgbFile, string extractedIMGBdir, bool showLog)
        {
            var imgbVars = new IMGBVariables
            {
                ShowLog = showLog,
                ImgHeaderBlockFileName = imgHeaderBlockFileName,
                GtexStartVal = SharedMethods.GetGTEXChunkPos(tmpImgHeaderBlockFile)
            };

            if (imgbVars.GtexStartVal == 0)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped to next file.", showLog);
                return;
            }

            SharedMethods.GetImageInfo(tmpImgHeaderBlockFile, imgbVars);

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
            using (var imgbStream = new FileStream(outImgbFile, FileMode.Append, FileAccess.Write))
            {

                switch (imgbVars.GtexImgTypeValue)
                {
                    // Classic or Other type
                    // Type 0 is Classic
                    // Type 4 is Other
                    case 0:
                    case 4:
                        IMGBRepack2Types.RepackClassicType(tmpImgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;

                    // Cubemap type 
                    case 1:
                        IMGBRepack2Types.RepackCubemapType(tmpImgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
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
                        IMGBRepack2Types.RepackStackType(tmpImgHeaderBlockFile, extractedIMGBdir, imgbVars, imgbStream);
                        break;
                }
            }
        }
    }
}