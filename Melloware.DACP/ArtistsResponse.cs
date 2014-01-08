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

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// ArtistsResponse object when receiving a artists request.
    ///
    ///
    /// Request To Get Artists By Artist:
    /// GET /databases/36/groups?session-id=1598562931&meta=dmap.itemname,dmap.itemid,dmap.persistentid,daap.songartist&type=music&group-type=artists&sort=artist&include-sort-headers=1&query='daap.songartist:*%s*'
    ///
    /// Response:
    ///   agar  --+
    /// 	mstt   4      000000c8 == 200
    /// 	muty   1      00 == 0
    /// 	mtco   4      00000002 == 2
    /// 	mrco   4      00000002 == 2
    /// 	mlcl  --+
    /// 		mlit  --+
    /// 			miid   4      000000a2 == 162
    /// 			mper   8      68d6f46ac9669c9f == 7554494164443241631
    /// 			minm   11     Hold Steady
    ///             agac   4      5 # album count
    /// 			mimc   4      0000003a == 58
    /// 		mlit  --+
    /// 			miid   4      000000a3 == 163
    /// 			mper   8      72fb7c682322f58f == 8285352726186096015
    /// 			minm   9      Lady Gaga
    ///             agac   4      1 # album count
    /// 			mimc   4      0000000c == 12
    /// 	mshl  --+
    /// 		mlit  --+
    /// 			mshc   2      0048
    /// 			mshi   4      00000000 == 0
    /// 			mshn   4      00000001 == 1
    /// 		mlit  --+
    /// 			mshc   2      004c
    /// 			mshi   4      00000001 == 1
    /// 			mshn   4      00000001 == 1
    /// </summary>
    public class ArtistsResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<ArtistNode> mlcl = new LinkedList<ArtistNode>();
        private LinkedList<IndexNode> indexes = new LinkedList<IndexNode>();

        // used for the letter index
        private int letterCounter = 0;
        private int itemCount = 1;
        private char letter = '\0';


        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public ArtistsResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating ArtistsResponse...");
            this.Muty = 0;
        }

        /// <summary>
        /// Creates the proper DACP response bytes for this type.
        /// </summary>
        /// <returns>a DACP byte[] array representing this type</returns>
        public override byte[] GetBytes() {

            // first cleanup the letter index if necessary
            if (!letter.Equals('\0')) {
                this.AddBrowseIndex(letter, letterCounter, itemCount);
                letter = '\0';
            }

            // Construct a response.
            byte[] payload = null;
            byte[] agarBytes = null;
            byte[] mlclBytes = null;
            byte[] mshlBytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mlcl.Count); // #total items found
                StreamInteger(writer, MRCO, this.Mlcl.Count); // #number of items returned here
                writer.Flush();
                agarBytes = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream()) {
                foreach (ArtistNode Artist in this.Mlcl) {
                    byte[] ArtistBytes = Artist.GetBytes();
                    stream.Write(ArtistBytes, 0 , ArtistBytes.Length);
                }
                mlclBytes = CreateDACPResponseBytes(MLCL, stream.ToArray());
            }

            // now loop through index nodes writing them out
            using (MemoryStream stream = new MemoryStream()) {
                foreach (IndexNode index in this.Indexes) {
                    byte[] indexBytes = index.GetBytes();
                    stream.Write(indexBytes, 0 , indexBytes.Length);
                }

                // create the MLIT byte arrays and wrap in an MSHL byte array
                mshlBytes = CreateDACPResponseBytes(MSHL, stream.ToArray());
            }


            // create the whole payload all elements
            using (MemoryStream stream = new MemoryStream()) {
                stream.Write(agarBytes, 0 , agarBytes.Length);
                stream.Write(mlclBytes, 0 , mlclBytes.Length);
                stream.Write(mshlBytes,  0 , mshlBytes.Length);

                // put the whole payload together and write it out
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(AGAR ,payload);
        }

        /// <summary>
        /// Adds a new letter to the index
        /// </summary>
        /// <param name="letter">the letter to add</param>
        /// <param name="startPosition">the start position in the list</param>
        /// <param name="count">the number of items for this letter</param>
        public void AddBrowseIndex(char letter, int startPosition, int count) {
            IndexNode node = new IndexNode(letter, startPosition, count);
            this.Indexes.AddLast(node);
        }

        /// <summary>
        /// Adds an Artist node and adds it to the letter index.
        /// </summary>
        /// <param name="id">the ID of the Artist</param>
        /// <param name="aArtist">the Artist name</param>
        /// <param name="albumCount">the album count</param>
        /// <param name="trackCount">the track count</param>
        public void AddArtistNode(int id,  string aArtist, int albumCount, int trackCount) {
            ArtistNode ArtistNode = new ArtistNode(id, aArtist, albumCount, trackCount);
            this.Mlcl.AddLast(ArtistNode);

            char firstLetter = Char.ToUpper(aArtist[0]);
            if (((letter == '\0') || (letter == 'A')) && ((firstLetter < 'A') || (firstLetter > 'Z'))) {
                firstLetter = 'A';
            }
            if ((firstLetter.Equals(letter)) || (firstLetter > 'Z')) {
                itemCount++;
            } else {
                if (!letter.Equals('\0')) {
                    this.AddBrowseIndex(letter, letterCounter, itemCount);
                    letterCounter += itemCount;
                }
                letter = firstLetter;
                itemCount = 1;
            }
        }

        /// <summary>
        /// List of Artists
        /// </summary>
        public LinkedList<ArtistNode> Mlcl {
            get {
                return mlcl;
            } set {
                mlcl = value;
            }
        }

        /// <summary>
        /// A list of indexed letter for the UI.
        /// </summary>
        public LinkedList<IndexNode> Indexes {
            get {
                return indexes;
            } set {
                indexes = value;
            }
        }
    }
}
