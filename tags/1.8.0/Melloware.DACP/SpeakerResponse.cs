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
    /// SpeakerResponse object when receiving a speaker request.
    ///
    /// We can also control the volume on a scale 0-100,
    /// both by asking for its value and setting it:
    ///
    /// Request To Get Speakers:
    /// GET /ctrl-int/1/getspeakers?session-id=8675309
    ///
    /// Response:
    ///   casp  --+
    /// 	mstt   4      000000c8 == 200
    /// 	mdcl  --+
    /// 		minm   8      Computer
    /// 		msma   8      0000000000000000 == 0
    /// 		caia   1      01 == 1
    /// 		cmvo   4      00000064 == 100
    /// 	mdcl  --+
    /// 		cmvo   4      00000064 == 100
    /// 		minm   27     Melloware's AirPort Express
    /// 		msma   8      000078ca3946072a == 132809939617578
    /// </summary>
    public class SpeakerResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private LinkedList<SpeakerNode> mdcl = new LinkedList<SpeakerNode>();

        public SpeakerResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating SpeakerResponse...");
            this.Muty = 0;
        }

        /// <summary>
        /// Adds a set of speakers to the list of available speakers
        /// </summary>
        /// <param name="name">the name of the speaker set</param>
        /// <param name="ipaddress">the ip address of the speakers</param>
        /// <param name="currentvolume">the current volume</param>
        public void AddSpeakers(string name, ulong ipaddress, int currentvolume, byte isAvailable) {
            SpeakerNode node = new SpeakerNode();
            node.Minm = name;
            node.Msma = ipaddress;
            node.Cmvo = currentvolume;
            node.Caia = isAvailable;
            this.Mdcl.AddLast(node);
        }

        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
            using (MemoryStream stream = new MemoryStream()) {
                BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
                StreamInteger(writer, MSTT, this.Mstt);
                StreamByte(writer, MUTY, this.Muty);
                StreamInteger(writer, MTCO, this.Mdcl.Count); // #total items found
                StreamInteger(writer, MRCO, this.Mdcl.Count); // #number of items returned here
                
                foreach (SpeakerNode node in this.Mdcl) {
                    byte[] nodeBytes = node.GetBytes();
                    stream.Write(nodeBytes, 0 , nodeBytes.Length);
                }

                writer.Flush();
                payload = stream.ToArray();
            }
            return CreateDACPResponseBytes(CASP ,payload);
        }

        public LinkedList<SpeakerNode> Mdcl {
            get {
                return mdcl;
            } set {
                mdcl = value;
            }
        }
    }
}
