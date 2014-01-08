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

using log4net;

namespace Melloware.DACP {
	/// <summary>
	/// A TrackNode is a representation of a single track.
	///
	/// dmap.itemname,dmap.itemid,daap.songartist,daap.songalbum,dmap.containeritemid,
	/// com.apple.itunes.has-video,daap.songuserrating,daap.songtime
	///              mlit  --+
	///                    mikd   1      02 == 2
	///                    asal   16     Almost Killed Me
	///                    asar   11     Hold Steady
	///                    astm   4      00030be5 == 199653
	///                    miid   4      000002a7 == 679
	///                    minm   12     Positive Jam
	///                    mcti   4      00000373 == 883
	///                    aeHV   1      00 == 0 # Itunes Has Video
	///                    asai   8      44ba43b77c9e1a8d == 4952345195596094093
	///              mlit  --+
	///                    mikd   1      02 == 2
	///                    asal   25     Boys And Girls In America
	///                    asar   11     Hold Steady
	///                    astm   4      0003915e == 233822
	///                    miid   4      000001b5 == 437
	///                    minm   14     Hot Soft Light
	///                    mcti   4      00000384 == 900
	///                    aeHV   1      00 == 0 # Itunes Has Video
	///                    asai   8      593042020b1764f5 == 6426709244801148149
	/// </summary>
	public class TrackNode:BaseNode {
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public TrackNode() {
			this.AeHV = 0;
			this.AeMK = 0;
			this.Asai = 0;
			this.Asal = "Album";
			this.Asar = "Artist";
			this.Asco = FALSE;
			this.Asdb = 0;
			this.Asdn = 1;
			this.Asgn = "Unknown"; // genre
			this.Asri = 0;
			this.Astm = 3000000;
			this.Astn = 1;
			this.Asur = 0;
			this.Asyr = 1970;
			this.CeJI = 0;
			this.CeJV = 0;
			this.Miid = 0;
			this.Mikd = 1;
			this.Minm = "Track";
		}

		public TrackNode(int Id, string artist, string album, string title, ulong albumId, int trackLength, ushort trackNumber, byte rating) : this() {
			LOG.DebugFormat("Track: {0} - {1}", trackNumber, title);
			this.Asai = albumId;
			this.Asal = album;
			this.Asar = artist;
			this.Astm = trackLength;
			this.Astn = trackNumber;
			this.Asur = rating;
			this.Miid = Id;
			this.Mikd = 1;
			this.Minm = title;
		}

		/// <summary>
		/// Returns this reponse in binary format.
		/// </summary>
		/// <returns>the bytes that represent this response</returns>
		public override byte[] GetBytes() {
			// Construct a response.
			byte[] payload = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
				StreamByte(writer, MIKD, this.Mikd);
				StreamString(writer, ASAL, this.Asal);
				StreamString(writer, ASAR, this.Asar);
				StreamByte(writer, ASUR, this.Asur);
				StreamInteger(writer, ASDN, this.Asdn);
				if (this.Astm != UInt32.MinValue) {
					StreamInteger(writer, ASTM, this.Astm);
				}
				if (this.Astn != UInt16.MinValue) {
					StreamShort(writer, ASTN, this.Astn);
				}
				if (this.Mcti != UInt16.MinValue) {
					StreamInteger(writer, MCTI, this.Mcti);
				}
				if (this.Asyr != UInt32.MinValue) {
					StreamInteger(writer, ASYR, this.Asyr);
				}
				if (this.Asai != UInt16.MinValue) {
					StreamLong(writer, ASAI, this.Asai);
				}
				if (this.Asri != UInt16.MinValue) {
					StreamLong(writer, ASRI, this.Asri);
				}
				if (this.Asdb != Byte.MinValue) {
					StreamByte(writer, ASDB, this.Asdb);
				}
				if (this.AeMK != UInt16.MinValue) {
					StreamInteger(writer, AEMK, this.AeMK);
				}
				if (this.CeJI != UInt16.MinValue) {
					StreamInteger(writer, CEJI, this.CeJI);
				}
				if (this.CeJV != UInt16.MinValue) {
					StreamInteger(writer, CEJV, this.CeJV);
				}
				if (this.Asco != UInt16.MinValue) {
					StreamShort(writer, ASCO, this.Asco);
				}
				StreamByte(writer, AEHV, this.AeHV);
				StreamInteger(writer, MIID, this.Miid);
				StreamString(writer, MINM, this.Minm);
				StreamString(writer, ASGN, this.Asgn);
				StreamString(writer, ASSA, this.Assa);
				StreamString(writer, ASSU, this.Assu);

				writer.Flush();
				payload = stream.ToArray();
			}
			return CreateDACPResponseBytes(MLIT ,payload);
		}

		/// <summary>
		/// #Item Kind (song = 1)
		/// </summary>
		public byte Mikd {
			get;
			set;
		}

		/// <summary>
		/// #Song Album name
		/// </summary>
		public string Asal {
			get;
			set;
		}

		/// <summary>
		/// #Song Artist name
		/// </summary>
		public string Asar {
			get;
			set;
		}
		
		/// <summary>
		/// #Song Sort by Artist
		/// </summary>
		public string Assa {
			get;
			set;
		}
		
		/// <summary>
		/// #Song Sort by Album
		/// </summary>
		public string Assu {
			get;
			set;
		}

		/// <summary>
		/// // #Song Time
		/// </summary>
		public int Astm {
			get;
			set;
		}

		/// <summary>
		/// #Song Track Number
		/// </summary>
		public ushort Astn {
			get;
			set;
		}

		/// <summary>
		/// #song user rating (0-100)
		/// </summary>
		public byte Asur {
			get;
			set;
		}

		/// <summary>
		/// #playlist item id (used only in Playlist lists)
		/// </summary>
		public int Mcti {
			get;
			set;
		}

		/// <summary>
		///  # Itunes Has Video (should be 0 for music)
		/// </summary>
		public byte AeHV {
			get;
			set;
		}

		/// <summary>
		/// #Song Album ID of parent album
		/// </summary>
		public ulong Asai {
			get;
			set;
		}
		
		/// <summary>
		/// #Song Artist ID of parent album
		/// </summary>
		public ulong Asri {
			get;
			set;
		}
		
		/// <summary>
		/// #Song Disabled
		/// </summary>
		public byte Asdb {
			get;
			set;
		}
		
		/// <summary>
		/// #com.apple.itunes.extended-media-kind
		/// </summary>
		public int AeMK {
			get;
			set;
		}
		/// <summary>
		/// #com.apple.itunes.jukebox-current
		/// </summary>
		public int CeJI {
			get;
			set;
		}
		/// <summary>
		/// #com.apple.itunes.jukebox-vote
		/// </summary>
		public int CeJV {
			get;
			set;
		}
		
		/// <summary>
		/// daap.songyear
		/// </summary>
		public int Asyr {
			get;
			set;
		}
		
		/// <summary>
		/// daap.songgenre
		/// </summary>
		public string Asgn {
			get;
			set;
		}
		
		/// <summary>
		/// daap.discnumber
		/// </summary>
		public int Asdn {
			get;
			set;
		}

		/// <summary>
		/// daap.songcompilation (Part of A Compilation)
		/// </summary>
		public byte Asco {
			get;
			set;
		}
	}
}
