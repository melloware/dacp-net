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

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// LoginResponse object when receiving a login request.
    ///
    /// Request:
    /// GET /login?pairing-guid=0x0000000000000001
    ///
    /// Response:
    /// mlog --+
    ///     mstt 4 000000c8 == 200
    ///     mlid 4 648a861f == 1686799903 # our new session-id
    /// </summary>
    public class LoginResponse:DACPResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LoginResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating LoginResponse...");
            this.Mstt = 200;

            // converts 0x0000000000000001 to ulong = 1
            this.Guid = ConvertHexParameterToLong(request, PROPERTY_PAIRING_GUID);
            LOG.DebugFormat("Login Pairing GUID = {0}", this.Guid);

            if (this.Guid <= 0) {
                throw new DACPSecurityException("Pairing GUID is not found in HTTP Rquest!");
            }

            this.Mlid = 1;
        }

        /// <summary>
        /// Session Id
        /// </summary>
        public int Mlid {
            get;
            set;
        }

        /// <summary>
        /// Pairing GUID input param from query string
        /// </summary>
        public ulong Guid {
            get;
            set;
        }

        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamInteger(writer, MLID, this.Mlid);

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(MLOG ,payload);
        }
    }
}
