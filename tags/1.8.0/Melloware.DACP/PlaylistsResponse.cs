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
    /// PlaylistsResponse object when receiving a playlists request.
    ///
    ///
    /// Request To Get Volume:
    /// GET /databases/36/containers?session-id=1686799903&meta=dmap.itemname,dmap.itemcount,dmap.itemid,dmap.persistentid,daap.baseplaylist,com.apple.itunes.special-playlist,com.apple.itunes.smart-playlist,com.apple.itunes.saved-genius,dmap.parentcontainerid,dmap.editcommandssupported
    ///
    /// Response:
    ///  aply  --+
    ///        mstt   4      000000c8 == 200
    ///        muty   1      00 == 0
    ///        mtco   4      0000000d == 13  # total items found
    ///        mrco   4      0000000d == 13  # number of items returned here
    ///        mlcl  --+
    ///                mlit  --+
    ///                        miid   4      00000071 == 113 #item id
    ///                        mper   8      d19bb75c3773b488 == 15103867382012294280 #persistent id
    ///                        minm   16     75736572e2809973204c696272617279 #name
    ///                        abpl   1      01 == 1 #daap base playlist indicator
    ///                        mpco   4      00000000 == 0   #parent container id
    ///                        meds   4      00000000 == 0   #edit status
    ///                        mimc   4      00000102 == 258 #item count
    ///                mlit  --+
    ///                        miid   4      000000e0 == 224 #item id
    ///                        mper   8      d19bb75c3773b492 == 15103867382012294290 #persistent id
    ///                        minm   5      Music           #name
    ///                        aeSP   1      01 == 1         #apple special playlist
    ///                        mpco   4      00000000 == 0   #parent container id
    ///                        aePS   1      06 == 6         #magic playlist
    ///                        meds   4      00000000 == 0   #edit status
    ///                        mimc   4      00000102 == 258 #item count
    ///                mlit  --+
    ///                        miid   4      00000106 == 262
    ///                        mper   8      d19bb75c3773b493 == 15103867382012294291
    ///                        minm   6      Movies
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        aePS   1      04 == 4
    ///                        meds   4      00000000 == 0
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      00000109 == 265
    ///                        mper   8      d19bb75c3773b494 == 15103867382012294292
    ///                        minm   8      TV Shows
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        aePS   1      05 == 5
    ///                        meds   4      00000000 == 0
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      000000d9 == 217
    ///                        mper   8      d19bb75c3773b491 == 15103867382012294289
    ///                        minm   8      Podcasts
    ///                        mpco   4      00000000 == 0
    ///                        aePS   1      01 == 1
    ///                        meds   4      00000003 == 3
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      00000115 == 277
    ///                        mper   8      d19bb75c3773b498 == 15103867382012294296
    ///                        minm   6      Genius
    ///                        mpco   4      00000000 == 0
    ///                        aePS   1      0c == 12
    ///                        meds   4      0000001e == 30
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      00000097 == 151
    ///                        mper   8      d19bb75c3773b489 == 15103867382012294281
    ///                        minm   12     3930e2809973204d75736963
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000064 == 100
    ///                        mimc   4      0000000b == 11
    ///                mlit  --+
    ///                        miid   4      000000d3 == 211
    ///                        mper   8      d19bb75c3773b48e == 15103867382012294286
    ///                        minm   12     Music Videos
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000060 == 96
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      0000009a == 154
    ///                        mper   8      d19bb75c3773b48a == 15103867382012294282
    ///                        minm   12     My Top Rated
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000060 == 96
    ///                        mimc   4      00000000 == 0
    ///                mlit  --+
    ///                        miid   4      000000ad == 173
    ///                        mper   8      d19bb75c3773b48d == 15103867382012294285
    ///                        minm   14     Recently Added
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000064 == 100
    ///                        mimc   4      00000102 == 258
    ///                mlit  --+
    ///                        miid   4      000000a5 == 165
    ///                        mper   8      d19bb75c3773b48c == 15103867382012294284
    ///                        minm   15     Recently Played
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000064 == 100
    ///                        mimc   4      00000014 == 20
    ///                mlit  --+
    ///                        miid   4      0000009d == 157
    ///                        mper   8      d19bb75c3773b48b == 15103867382012294283
    ///                        minm   18     Top 25 Most Played
    ///                        aeSP   1      01 == 1 #apple special playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000060 == 96
    ///                        mimc   4      00000014 == 20
    ///                mlit  --+
    ///                        miid   4      00000854 == 2132
    ///                        mper   8      eb1a08f3f8f63195 == 16940862792254501269
    ///                        minm   17     untitled playlist
    ///                        mpco   4      00000000 == 0
    ///                        meds   4      00000067 == 103
    ///                        mimc   4      00000003 == 3
    /// </summary>
    public class PlaylistsResponse:SessionBoundResponse {
    	
    	// logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<PlaylistNode> mlcl = new LinkedList<PlaylistNode>();

        public PlaylistsResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating PlaylistsResponse...");
            this.Muty = 0;
        }

        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            byte[] aplyBytes = null;
            byte[] mlclBytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mlcl.Count);
                StreamInteger(writer, MRCO, this.Mlcl.Count); // #number of items returned here

                writer.Flush();
                aplyBytes = stream.ToArray();
            }

            // now loop through playlist nodes writing them out
            using (MemoryStream stream = new MemoryStream()) {
                foreach (PlaylistNode playlist in this.Mlcl) {
                    byte[] playlistBytes = playlist.GetBytes();
                    stream.Write(playlistBytes, 0 , playlistBytes.Length);
                }

                // create the MLIT byte arrays and wrap in an MLCL byte array
                mlclBytes = CreateDACPResponseBytes(MLCL, stream.ToArray());
            }

            using (MemoryStream stream = new MemoryStream()) {
                stream.Write(aplyBytes, 0 , aplyBytes.Length);
                stream.Write(mlclBytes, 0 , mlclBytes.Length);

                // create the MLIT byte arrays and wrap in an MLCL byte array
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(APLY ,payload);
        }

        public LinkedList<PlaylistNode> Mlcl {
            get {
                return mlcl;
            } set {
                mlcl = value;
            }
        }
    }
}
