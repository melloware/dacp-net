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
    /// TrackResponse object when receiving a search or track listing request.
    ///
    ///
    /// Request To Get Artist Tracks:
    /// GET /databases/36/containers/113/items?session-id=1535976870&meta=dmap.itemname,dmap.itemid,daap.songartist,daap.songalbum,daap.songalbum&type=music&sort=album&query='daap.songartist:Family%20Force%205'
    ///
    /// Request for Search Results:
    /// GET /databases/36/containers/113/items?session-id=1535976870&revision-number=61&meta=dmap.itemname,dmap.itemid,daap.songartist,daap.songalbum&type=music&sort=name&include-sort-headers=1&query='dmap.itemname:*sea*'&index=0-7
    ///
    /// Request for Album Tracks:
    /// GET /databases/36/containers/113/items?session-id=1301749047&meta=dmap.itemname,dmap.itemid,daap.songartist,daap.songalbum,daap.songalbum,daap.songtime,daap.songtracknumber&type=music&sort=album&query='daap.songalbumid:11624070975347817354'
    ///
    /// Response:
    ///  apso  --+
    ///        mstt   4      000000c8 == 200
    ///        muty   1      00 == 0
    ///        mtco   4      0000000d == 13  # total items found
    ///        mrco   4      0000000d == 13  # number of items returned here
    ///        mlcl  --+
    ///              mlit  --+
    ///                    mikd   1      02 == 2
    ///                    asal   12     Dance or Die
    ///                    asar   14     Family Force 5
    ///                    astm   4      0003d5d6 == 251350
    ///                    astn   2      0001
    ///                    miid   4      0000005b == 91
    ///                    minm   12     dance or die
    ///              mlit  --+
    ///                    mikd   1      02 == 2
    ///                    asal   16     Almost Killed Me
    ///                    asar   11     Hold Steady
    ///                    astm   4      00030be5 == 199653
    ///                    miid   4      000002a7 == 679
    ///                    minm   12     Positive Jam
    ///                    mcti   4      00000373 == 883
    ///                    aeHV   1      00 == 0
    ///                    asai   8      44ba43b77c9e1a8d == 4952345195596094093
    ///              mlit  --+
    ///                    mikd   1      02 == 2
    ///                    asal   25     Boys And Girls In America
    ///                    asar   11     Hold Steady
    ///                    astm   4      0003915e == 233822
    ///                    miid   4      000001b5 == 437
    ///                    minm   14     Hot Soft Light
    ///                    mcti   4      00000384 == 900
    ///                    aeHV   1      00 == 0
    ///                    asai   8      593042020b1764f5 == 6426709244801148149
    /// </summary>
    public class TracksResponse:SessionBoundResponse {

        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<TrackNode> mlcl = new LinkedList<TrackNode>();

        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public TracksResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating TracksResponse...");
            this.Muty = 0;
            this.PlaylistId = 1;

            // check whether this is playlist request for tracks
            if (request.RawUrl.Contains(PROPERTY_CONTAINERS)) {
                this.PlaylistId = GetContainerId();
            } else {
                LOG.InfoFormat("Track Query = {0}", this.GetQuery());
            }
        }

        /// <summary>
        /// Creates the proper DACP response bytes for this type.
        /// </summary>
        /// <returns>a DACP byte[] array representing this type</returns>
        public override byte[] GetBytes() {
            byte[] payload = null;
            byte[] apsoBytes = null;
            byte[] mlclBytes = null;
            // Construct a response.
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mlcl.Count); // #total items found
                StreamInteger(writer, MRCO, this.Mlcl.Count); // #number of items returned here

                writer.Flush();
                apsoBytes = stream.ToArray();
            }

            // now loop through track nodes writing them out
            using (MemoryStream stream = new MemoryStream()) {
                foreach (TrackNode track in this.Mlcl) {
                    byte[] trackBytes = track.GetBytes();
                    stream.Write(trackBytes, 0 , trackBytes.Length);
                }

                // create the MLIT byte arrays and wrap in an MLCL byte array
                mlclBytes = CreateDACPResponseBytes(MLCL, stream.ToArray());
            }

            // create the whole payload all element
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                writer.Write(apsoBytes, 0 , apsoBytes.Length);
                writer.Write(mlclBytes, 0 , mlclBytes.Length);
                StreamByte(writer, MUDL, 0);
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(APSO ,payload);
        }

        /// <summary>
        /// Adds a TrackNode to the list and increments the MCTI playlist counter by
        /// 1 for playlist id tracking.
        /// </summary>
        /// <param name="node">the TrackNode to add to the list</param>
        public void AddTrackNode(TrackNode node) {
            int count = this.Mlcl.Count;
            node.Mcti = count; // MCTI is the playlist item number which is required
            this.Mlcl.AddLast(node);
        }

        /// <summary>
        /// List of Tracks
        /// </summary>
        public LinkedList<TrackNode> Mlcl {
            get {
                return mlcl;
            } set {
                mlcl = value;
            }
        }

        /// <summary>
        /// Playlist ID if this is a playlist request
        /// </summary>
        public int PlaylistId {
            get;
            set;
        }
    }
}
