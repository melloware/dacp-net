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
    /// A EditNode is a representation of a single container item in a playlist
    ///
    /// mlit  --+
    ///       mcti   4      00000f7c == 3964 // #item id
    /// </summary>
    public class EditNode:BaseNode {

        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor
        /// </summary>
        public EditNode() {
            this.Mcti = -1;
        }

        /// <summary>
        /// Constructor with params
        /// </summary>
        /// <param name="Id">the ID of the album</param>
        /// <param name="artist">the artist of the album</param>
        /// <param name="album">the album name</param>
        /// <param name="trackCount">the track count</param>
        public EditNode(int Id) {
            this.Mcti = Id;
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
                if (this.Mcti > 0 ) {
                    StreamInteger(writer, MCTI, this.Mcti);
                }
                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLIT ,payload);
        }
        
        /// <summary>
        /// The Playlist container item id
        /// </summary>
        private int Mcti {
            get;   
            set;
        }

    }
}

