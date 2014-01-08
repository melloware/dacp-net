/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.IO;
using System.Text;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// PairingClientResponse object when receiving a pairing request
    /// from a server, this is a fake response returning a GUID..
    ///
    /// Request:
    /// GET /pair?pairingcode=75D809650423A40091193AA4944D1FBD&servicename=D19BB75C3773B485
    ///
    /// Response:
    /// cmpa --+
    ///     cmpg 8 000000c8 == 0000000000000001
    ///     cmnm 10 648a861f == devicename
    ///     cmty 4 648a861f == ipod
    /// </summary>
    public class PairingClientResponse:DACPResponse {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PairingClientResponse() {
            LOG.Debug("Creating PairingClientResponse...");
            this.Cmpg = "\x00\x00\x00\x00\x00\x00\x00\x01";
            this.Cmnm = "devicename";
            this.Cmty = "ipod";
        }

        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamString(writer, CMPG, this.Cmpg);
                StreamString(writer, CMNM, this.Cmnm);
                StreamString(writer, CMTY, this.Cmty);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(CMPA ,payload);
        }

        public string Cmpg {
            get;
            set;
        }

        public string Cmnm {
            get;
            set;
        }

        public string Cmty {
            get;
            set;
        }
    }
}
