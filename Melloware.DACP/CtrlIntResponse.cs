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
    /// Description of CtrlIntResponse.
    ///
    /// Request To Get Artist Tracks:
    /// GET /ctrl-int
    ///
    /// Response:
    ///  caci  --+
    ///        mstt   4      000000c8 == 200
    ///        muty   1      00 == 0
    ///        mtco   4      00000001 == 1  # total items found
    ///        mrco   4      00000001 == 1  # number of items returned here
    ///        mlcl  --+
    ///              mlit  --+
    ///                     miid   1      02 == 2
    ///                     cmik   1      01 == 1
    ///                     cmsp   1      01 == 1
    ///                     cmsv   1      01 == 1
    ///                     cass   1      01 == 1
    ///                     casu   1      01 == 1
    ///                     caSG   1      01 == 1
    /// </summary>
    public class CtrlIntResponse:DACPResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<CtrlIntNode> mlcl = new LinkedList<CtrlIntNode>();

        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public CtrlIntResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating CtrlIntResponse...");
            this.Muty = 0;
            CtrlIntNode node = new CtrlIntNode();
            this.Mlcl.AddLast(node);
        }

        /// <summary>
        /// Creates the proper DACP response bytes for this type.
        /// </summary>
        /// <returns>a DACP byte[] array representing this type</returns>
        public override byte[] GetBytes() {
            byte[] payload = null;
            byte[] caciBytes = null;
            byte[] mlclBytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mlcl.Count); // #total items found
                StreamInteger(writer, MRCO, this.Mlcl.Count); // #number of items returned here
                writer.Flush();
                caciBytes = stream.ToArray();
            }

            // now loop through track nodes writing them out
            using (MemoryStream stream = new MemoryStream()) {
                foreach (CtrlIntNode node in this.Mlcl) {
                    byte[] bytes = node.GetBytes();
                    stream.Write(bytes, 0 , bytes.Length);
                }

                // create the MLIT byte arrays and wrap in an MLCL byte array
                mlclBytes = CreateDACPResponseBytes(MLCL, stream.ToArray());
            }

            // put the whole payload together and write it out
            using (MemoryStream stream = new MemoryStream()) {
                stream.Write(caciBytes, 0 , caciBytes.Length);
                stream.Write(mlclBytes, 0 , mlclBytes.Length);
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(CACI ,payload);
        }

        /// <summary>
        /// List of CtrlInt nodes
        /// </summary>
        public LinkedList<CtrlIntNode> Mlcl {
            get {
                return mlcl;
            } set {
                mlcl = value;
            }
        }
    }
}
