/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// ArtworkResponse object when receiving a request for either thumbnail or
    /// now playing artwork.
    ///
    /// Request To Thumbnail:
    /// GET /databases/38/items/2854/extra_data/artwork?session-id=788509571&revision-number=196&mw=55&mh=55
    ///
    /// Request To Get Now Playing:
    /// GET /ctrl-int/1/nowplayingartwork?mw=320&mh=320&session-id=1940361390
    ///
    /// Request To Get Now Playing on iPad:
    /// GET /ctrl-int/1/nowplayingartwork?mw=0&mh=0&session-id=1940361390
    ///
    /// Response:
    ///   A byte stream of the image/png in PNG format.
    /// </summary>
    public class ArtworkResponse:SessionBoundResponse {
        // constants
        public const int NOW_PLAYING = 0;
        public const int THUMBNAIL_HEIGHT = 55;
        public const int THUMBNAIL_WIDTH = 55;
        public const int RETINA_HEIGHT = 768;
        public const int RETINA_WIDTH = 768;
        public const string PROPERTY_NOW_PLAYING = "nowplayingartwork";

        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private MemoryStream artwork;
        private int height = THUMBNAIL_HEIGHT;
        private int width = THUMBNAIL_WIDTH;
        private int itemId = NOW_PLAYING;
        public string GroupType = null;


        /// <summary>
        /// Constructs an ArtworkResponse and using the HTTP Request URL and params it
        /// figures out exactly which artwork you want. Either the NOW PLAYING artwork
        /// or a specific ItemId in the catalog.  Also it gets the Height and Width of
        /// the requested artwork from the URL params.
        /// </summary>
        /// <param name="request">the HTTPRequest to get the parameters</param>
        public ArtworkResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating ArtworkResponse...");
            string rawURL = request.RawUrl;

            // check whether this is Now Playing or a specific artwork request
            if (rawURL.Contains(PROPERTY_NOW_PLAYING)) {
                this.ItemId = NOW_PLAYING;
            } else {
                // default to now playing
                int id = NOW_PLAYING;
                try {
                    string[] values = rawURL.Split('/');
                    LOG.Debug(values);
                    // the 5th item is the id (/databases/38/items/2854/)
                    id = Convert.ToInt32(values[4]);
                } catch (Exception ex) {
                    LOG.Warn("ArtworkResponse Error trying to get the item id so defaulting to Now Playing.", ex);
                }
                this.ItemId = id;
            }

            LOG.DebugFormat("Artwork for Item = {0}", this.ItemId);

            try {
                this.Height = Convert.ToInt32(request.QueryString["mh"]);
                this.Width = Convert.ToInt32(request.QueryString["mw"]);
                if ((this.Height <= 0) || (this.Height > RETINA_HEIGHT)) {
                    this.Height = RETINA_HEIGHT;
                }
                if ((this.Width <= 0) ||  (this.Width > RETINA_WIDTH)) {
                    this.Width = RETINA_WIDTH;
                }
            } catch (Exception ex) {
                LOG.Warn("ArtworkResponse Error trying to get the height and width values.", ex);
            }
            LOG.DebugFormat("Height = {0} Width = {1}", this.Height, this.Width);
            
            // group-type=albums
            GroupType = request.QueryString[PROPERTY_GROUP_TYPE];

            // null out the request because we are caching this response
            this.HttpRequest = null;
        }

        /// <summary>
        /// Adds the stream of an image file to this object and adds it to the cache of
        /// images.
        /// </summary>
        /// <param name="stream">the Stream of the image to assign</param>
        public void AddArtwork(MemoryStream stream) {
            this.Artwork = stream;

            // add to the cache
            ArtworkCache.Instance.AddItem(this);
        }

        /// <summary>
        /// Adds artwork from the filepath specified and adds to the cache.
        /// </summary>
        /// <param name="filePath">the location of the PNG/Image file</param>
        public void AddArtwork(string filePath) {
            MemoryStream stream = LoadArtwork(filePath);
            this.AddArtwork(stream);
        }

        /// <summary>
        /// Loads a image file off disk and resizes it to a PNG of the correct size.
        /// </summary>
        /// <param name="image">The Bitmap image to resize</param>
        /// <param name="width">the width you want to make the PNG</param>
        /// <param name="height">the height you want to make the PNG</param>
        /// <returns>a MemoryStream full of image in bytes</returns>
        public static MemoryStream LoadArtwork(Bitmap image, int width, int height) {
            MemoryStream result = new MemoryStream();

            // Make a bitmap for the result.
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(72,72);

            // Make a Graphics object for the result Bitmap.
            Graphics graphics = Graphics.FromImage(bitmap);

            try {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Copy the source image into the destination bitmap.
                //graphics.DrawImage(image, 0, 0, bitmap.Width + 1,bitmap.Height + 1);
                graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

                // stream the PNG out to bytes
                bitmap.Save(result, ImageFormat.Png);
            } finally {
                // very important to dispose to release native resources
                graphics.Dispose();
                graphics = null;
                bitmap.Dispose();
                bitmap = null;
            }

            return result;

        }
        /// <summary>
        /// Loads a image file off disk and resizes it to a PNG of the correct size.
        /// </summary>
        /// <param name="filePath">the image file location to load</param>
        /// <param name="width">the width you want to make the PNG</param>
        /// <param name="height">the height you want to make the PNG</param>
        /// <returns>a MemoryStream full of image in bytes</returns>
        public static MemoryStream LoadArtwork(string filePath, int width, int height) {
            LOG.DebugFormat("LoadArtwork: {0} {1} {2}", width, height, filePath);
            if (!File.Exists(filePath)) {
                LOG.WarnFormat("Artwork file {0} does not exist!", filePath);
                return null;
            }

            MemoryStream result = null;

            // load the original bitmap
            Bitmap bitmap = new Bitmap(filePath);
            try {
                result = LoadArtwork(bitmap, width, height);
            } finally {
                // very important to dispose to release native resources
                bitmap.Dispose();
                bitmap = null;
            }

            return result;
        }


        /// <summary>
        /// Loads the raw image file at its native resolution.
        /// </summary>
        /// <param name="filePath">the path to the image to load</param>
        /// <returns>a Memorystream of bytes of the image</returns>
        public static MemoryStream LoadArtwork(string filePath) {
            LOG.DebugFormat("LoadArtwork: {0}", filePath);
            if (!File.Exists(filePath)) {
                LOG.WarnFormat("Artwork file {0} does not exist!", filePath);
                return null;
            }
            MemoryStream result = new MemoryStream();

            // load the original bitmap
            Bitmap bitmap = new Bitmap(filePath);
            try {
                // stream the PNG out to bytes
                bitmap.Save(result, ImageFormat.Png);
            } finally {
                // very important to dispose to release native resources
                bitmap.Dispose();
                bitmap = null;
            }

            return result;
        }


        /// <summary>
        /// Takes a Stream of an image and resizes it.
        /// </summary>
        /// <param name="stream">the stream containing the image</param>
        /// <param name="width">the new width</param>
        /// <param name="height">the new height</param>
        /// <returns></returns>
        private static byte[] ResizeArtwork(MemoryStream stream, int width, int height) {
            LOG.DebugFormat("ResizeArtwork: {0} {1}", width, height);
            byte[] result = null;
            Bitmap original = new Bitmap(stream);
            Bitmap scaled = new Bitmap(original, new Size(width, height));
            try {
                MemoryStream scaledStream = new MemoryStream();
                // stream the PNG out to bytes
                scaled.Save(scaledStream, ImageFormat.Png);
                result = scaledStream.ToArray();
            } finally {
                original.Dispose();
                scaled.Dispose();
                original = null;
                scaled = null;
            }
            return result;
        }

        /// <summary>
        /// Represents this response as a byte stream.
        /// </summary>
        /// <returns>a PNG in bytes of the item artwork</returns>
        public override byte[] GetBytes() {
            byte[] result = new byte[1];
            if (this.Artwork != null) {
                result = ResizeArtwork(this.Artwork, this.Width, this.Height);
            }
            return result;
        }

        /// <summary>
        /// The PNG artwork in a stream of bytes
        /// </summary>
        public MemoryStream Artwork {
            get {
                return artwork;
            } set {
                artwork = value;
            }
        }

        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height {
            get {
                return height;
            } set {
                height = value;
            }
        }

        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width {
            get {
                return width;
            } set {
                width = value;
            }
        }

        /// <summary>
        /// The ID of this image
        /// </summary>
        public int ItemId {
            get {
                return itemId;
            } set {
                itemId = value;
            }
        }
    }
}
