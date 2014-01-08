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
	/// PlayerStatusUpdateResponse object when a client asks for the
	/// current status of the player.
	///
	/// Request:
	/// GET /ctrl-int/1/playstatusupdate?revision-number=1&session-id=1686799903
	///
	/// Something important to note is the revision-number field.
	/// By starting with revision-number=1, we know that we'll always get
	/// a response. However, a new revision-number is returned in the cmsr field.
	/// If we request that new revision-number, our HTTP request hangs until the
	/// next server event happens, providing push event notification. For example
	/// , our request might hang until the user manually changes tracks on the
	/// computer, or pauses the track, or maybe a TCP timeout happens.
	///
	/// Response:
	/// cmst --+
	///     mstt 4 000000c8 == 200
	///     cmsr 4 00000006 == 6 # revision-number
	///     caps 1 04 == 4 # play status: 4=playing, 3=paused, 2=stopped
	///     cash 1 01 == 1 # shuffle status: 0=off, 1=on
	///     carp 1 00 == 0 # repeat status: 0=none, 1=single, 2=all
	///     cavc 1 01 == 1 # volume controllable: 0=false, 1=true
	///     caas 4 00000002 == 2 # available shuffle states, only seen '2'
	///     caar 4 00000006 == 6 # available repeat states, only seen '6'
	///     canp 16 00000026000052200000530200000f68 #4 ids: dbid, plid, playlistItem, itemid
	///     cann 13 Secret Crowds # track
	///     cana 17 Angels & Airwaves # artist
	///     canl 8 I-Empire # album
	///     cang 0 # genre
	///     asai 8 a0d34e8b82616ae8 == 11588692627249261288 # album-id
	///     cmmk 4 00000001 == 1 # MediaKind (1 = song)
	///     cant 4 0003a15f == 237919 # remaining track time in ms
	///     cast 4 0004a287 == 303751 # total track length in ms
	///     cavs 1 01 == 1 # visualizer controllable: 0=false, 1=true
	///     cafs 1 01 == 1 # fullscreen controllable: 0=false, 1=true
	///     ceGS 1 01 == 1 # genius selectable: 0=false, 1=true
	/// </summary>
	public class PlayerStatusUpdateResponse:SessionBoundResponse {


		// logger
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//fields
		public int Cmsr;       // #media revision
		public byte Caps;      // #play status: 4=playing, 3=paused, 2=stopped
		public int Cmvo;       // #current volume
		public byte Cash;      // #shuffle status: 0=off, 1=on
		public byte Carp;      // #repeat status: 0=none, 1=single, 2=all
		public byte Cavs;      // #visualizer controllable: 0=false, 1=true
		public byte Cafs;      // #fullscreen controllable: 0=false, 1=true
		public byte Cavc;      // #volume controllable: 0=false, 1=true
		public int Caas;       // #available shuffle states, only seen '2'
		public int Caar;       // #available repeat states, only seen '6'
		public byte Cafe;      // #Fullscreen Enabled: 0=false, 1=true
		public byte Cave;      // #Visualizer Enabled ( 0=false, 1=true)
		public byte CeGS;      // #genius seletable: 0=false, 1=true  if ceGS then the song supports generating a genius playlist
		public string Cann;    // #now playing track
		public string Cana;    // #now playing artist
		public string Canl;    // #now playing album
		public string Cang;    // #now playing genre
		public ulong Asai;     // #now playing album id
		public int Cmmk;       // #media kind (MUSIC = 1;VIDEO = 2;PODCAST = 4;AUDIOBOOK = 8;)
		public int Cant;       // #song remaining time
		public int Cast;       // #song time
		public byte Casu;      // #unknown? (default = 1)

		// used to construct the 'canp'
		public int CurrentDatabase = 0;
		public int CurrentPlaylist = 0;
		public int CurrentPlaylistTrack = 0;
		public int CurrentTrack = 0;

		/// <summary>
		/// Constructor from HTTPRequest.
		/// </summary>
		/// <param name="request">the HTTPRequest to use</param>
		public PlayerStatusUpdateResponse(HttpListenerRequest request):base(request) {
			LOG.Debug("Creating PlayerStatusUpdateResponse...");
			string revisionStr = request.QueryString[PROPERTY_REVISION];
			int revision = 0;
			Int32.TryParse(revisionStr, out revision);
			this.Cmsr = revision; //revision number
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
			this.Cmmk = MEDIAKIND_MUSIC;
			this.Cant = 179000;
			this.Cast = 180000;
			this.Casu = TRUE;

			LOG.DebugFormat("PlayerStatusUpdateResponse Blocking until signal received CMSR = {0}", this.Cmsr);
			while (this.Cmsr > Session.CtrlIntRevision) {
				Thread.Sleep(SessionInfo.SLEEP_INTERVAL);
			}

			LOG.InfoFormat("PlayerStatusUpdateResponse Waking up CMSR = {0}, CTRLINT = {1}!", this.Cmsr, Session.CtrlIntRevision);
			this.Cmsr = Session.CtrlIntRevision + 1;
		}

		/// Response:
		/// cmst --+
		///     mstt 4 000000c8 == 200
		///     cmsr 4 00000006 == 6 # revision-number
		///     caps 1 04 == 4 # play status: 4=playing, 3=paused, 2=stopped
		///     cash 1 01 == 1 # shuffle status: 0=off, 1=on
		///     carp 1 00 == 0 # repeat status: 0=none, 1=single, 2=all
		///     cavc 1 01 == 1 # volume controllable: 0=false, 1=true
		///     caas 4 00000002 == 2 # available shuffle states, only seen '2'
		///     caar 4 00000006 == 6 # available repeat states, only seen '6'
		///     canp 16 00000026000052200000530200000f68 #4 ids: dbid, plid, playlistItem, itemid
		///     cann 13 Secret Crowds # track
		///     cana 17 Angels & Airwaves # artist
		///     canl 8 I-Empire # album
		///     cang 0 # genre
		///     asai 8 a0d34e8b82616ae8 == 11588692627249261288 # album-id
		///     cmmk 4 00000001 == 1 # MediaKind (1 = song)
		///     cant 4 0003a15f == 237919 # remaining track time in ms
		///     cast 4 0004a287 == 303751 # total track length in ms
		public override byte[] GetBytes() {
			// Construct a response.
			byte[] payload = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
				StreamInteger(writer, MSTT, this.Mstt);
				StreamInteger(writer, CMSR, this.Cmsr);
				StreamByte(writer, CAPS, this.Caps);
				StreamInteger(writer, CMVO, this.Cmvo);
				StreamByte(writer, CASH, this.Cash);
				StreamByte(writer, CARP, this.Carp);
				StreamByte(writer, CAVC, this.Cavc);
				StreamByte(writer, CAVS, this.Cavs);
				StreamByte(writer, CAFS, this.Cafs);
				StreamByte(writer, CAFE, this.Cafe);
				StreamByte(writer, CAVE, this.Cave);
				StreamByte(writer, CASU, this.Casu);
				
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

			return CreateDACPResponseBytes(CMST ,payload);
		}

	}
}