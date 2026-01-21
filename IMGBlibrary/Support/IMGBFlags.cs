namespace IMGBlibrary.Support
{
    /// <summary>
    /// Provides flags for accessing functions from this library.
    /// </summary>
    public class IMGBFlags
    {
        /// <summary>
        /// Use for determing the extension of the image header block file.
        /// </summary>
        public enum FileExtensions
        {
            /// <summary>
            /// Present mainly in trb files
            /// </summary>
            txb,

            /// <summary>
            /// Present manily in xgr files
            /// </summary>
            txbh,

            /// <summary>
            /// Present mainly in xfv files
            /// </summary>
            vtex
        }

        /// <summary>
        /// Use for setting the platform of the image header block file.
        /// </summary>
        public enum Platforms
        {
            /// <summary>
            /// PC version
            /// </summary>
            win32,

            /// <summary>
            /// PS3 version
            /// </summary>
            ps3,

            /// <summary>
            /// Xbox 360 version
            /// </summary>
            x360
        }
    }
}