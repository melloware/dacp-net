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
using System.Net;
using System.Threading;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// UpdateResponse object when receiving a update request.
    ///
    /// Request To Update:
    /// GET /update?session-id=1423769200&revision-number=1
    ///
    /// Response:
    /// mupd --+
    ///     mstt 4 000000c8 == 200
    ///     musr 4 00000001 == 3 #server revision number
    /// </summary>
    public class UpdateResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs an UpdateResposne which returns revision number of the
        /// server we are using.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public UpdateResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating UpdateResponse...");
            string revisionStr = request.QueryString[PROPERTY_REVISION];
            int revision = 0;
            Int32.TryParse(revisionStr, out revision);
            this.Musr = revision;

            LOG.Info("UpdateResponse Blocking until signal received");
            while (this.Musr > Session.DatabaseRevision) {
                Thread.Sleep(SessionInfo.SLEEP_INTERVAL);
            }

            LOG.Warn("UpdateResponse Waking up!");
            this.Musr = Session.DatabaseRevision + 1;
        }

        /// <summary>
        /// Response:
        /// mupd --+
        ///     mstt 4 000000c8 == 200
        ///     musr 4 00000001 == 1 #server revision number
        /// </summary>
        /// <returns>the Update response in bytes</returns>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamInteger(writer, MUSR, this.Musr);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MUPD ,payload);
        }

        /// <summary>
        /// Server revision number field MUSR.
        /// </summary>
        public int Musr {
            get ;
            set ;
        }
    }
}
