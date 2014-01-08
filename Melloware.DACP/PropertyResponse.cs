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
    /// Description of PropertyResponse.
    /// </summary>
    public class PropertyResponse:SessionBoundResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //fields
        public byte Caps;      // #play status: 4=playing, 3=paused, 2=stopped
        public int Cmvo;       // #current volume
        public byte Cash;      // #shuffle status: 0=off, 1=on
        public int Caas;       // #available shuffle states, only seen '2'
        public byte Carp;      // #repeat status: 0=none, 1=single, 2=all
        public byte Cafs;      // #fullscreen controllable: 0=false, 1=true
        public byte Cavs;      // #visualizer controllable: 0=false, 1=true
        public byte Cavc;      // #volume controllable: 0=false, 1=true
        public int Caar;       // #available repeat states, only seen '6'
        public byte Cafe;      // #Fullscreen Enabled: 0=false, 1=true
        public byte Cave;      // #visualizer Enabled: 0=false, 1=true
        public byte CeGS;      // #genius seletable: 0=false, 1=true
        public string Cann;    // #now playing track
        public string Cana;    // #now playing artist
        public string Canl;    // #now playing album
        public string Cang;    // #now playing genre
        public ulong Asai;     // #song album id
        public int Cant;       // #song remaining time
        public int Cast;       // #song time
        public int Cmmk;       // #media kind (MUSIC = 1;VIDEO = 2;PODCAST = 4;AUDIOBOOK = 8;)
        
        // used to construct the 'canp'
		public int CurrentDatabase = 0;
		public int CurrentPlaylist = 0;
		public int CurrentPlaylistTrack = 0;
		public int CurrentTrack = 0;

        /// <summary>
        /// Constructor from HTTPRequest.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public PropertyResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating PropertyResponse...");
            this.Caps = PAUSED; //default to paused
			this.Cmvo = 0;
			this.Cash = 0;
			this.Carp = 0;
			this.Cavc = TRUE;
			this.Cavs = TRUE;
			this.Cafs = TRUE;
			this.CeGS = FALSE;
			this.Caas = 2;
			this.Caar = 6;
			this.Cafe = TRUE;
			this.Cave = TRUE;
			this.Cann = "Track";
			this.Cana = "Artist";
			this.Canl = "Album";
			this.Cang = "Genre";
			this.Asai = (ulong)this.GetHashCode();
			this.Cant = 179000;
			this.Cast = 180000;
			this.Cmmk = MEDIAKIND_MUSIC;

        }


        public override byte[] GetBytes() {
            // Construct a response.
            byte[] payload = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
				StreamInteger(writer, MSTT, this.Mstt);
				StreamByte(writer, CAPS, this.Caps);
				StreamInteger(writer, CMVO, this.Cmvo);
				StreamByte(writer, CASH, this.Cash);
				StreamByte(writer, CARP, this.Carp);
				StreamByte(writer, CAVC, this.Cavc);
				StreamByte(writer, CAVS, this.Cavs);
				StreamByte(writer, CAFS, this.Cafs);
				StreamByte(writer, CAFE, this.Cafe);
				StreamByte(writer, CAVE, this.Cave);
				
				// only send this value if it is true
				if (this.CeGS == TRUE) {
					StreamByte(writer, CEGS, this.CeGS);
				}
				StreamInteger(writer, CAAS, this.Caas);
				StreamInteger(writer, CAAR, this.Caar);

				// CANP is actually a 4 integer combined array
				int[] array = new int[4];
				array[0] = this.CurrentDatabase;
				array[1] = this.CurrentPlaylist;
				array[2] = this.CurrentPlaylistTrack;
				array[3] = this.CurrentTrack;
				StreamIntArray(writer, CANP, array);

				StreamString(writer, CANN, this.Cann);
				StreamString(writer, CANA, this.Cana);
				StreamString(writer, CANL, this.Canl);
				StreamString(writer, CANG, this.Cang);

				StreamLong(writer, ASAI, this.Asai);
				StreamInteger(writer, CMMK, this.Cmmk);
				StreamInteger(writer, CANT, this.Cant);
				StreamInteger(writer, CAST, this.Cast);

				writer.Flush();
				payload = stream.ToArray();
			}

            return CreateDACPResponseBytes(CMGT ,payload);
        }

    }
}
