/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace Melloware.DACP {
    /// <summary>
    /// BrowseResponse index which tracks how many letters of each
    /// artist, composer, genre there are and what order so the UI can
    /// give the cool A, B, C, letter on the right hand gutter.
    ///
    /// Response:
    ///  abro  --+
    ///        mstt   4      000000c8 == 200
    ///        muty   1      00 == 0
    ///        mtco   4      0000000d == 13  # total items found
    ///        mrco   4      0000000d == 13  # number of items returned here
    ///        abar  --+
    ///                mlit  7      Beatles                 #name
    ///                mlit  7      Beulah                  #name
    ///                mlit  7      Brian Wilson            #name
    ///                mlit  7      Hold Steady             #name
    ///        mshl  --+
    ///                mlit  --+
    ///                    mshc   2      0042               # letter B
    ///                    mshi   4      00000000 == 0      # start pos
    ///                    mshn   4      00000002 == 3      # artist count
    ///                mlit  --+
    ///                    mshc   2      0042               # letter H
    ///                    mshi   4      00000000 == 3      # start position
    ///                    mshn   4      00000002 == 1      # artist count
    ///
    /// </summary>
    public class IndexNode:DACPResponse {

        /// <summary>
        /// Default constructor
        /// </summary>
        public IndexNode() {
            this.Mshi = 0;
            this.Mshn = 1;
        }

        /// <summary>
        /// Constructor with params for BrowseIndexNode
        /// </summary>
        /// <param name="letter">the letter to add</param>
        /// <param name="startPosition">the start position in the list</param>
        /// <param name="count">the number of items for this letter</param>
        public IndexNode(char letter, int startPosition, int count) {
            this.Mshc = Convert.ToUInt16(letter);
            this.Mshi = startPosition;
            this.Mshn = count;
        }

        /// <summary>
        /// Returns this reponse in binary format.
        /// </summary>
        /// <returns>the bytes that represent this response</returns>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamShort(writer, MSHC, this.Mshc);
                StreamInteger(writer, MSHI, this.Mshi);
                StreamInteger(writer, MSHN, this.Mshn);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLIT ,payload);
        }

        /// <summary>
        /// Alphabetic Letter Property
        /// </summary>
        public ushort Mshc {
            get;
            set;
        }

        /// <summary>
        /// Start Position Property
        /// </summary>
        public int Mshi {
            get;
            set;
        }

        /// <summary>
        /// Item Count For This Letter Property
        /// </summary>
        public int Mshn {
            get;
            set;
        }
    }
}
