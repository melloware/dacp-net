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
    // declares the enum
    public enum BrowseType {
        Artist,
        Composer,
        Genre
    }

    /// <summary>
    /// BrowseResponse object when receiving a browse database request.
    ///
    ///
    /// Request To Get Artists:
    /// GET /databases/1/browse/artists?session-id=1686799903&include-sort-headers=1
    ///
    /// Request To Get Genres:
    /// GET /databases/1/browse/genres?session-id=1686799903&include-sort-headers=1
    ///
    /// Request To Get Composers:
    /// GET /databases/1/browse/composers?session-id=1686799903&include-sort-headers=1
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
    ///                mlit  7      Hold Steady             #name
    ///        mshl  --+
    ///                mlit  --+
    ///                    mshc   2      0042               # letter B
    ///                    mshi   4      00000000 == 0      # order
    ///                    mshn   4      00000002 == 2      # artist count
    ///                mlit  --+
    ///                    mshc   2      0042               # letter H
    ///                    mshi   4      00000000 == 1      # order
    ///                    mshn   4      00000002 == 1      # artist count
    ///
    /// </summary>
    public class BrowseResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<String> genres = null;
        private LinkedList<String> artists = null;
        private LinkedList<String> composers = null;
        private LinkedList<IndexNode> indexes = new LinkedList<IndexNode>();

        // used for letter index
        private int letterCounter = 0;
        private int itemCount = 1;
        private char letter = '\0';


        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public BrowseResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating BrowseResponse...");
            this.Muty = 0;
            this.Mtco = 1;
            this.Mrco = 1;
        }

        /// <summary>
        /// Creates the proper DACP response bytes for this type.
        /// </summary>
        /// <returns>a DACP byte[] array representing this type</returns>
        public override byte[] GetBytes() {

            // first check the index and add default if empty
            if (!letter.Equals('\0')) {
                this.AddBrowseIndex(letter, letterCounter, itemCount);
                letter = '\0';
            }

            // Construct a response.
            this.Mrco = this.Artists.Count;
            byte[] payload = null;
            byte[] abroBytes = null;
            byte[] listBytes = new byte[1];
            byte[] mshlBytes = null;

            // now loop through nodes writing them out
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                if ((this.Artists != null) && (this.Artists.Count > 0)) {
                    foreach (string artist in this.Artists) {
                        StreamString(writer, MLIT, artist);
                    }

                    this.Mtco = this.Artists.Count;
                    this.Mrco = this.Artists.Count;

                    // create the MLIT byte arrays and wrap in an ABAR byte array
                    byte[] artistsBytes = stream.ToArray();
                    listBytes = CreateDACPResponseBytes(ABAR, artistsBytes);
                }

                if ((this.Genres != null) && (this.Genres.Count > 0)) {
                    foreach (string genre in this.Genres) {
                        StreamString(writer, MLIT, genre);
                    }

                    this.Mtco = this.Genres.Count;
                    this.Mrco = this.Genres.Count;

                    // create the MLIT byte arrays and wrap in an ABGN byte array
                    byte[] genresBytes = stream.ToArray();
                    listBytes = CreateDACPResponseBytes(ABGN, genresBytes);
                }

                if ((this.Composers != null) && (this.Composers.Count > 0)) {
                    foreach (string composer in this.Composers) {
                        StreamString(writer, MLIT, composer);
                    }

                    this.Mtco = this.Composers.Count;
                    this.Mrco = this.Composers.Count;

                    // create the MLIT byte arrays and wrap in an ABCP byte array
                    byte[] composerBytes = stream.ToArray();
                    listBytes = CreateDACPResponseBytes(ABCP, composerBytes);
                }
            }


            using (MemoryStream indexStream = new MemoryStream()) {
                foreach (IndexNode index in this.Indexes) {
                    byte[] indexBytes = index.GetBytes();
                    indexStream.Write(indexBytes, 0 , indexBytes.Length);
                }

                // create the MLIT byte arrays and wrap in an MSHL byte array
                mshlBytes = CreateDACPResponseBytes(MSHL, indexStream.ToArray());
            }

            using (MemoryStream browseStream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(browseStream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mtco);
                StreamInteger(writer, MRCO, this.Mrco);

                writer.Flush();
                abroBytes = browseStream.ToArray();
            }

            // create the whole payload all elements
            using (MemoryStream stream = new MemoryStream()) {
                stream.Write(abroBytes,  0 , abroBytes.Length);
                stream.Write(listBytes,  0 , listBytes.Length);
                stream.Write(mshlBytes,  0 , mshlBytes.Length);
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(ABRO ,payload);
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
        /// Adds an Artist to the list and updates the index.
        /// </summary>
        /// <param name="aArtist">the Artist to add</param>
        public void AddArtist(string aArtist) {
            this.Artists.AddLast(aArtist);
            this.EndIndex = this.Artists.Count;
            this.UpdateBrowseIndex(aArtist);
        }

        /// <summary>
        /// Adds a Genre to the list and updates the index.
        /// </summary>
        /// <param name="aGenre">the Genre to add</param>
        public void AddGenre(string aGenre) {
            this.Genres.AddLast(aGenre);
            this.EndIndex = this.Genres.Count;
            this.UpdateBrowseIndex(aGenre);
        }

        /// <summary>
        /// Adds a Composer to the list and updates the index.
        /// </summary>
        /// <param name="aComposer">the Composer to add</param>
        public void AddComposer(string aComposer) {
            this.Composers.AddLast(aComposer);
            this.EndIndex = this.Composers.Count;
            this.UpdateBrowseIndex(aComposer);
        }

        /// <summary>
        /// Updates the browse index of letters.
        /// </summary>
        /// <param name="aValue">the value to add to the index</param>
        private void UpdateBrowseIndex(string aValue) {
            char firstLetter = Char.ToUpper(aValue[0]);
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
        /// A list of Artists
        /// </summary>
        public LinkedList<string> Artists {
            get {
                if (artists == null) {
                    artists = new LinkedList<String>();
                }
                return artists;
            } set {
                artists = value;
            }
        }

        /// <summary>
        /// A list of Genres
        /// </summary>
        public LinkedList<string> Genres {
            get {
                if (genres == null) {
                    genres = new LinkedList<String>();
                }
                return genres;
            } set {
                genres = value;
            }
        }

        /// <summary>
        /// A list of composers
        /// </summary>
        public LinkedList<string> Composers {
            get {
                if (composers == null) {
                    composers = new LinkedList<String>();
                }
                return composers;
            } set {
                composers = value;
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
