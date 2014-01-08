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
    /// A AlbumNode is a representation of a single album.
    ///
    /// mlit  --+
    ///       miid   4      00000f7c == 3964 // #item id
    ///       mper   8      eb1a08f3f8f632e3 == 16940862792254501603 // #persistent id
    ///       minm   27     A Rush of Blood to the Head // #album name
    ///       asaa   8      Coldplay        //#album artist
    ///       mimc   4      0000000b == 11  //#track count
    /// mlit  --+
    ///       miid   4      00000f7d == 3965
    ///       mper   8      eb1a08f3f8f632e4 == 16940862792254501604
    ///       minm   3      X&Y
    ///       asaa   8      Coldplay
    ///       mimc   4      0000000d == 13
    /// </summary>
    public class AlbumNode:BaseNode {

        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor
        /// </summary>
        public AlbumNode() {
            this.Miid = 0;
            this.Mper = (ulong)this.GetHashCode();
            this.Minm = "Album";
            this.Asaa = "Artist";
            this.Mimc = 0;
        }

        /// <summary>
        /// Constructor with params
        /// </summary>
        /// <param name="Id">the ID of the album</param>
        /// <param name="artist">the artist of the album</param>
        /// <param name="album">the album name</param>
        /// <param name="trackCount">the track count</param>
        public AlbumNode(int Id, string artist, string album, int trackCount) {
            this.Miid = Id;
            this.Mper = (ulong)this.Miid;
            this.Minm = album;
            this.Asaa = artist;
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
                if (this.Asaa != null) {
                    StreamString(writer, ASAA, this.Asaa);
                }
                StreamInteger(writer, MIMC, this.Mimc);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLIT ,payload);
        }
        
        /// <summary>
        /// The Album Artist field
        /// </summary>
        private string Asaa {
            get;    // #album artist
            set;
        }

    }
}
