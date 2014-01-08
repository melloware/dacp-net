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
    /// AlbumsResponse object when receiving a albums request.
    ///
    ///
    /// Request To Get Albums By Artist:
    /// GET /databases/36/groups?session-id=1598562931&meta=dmap.itemname,dmap.itemid,dmap.persistentid,daap.songartist&type=music&group-type=albums&sort=artist&include-sort-headers=1&query='daap.songartist:*%s*'
    ///
    /// Request To Get Albums by Index:
    /// GET /databases/36/groups?session-id=1034286700&meta=dmap.itemname,dmap.itemid,dmap.persistentid,daap.songartist&type=music&group-type=albums&sort=artist&include-sort-headers=1&index=0-50
    ///
    /// Response:
    ///  agal  --+
    ///    mstt   4      000000c8 == 200
    ///    muty   1      00 == 0
    ///    mtco   4      00000017 == 23
    ///    mrco   4      00000017 == 23
    ///    mlcl  --+
    ///            mlit  --+
    ///                    miid   4      00000025 == 37
    ///                    mper   8      a150fef71188fb88 == 11624070975347817352
    ///                    minm   13     New Surrender
    ///                    asaa   8      Anberlin
    ///                    mimc   4      0000000c == 12
    ///            mlit  --+
    ///                    miid   4      00000026 == 38
    ///                    mper   8      a150fef71188fb89 == 11624070975347817353
    ///                    minm   19     This is an Outrage!
    ///                    asaa   14     Capital Lights
    ///                    mimc   4      0000000c == 12
    ///            mlit  --+
    ///                    miid   4      00000f7c == 3964
    ///                    mper   8      eb1a08f3f8f632e3 == 16940862792254501603
    ///                    minm   27     A Rush of Blood to the Head
    ///                    asaa   8      Coldplay
    ///                    mimc   4      0000000b == 11
    ///            mlit  --+
    ///                    miid   4      00000f7d == 3965
    ///                    mper   8      eb1a08f3f8f632e4 == 16940862792254501604
    ///                    minm   3      X&Y
    ///                    asaa   8      Coldplay
    ///                    mimc   4      0000000d == 13
    ///            mlit  --+
    ///                    miid   4      00000f7f == 3967
    ///                    mper   8      eb1a08f3f8f632e6 == 16940862792254501606
    ///                    minm   13     All I Can Say
    ///                    asaa   18     David Crowder Band
    ///                    mimc   4      0000000b == 1
    ///    mshl  --+
    ///            mlit  --+
    ///                    mshc   2      0041
    ///                    mshi   4      00000000 == 0
    ///                    mshn   4      00000001 == 1
    ///            mlit  --+
    ///                    mshc   2      0043
    ///                    mshi   4      00000001 == 1
    ///                    mshn   4      00000003 == 3
    ///            mlit  --+
    ///                    mshc   2      0044
    ///                    mshi   4      00000004 == 4
    ///                    mshn   4      00000008 == 8
    ///            mlit  --+
    ///                    mshc   2      0046
    ///                    mshi   4      0000000c == 12
    ///                    mshn   4      00000001 == 1
    ///            mlit  --+
    ///                    mshc   2      004d
    ///                    c   4      0000000d == 13
    ///                    mshn   4      00000005 == 5
    ///            mlit  --+
    ///                    mshc   2      0052
    ///                    mshi   4      00000012 == 18
    ///                    mshn   4      00000005 == 5
    ///    mudl   0
    /// </summary>
    public class AlbumsResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<AlbumNode> mlcl = new LinkedList<AlbumNode>();
        private LinkedList<IndexNode> indexes = new LinkedList<IndexNode>();

        // used for the letter index
        private int letterCounter = 0;
        private int itemCount = 1;
        private char letter = '\0';


        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public AlbumsResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating AlbumsResponse...");
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
            byte[] agalBytes = null;
            byte[] mlclBytes = null;
            byte[] mshlBytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mlcl.Count); // #total items found
                StreamInteger(writer, MRCO, this.Mlcl.Count); // #number of items returned here
                writer.Flush();
                agalBytes = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream()) {
                foreach (AlbumNode album in this.Mlcl) {
                    byte[] albumBytes = album.GetBytes();
                    stream.Write(albumBytes, 0 , albumBytes.Length);
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
                stream.Write(agalBytes, 0 , agalBytes.Length);
                stream.Write(mlclBytes, 0 , mlclBytes.Length);
                stream.Write(mshlBytes,  0 , mshlBytes.Length);

                // put the whole payload together and write it out
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(AGAL ,payload);
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
        /// Adds an album node and adds it to the letter index.
        /// </summary>
        /// <param name="Id">the ID of the album</param>
        /// <param name="artist">the artist of the album</param>
        /// <param name="album">the album name</param>
        /// <param name="trackCount">the track count</param>
        public void AddAlbumNode(int id, string artist, string aAlbum, int trackCount) {
            AlbumNode albumNode = new AlbumNode(id, artist, aAlbum, trackCount);
            this.Mlcl.AddLast(albumNode);

            char firstLetter = Char.ToUpper(aAlbum[0]);
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
        /// List of Albums
        /// </summary>
        public LinkedList<AlbumNode> Mlcl {
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
