/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// An EditResponse is returned when either adding a new playlist or adding
    /// a track to a playlist.
    ///
    /// Request (new playlist named Test):
    /// GET /databases/39/edit?session-id=1959703320&action=add&edit-params='dmap.itemname:Test'
    /// 
    /// medc  --+
    ///     mstt   4      000000c8 == 200   #OK
    ///     miid   4      00000438 == 1080  #Playlist ID created
    ///
    /// Request (add track to playlist 1092):
    /// GET /databases/39/containers/1092/edit?session-id=1959703320&action=add&edit-params='dmap.itemid:207'
    ///
    /// medc  --+
    ///    mstt   4      200
    ///    mlit  --+
    ///            mcti    4    28773 # mcti tells us the new container-item-id of the track in the playlist.
    /// </summary>
    public class EditResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // fields
        private LinkedList<EditNode> mlit = new LinkedList<EditNode>(); // playlist container item id

        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public EditResponse(HttpListenerRequest request):base(request) {
            this.Miid = 0;
            // get the container ID if we are adding tracks to a playlist
            this.PlaylistId = GetContainerId();
        }

        /// Response:
        /// medc  --+
        ///     mstt   4      000000c8 == 200   #OK
        ///     miid   4      00000438 == 1080  #Playlist ID created
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            byte[] mlitBytes = null;
            
            if (this.Mlit.Count > 0) {
            	using (MemoryStream stream = new MemoryStream()) {
            		foreach (EditNode node in this.Mlit) {
            			byte[] itemBytes = node.GetBytes();
            			stream.Write(itemBytes, 0 , itemBytes.Length);
            		}
            		mlitBytes = CreateDACPResponseBytes(MLIT, stream.ToArray());
            	}
            }

            
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                if (this.Miid != 0) {
                    StreamInteger(writer, MIID, this.Miid);
                }
                
                if (mlitBytes != null) {
                	stream.Write(mlitBytes, 0 , mlitBytes.Length);
                }

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MEDC ,payload);
        }
        
        
        /// <summary>
        /// Adds a new container item to the playlist
        /// </summary>
        /// <param name="letter">the letter to add</param>
        /// <param name="startPosition">the start position in the list</param>
        /// <param name="count">the number of items for this letter</param>
        public void AddEditNode(int containerItemId) {
            EditNode node = new EditNode(containerItemId);
            this.Mlit.AddLast(node);
        }

        /// <summary>
        /// Property for Track Unique ID
        /// </summary>
        public int Miid {
            get;
            set;
        }

        /// <summary>
        /// Property to hold which container is selected
        /// </summary>
        public int PlaylistId {
            get;
            set;
        }
        
        /// <summary>
        /// A list of Container Item Ids
        /// </summary>
        public LinkedList<EditNode> Mlit {
            get {
                if (mlit == null) {
                    mlit = new LinkedList<EditNode>();
                }
                return mlit;
            } set {
                mlit = value;
            }
        }
    }
}
