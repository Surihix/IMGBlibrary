using IMGBlibrary.Support;
using System.IO;
using System.Linq;

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
        /// <param name="tmpImgHeaderBlockFile">Create a copy of the original header block file and provide its path. should have the GTEX chunk.</param>
        /// <param name="imgHeaderBlockFileName">Name of the header block file. this should be the original file's name.</param>
        /// <param name="outImgbFile">IMGB file path. not mandatory for the file to be present.</param>
        /// <param name="extractedIMGBdir">Path to the directory where the image files are present.</param>
        /// <param name="showLog">Determine whether to show more messages related to this method's process.</param>
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