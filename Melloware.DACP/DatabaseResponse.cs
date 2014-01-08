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
    /// DatabaseResponse object when receiving a database request.
    ///
    /// Request:
    /// GET /databases?session-id=1034286700&revision-number=1
    ///
    /// Response:
    /// avdb  --+
    ///    mstt   4      000000c8 == 200
    ///    muty   1      00 == 0
    ///    mtco   4      00000001 == 1
    ///    mrco   4      00000001 == 1
    ///    mlcl  --+
    ///            mlit  --+
    ///                    miid   4      00000024 == 36
    ///                    mper   8      d19bb75c3773b487 == 15103867382012294279
    ///                    minm   16     75736572e2809973204c696272617279
    ///                    mimc   4      00000102 == 258
    ///                    mctc   4      0000000d == 13
    ///                    meds   4      00000003 == 3
    ///                    mdbk   4      00000001 == 1  # local iTunes database
    /// </summary>
    public class DatabaseResponse:SessionBoundResponse {
    	
    	// logger for this class
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // database kind constants
        public const int DATABASE_KIND_LOCAL = 1;
        public const int DATABASE_KIND_SHARED = 2;
        public const int DATABASE_KIND_RADIO = 100;

        // default constructor
        public DatabaseResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating DatabaseResponse...");
            this.Muty = 0;
            this.Mtco = 1;
            this.Mrco = 1;
            this.Miid = 1;
            this.Mper = 15103867382;
            this.Minm = "DACPServer";
            this.Mimc = 1;
            this.Mctc = 17;
            this.Meds = 3;
            this.Mdbk = DATABASE_KIND_LOCAL;
        }

        /// <summary>
        /// Response:
        /// avdb  --+
        ///    mstt   4      000000c8 == 200
        ///    muty   1      00 == 0
        ///    mtco   4      00000001 == 1
        ///    mrco   4      00000001 == 1
        ///    mlcl  --+
        ///            mlit  --+
        ///                    miid   4      00000024 == 36
        ///                    mper   8      d19bb75c3773b487 == 15103867382012294279
        ///                    minm   16     75736572e2809973204c696272617279
        ///                    mimc   4      00000102 == 258
        ///                    mctc   4      0000000d == 13
        ///                    meds   4      00000003 == 3
        ///                    mdbk   4      00000001 == 1
        /// </summary>
        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            byte[] avdb = null;
            byte[] mlit = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mtco);
                StreamInteger(writer, MRCO, this.Mrco);
                writer.Flush();
                avdb = stream.ToArray();
            }

            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MIID, this.Miid);
                StreamLong(writer, MPER, this.Mper);
                StreamString(writer, MINM, this.Minm);
                StreamInteger(writer, MIMC, this.Mimc);
                StreamInteger(writer, MCTC, this.Mctc);
                StreamInteger(writer, MEDS, this.Meds);
                StreamInteger(writer, MDBK, this.Mdbk);
                writer.Flush();
                mlit = CreateDACPResponseBytes(MLIT ,stream.ToArray());
            }

            byte[] mlcl = CreateDACPResponseBytes(MLCL ,mlit);

            using (MemoryStream stream = new MemoryStream()) {
                stream.Write(avdb, 0 , avdb.Length);
                stream.Write(mlcl, 0 , mlcl.Length);
                payload = stream.ToArray();
            }

            return CreateDACPResponseBytes(AVDB ,payload);
        }

        /// <summary>
        /// unique item id
        /// </summary>
        public int Miid {
            get;
            set;
        }


        /// <summary>
        /// persistent id
        /// </summary>
        public ulong Mper {
            get;
            set;
        }


        /// <summary>
        /// Name
        /// </summary>
        public string Minm {
            get;
            set;
        }


        /// <summary>
        /// Item Count
        /// </summary>
        public int Mimc {
            get;
            set;
        }


        /// <summary>
        /// Container Count
        /// </summary>
        public int Mctc {
            get;
            set;
        }


        /// <summary>
        /// Edit Status
        /// </summary>
        public int Meds {
            get;
            set;
        }
        
        /// <summary>
        /// To find the radio database, the requesthelper now advertises itself as client-daap-version 3.10.  
        /// This results in an array of databases being returned in the database query instead of just one.  
        /// As far as I can tell the remote uses the mdbk (database kind?) field to determine which is which.
        /// From what I have observed
        /// mdbk 1 = local itunes database
        /// mdbk 2 = shared itunes database on local network
        /// mdbk 100 = radio database
        /// </summary>
        public int Mdbk {
        	get;
        	set;
        }
    }
}
