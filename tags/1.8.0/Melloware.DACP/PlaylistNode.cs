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
    /// A PlaylistNode is a representation of a single playlist.
    ///
    /// mlit  --+
    ///      miid   4      00000071 == 113 #item id
    ///      mper   8      d19bb75c3773b488 == 15103867382012294280 #persistent id
    ///      minm   16     75736572e2809973204c696272617279 #name
    ///      abpl   1      01 == 1 #daap base playlist indicator
    ///      mpco   4      00000000 == 0   #parent container id
    ///      meds   4      00000000 == 0   #edit status
    ///      mimc   4      00000102 == 258 #item count
    /// mlit  --+
    ///      miid   4      000000e0 == 224 #item id
    ///      mper   8      d19bb75c3773b492 == 15103867382012294290 #persistent id
    ///      minm   5      Music           #name
    ///      aeSP   1      01 == 1         #apple special playlist
    ///      mpco   4      00000000 == 0   #parent container id
    ///      aePS   1      06 == 6         #magic playlist
    ///      meds   4      00000000 == 0   #edit status
    ///      mimc   4      00000102 == 258 #item count
    /// </summary>
    public class PlaylistNode:BaseNode {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // constants for special playlist type
        public const byte PLAYLIST_TYPE_PODCASTS = 1;
        public const byte PLAYLIST_TYPE_DJ = 2;
        public const byte PLAYLIST_TYPE_MOVIES = 4;
        public const byte PLAYLIST_TYPE_TV = 5;
        public const byte PLAYLIST_TYPE_MUSIC = 6;
        public const byte PLAYLIST_TYPE_BOOKS = 7;
        public const byte PLAYLIST_TYPE_PURCHASED_ON_DEVICE = 9;
        public const byte PLAYLIST_TYPE_RENTALS = 10;
        public const byte PLAYLIST_TYPE_GENIUS = 12;
        public const byte PLAYLIST_TYPE_ITUNESU = 13;
        public const byte PLAYLIST_TYPE_GENIUS_FOLDER = 15;
        public const byte PLAYLIST_TYPE_GENIUS_GENERATED = 16;

        // constants for Edit Status
        public const int PLAYLIST_EDIT_STATUS_PODCASTS = 3;
        public const int PLAYLIST_EDIT_STATUS_DJ = 7;
        public const int PLAYLIST_EDIT_STATUS_GENIUS = 30;
        public const int PLAYLIST_EDIT_STATUS_TOP = 96;
        public const int PLAYLIST_EDIT_STATUS_RECENT = 100;
        public const int PLAYLIST_EDIT_STATUS_OTHER = 103;

        // fields
        private byte abpl;    // #daap base playlist indicator
        private byte aesp;    // #apple smart playlist
        private byte aeps;    // #apple special playlist
        private int mpco;     // #parent container id
        private int meds;     // #edit status
        private int ceji;     // #itunes dj current jukebox
        private string ascn;  // #description of radio stream (comments field in itunes) 
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public PlaylistNode() {
            LOG.Debug("Creating Playlist Node...");
            this.Miid = 0;
            this.Mper = (ulong)this.GetHashCode();
            this.Minm = "Music";
            this.Meds = 0;
            this.Mpco = 0;
            this.Ceji = 0;
            this.Ascn = null;
        }

        /// <summary>
        /// Response:
        /// mlit  --+
        ///      miid   4      00000071 == 113 #item id
        ///      mper   8      d19bb75c3773b488 == 15103867382012294280 #persistent id
        ///      minm   16     75736572e2809973204c696272617279 #name
        ///      abpl   1      01 == 1 #daap base playlist indicator
        ///      mpco   4      00000000 == 0   #parent container id
        ///      meds   4      00000000 == 0   #edit status
        ///      mimc   4      00000102 == 258 #item count
        /// </summary>
        /// <returns>the Playlist response in bytes</returns>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
            	BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
            	StreamInteger(writer, MIID, this.Miid);
            	StreamLong(writer, MPER, this.Mper);
            	StreamString(writer, MINM, this.Minm);
            	if (this.Abpl != Byte.MinValue) {
            		StreamByte(writer, ABPL, this.Abpl);
            	}
            	if (this.Aesp != Byte.MinValue) {
            		StreamByte(writer, AESP, this.Aesp);
            	}
            	StreamInteger(writer, MPCO, this.Mpco);
            	if (this.Aeps != Byte.MinValue) {
            		StreamByte(writer, AEPS, this.Aeps);
            	}
            	StreamInteger(writer, MEDS, this.Meds);
            	StreamInteger(writer, MIMC, this.Mimc);
            	
            	
            	if (this.Ceji != UInt16.MinValue) {
            		StreamInteger(writer, CEJI, this.Ceji);
            	}
            	StreamString(writer, ASCN, this.Ascn);

            	writer.Flush();
            	payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLIT ,payload);
        }

        /// <summary>
        /// DAAP Base Playlist Indicator
        /// </summary>
        public byte Abpl {
            get {
                return abpl;
            } set {
                abpl = value;
            }
        }

        /// <summary>
        /// Apple Smart Playlist
        /// </summary>
        public byte Aesp {
            get {
                return aesp;
            } set {
                aesp = value;
            }
        }

        /// <summary>
        /// Apple Special Playlist
        /// </summary>
        public byte Aeps {
            get {
                return aeps;
            } set {
                aeps = value;
            }
        }

        /// <summary>
        /// Parent Container Id
        /// </summary>
        public int Mpco {
            get {
                return mpco;
            } set {
                mpco = value;
            }
        }

        /// <summary>
        /// Edit Status
        /// </summary>
        public int Meds {
            get {
                return meds;
            } set {
                meds = value;
            }
        }
        
        /// <summary>
        /// Itunes DJ Current Jukebox
        /// </summary>
        public int Ceji {
        	get { return ceji; }
        	set { ceji = value; }
        }
        
        /// <summary>
        /// iTunes Comment Field, used to identify Radio Streams
        /// </summary>
        public string Ascn {
        	get { return ascn; }
        	set { ascn = value; }
        }

    }
}
