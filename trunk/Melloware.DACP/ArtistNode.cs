/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/


using System;
using System.Text;
using System.IO;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// A ArtistNode is a representation of a single Artist.
    ///
    ///     mlcl  --+
    ///         mlit  --+
    ///             miid   4      000000a2 == 162
    ///             mper   8      68d6f46ac9669c9f == 7554494164443241631
    ///             minm   11     Hold Steady
    ///             agac   4      5 # album count
    ///             mimc   4      0000003a == 58
    ///         mlit  --+
    ///             miid   4      000000a3 == 163
    ///             mper   8      72fb7c682322f58f == 8285352726186096015
    ///             minm   9      Lady Gaga
    ///             agac   4      1 # album count
    ///             mimc   4      0000000c == 12
    /// </summary>
    public class ArtistNode:BaseNode {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor
        /// </summary>
        public ArtistNode() {
            this.Miid = 0;
            this.Mper = (ulong)this.GetHashCode();
            this.Minm = "Artist";
            this.Agac = 0;
            this.Mimc = 0;
        }

        /// <summary>
        /// Constructor with params
        /// </summary>
        /// <param name="Id">the ID of the Artist</param>
        /// <param name="artist">the artist of the Artist</param>
        /// <param name="Artist">the Artist name</param>
        /// <param name="trackCount">the track count</param>
        public ArtistNode(int Id, string artist, int albumCount, int trackCount) {
            this.Miid = Id;
            this.Mper = (ulong)this.Miid;
            this.Minm = artist;
            this.Agac = albumCount;
            this.Mimc = trackCount;
        }


        /// <summary>
        /// Creates the proper DACP response bytes for this type.
        /// </summary>
        /// <returns>a DACP byte[] array representing this type</returns>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MIID, this.Miid);
                StreamLong(writer, MPER, this.Mper);
                if (this.Minm != null) {
                    StreamString(writer, MINM, this.Minm);
                }
                StreamInteger(writer, AGAC, this.Agac);
                StreamInteger(writer, MIMC, this.Mimc);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLIT ,payload);
        }

        /// <summary>
        /// Album count for all albums for an artist
        /// </summary>
        private int Agac {
            get;    // #album count
            set;
        }
    }
}
