using IMGBlibrary.Support;
using System.IO;
using System.Linq;

namespace IMGBlibrary.Unpack
{
    /// <summary>
    /// Use for unpacking image files.
    /// </summary>
    public class IMGBUnpack
    {
        public static void UnpackIMGB(string imgHeaderBlockFile, string inImgbFile, string extractIMGBdir, IMGBEnums.Platforms imgbPlatform, bool showLog)
        {
            var imgbVars = new IMGBVariables
            {
                ShowLog = showLog,
                GtexStartVal = SharedMethods.GetGTEXChunkPos(imgHeaderBlockFile)
            };

            if (imgbVars.GtexStartVal == 0)
            {
                SharedMethods.DisplayLogMessage("Unable to find GTEX chunk. skipped to next file.", imgbVars.ShowLog);
                return;
            }

            switch (imgbPlatform)
            {
                case IMGBEnums.Platforms.win32:
                    imgbVars.IsWin32Imgb = true;
                    break;

                case IMGBEnums.Platforms.ps3:
                    imgbVars.IsPs3Imgb = true;
                    break;

                case IMGBEnums.Platforms.x360:
                    imgbVars.IsX360Imgb = true;
                    break;
            }

            if (imgbVars.IsX360Imgb)
            {
                SharedMethods.DisplayLogMessage("Platform set to x360. extracted image file(s) will not be unswizzled.", imgbVars.ShowLog);
            }

            SharedMethods.GetImageInfo(imgHeaderBlockFile, imgbVars);

            SharedMethods.DisplayLogMessage("Image Format Value: " + imgbVars.GtexImgFormatValue, imgbVars.ShowLog);
            SharedMethods.DisplayLogMessage("Image MipCount: " + imgbVars.GtexImgMipCount, imgbVars.ShowLog);
            SharedMethods.DisplayLogMessage("Image Type Value: " + imgbVars.GtexImgTypeValue, imgbVars.ShowLog);
            SharedMethods.DisplayLogMessage("Image Width: " + imgbVars.GtexImgWidth, imgbVars.ShowLog);
            SharedMethods.DisplayLogMessage("Image Height: " + imgbVars.GtexImgHeight, imgbVars.ShowLog);

            if (!IMGBVariables.GtexImgFormatValues.Contains(imgbVars.GtexImgFormatValue))
            {
                SharedMethods.DisplayLogMessage("Detected unknown image format. skipped to next file.", imgbVars.ShowLog);
                return;
            }

            if (!IMGBVariables.GtexImgTypeValues.Contains(imgbVars.GtexImgTypeValue))
            {
                SharedMethods.DisplayLogMessage("Detected unknown image type. skipped to next file.", imgbVars.ShowLog);
                return;
            }


            // Open the IMGB file and start extracting
            // the images according to the image type
            using (var imgbStream = new FileStream(inImgbFile, FileMode.Open, FileAccess.ReadWrite))
            {

                switch (imgbVars.GtexImgTypeValue)
                {
                    // Classic or Other type
                    // Type 0 is Classic
                    // Type 4 is Other
                    case 0:
                    case 4:
                        IMGBUnpackTypes.UnpackClassic(imgHeaderBlockFile, extractIMGBdir, imgbVars, imgbStream);
                        break;

                    // Cubemap type 
                    // Type 5 is for PS3
                    case 1:
                    case 5:
                        IMGBUnpackTypes.UnpackCubemap(imgHeaderBlockFile, extractIMGBdir, imgbVars, imgbStream);
                        break;

                    // Stacked type (LR only)
                    case 2:
                        if (imgbVars.GtexImgMipCount > 1)
                        {
                            SharedMethods.DisplayLogMessage("Detected more than one mip in this stack type image. skipped to next file.", showLog);
                            return;
                        }
                        IMGBUnpackTypes.UnpackStack(imgHeaderBlockFile, extractIMGBdir, imgbVars, imgbStream);
                        break;
                }
            }
        }
    }
}