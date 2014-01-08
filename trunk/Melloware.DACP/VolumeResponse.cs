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
    /// VolumeResponse object when receiving a volume request.
    ///
    /// We can also control the volume on a scale 0-100,
    /// both by asking for its value and setting it:
    ///
    /// Request To Get Volume:
    /// GET /ctrl-int/1/getproperty?properties=dmcp.volume&session-id=1686799903
    ///
    /// Response:
    /// cmgt --+
    ///     mstt 4 000000c8 == 200
    ///     cmvo 4 00000054 == 84 # current volume
    /// </summary>
    public class VolumeResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs a Volumeresponse which gets the volume field out of the
        /// HTTP Request like dmcp.volume=49.  If no volume is in the query then
        /// the client is just asking for the current volume.
        /// </summary>
        /// <param name="request">the HTTPListenerRequest</param>
        public VolumeResponse(HttpListenerRequest request):base(request) {
            LOG.DebugFormat("Creating VolumeResponse...");
            int volume = 50;

            try {
                string property = request.QueryString["properties"];
                string vol = request.QueryString[PROPERTY_VOLUME];

                if (property != null) {
                    string[] values = property.Split('=');
                    if (values.Length > 1) {
                        volume = Convert.ToInt32(values[1]);
                    }
                }

                if (vol != null) {
                    volume = Convert.ToInt32(vol);
                }

            } catch (Exception ex) {
                LOG.Warn("VolumeResponse Contructor Error!", ex);
            }

            if (volume > 100) {
                volume = 100;
            } else if (volume < 0) {
                volume = 0;
            }

            LOG.DebugFormat("Volume = {0}", volume);
            this.Cmvo = volume;
        }

        /// <summary>
        /// Response:
        /// cmgt --+
        ///     mstt 4 000000c8 == 200
        ///     cmvo 4 00000054 == 84 # current volume
        /// </summary>
        /// <returns>the Volume response in bytes</returns>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamInteger(writer, CMVO, this.Cmvo);
                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(CMGT ,payload);
        }

        /// <summary>
        /// Volume property from 0-100
        /// </summary>
        public int Cmvo {
            get;
            set;
        }
    }
}
