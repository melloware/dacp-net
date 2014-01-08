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
using System.Collections;
using System.Collections.Generic;

using log4net;

namespace Melloware.DACP {
	/// <summary>
	/// Description of ServerInfoResponse.
	///
	///
	/// Request To Update:
	/// GET /server-info
	///
	/// Response:
	/// msrv --+
	///     mstt   4 000000c8 == 200
	///     mpro   4 00020006 == 131078 #DMAP Protocol Version
	///     apro   4 00030009 == 196617 #DAAP Protocol Version
	///     aeSV   4 00030002 == 196610 #Apple Itunes Music Sharing Version
	///     aeFP   1      02 == 2
	///     ated   2      0003
	///     asgr   2      0003
	///     asse   8      0000000000080000 == 524288
	///     aeMQ   1      01 == 1
	///     aeFR   1      d d
	///     aeTr   1      01 == 1
	///     aeSL   1      01 == 1
	///     aeFP   1      02 == 2
	///     aeSR   1      01 == 1
	///     aeSX   8      000000000000003f == 63
	///     msed   1      01 == 1
	///     msml  --+
	///         msma   8      0000635ed1209ad4 == 109258886650580
	///         msma   8      0000e53c5e209ad4 == 252047439993556
	///     ceWM   0
	///     ceVO   1      00 == 0
	///     minm   21     4d656c6c6f77617265e2809973204c696272617279 Mellowares Library
	///     mslr   1      01 == 1
	///     mstm   4      00000708 == 1800
	///     msal   1      01 == 1
	///     msas   1      03 == 3
	///     msup   1      01 == 1
	///     mspi   1      01 == 1
	///     msex   1      01 == 1
	///     msbr   1      01 == 1
	///     msqy   1      01 == 1
	///     msix   1      01 == 1
	///     msrs   1      01 == 1
	///     msdc   4      00000001 == 1
	///     mstc   4      4ca27603 == 1285715459
	///     msto   4      ffffc7c0 == 4294952896
	///
	/// </summary>
	public class ServerInfoResponse:DACPResponse {
		// logger
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// fields
		public int Mpro; 	// #DMAP Protocol Version
		public int Apro; 	// #DAAP Protocol Version
		public int Aesv; 	// #Apple Itunes Music Sharing Version
		public byte Aefp; 	// #Apple Itunes Unknown (Server Info)
		public ushort Ated; // #Supports Extra Data
		public ushort Asgr; // #Use Groups
		public ulong Asse;
		public byte Aemq;
		public byte Aefr; 	// # Apple Fairplay for allow other users to make selections
		public byte Aetr;
		public byte Aesl;
		public byte Aesr;
		public ulong Aesx;
		public byte Msed; 	// #Supports Edit
		public byte Cewm;   // # Unknown
		public byte Cevo;   // # Supports Volume Control
		public string Minm; // #Server Name
		public byte Mslr; 	// #Login Required
		public int Mstm; 	// #Timeout Interval
		public byte Msal; 	// #Support Auto Logout
		public byte Msas; 	// #Support Authentication Schemes
		public byte Msup; 	// #Support Updates
		public byte Mspi; 	// #Support Persistent ID
		public byte Msex; 	// #Support Extensions
		public byte Msbr; 	// #Support Browse
		public byte Msqy; 	// #Support Query
		public byte Msix; 	// #Support Index
		public byte Msrs; 	// #Support Resolve
		public int Msdc; 	// #Database Count
		public int Mstc; 	// #UTC Time
		public int Msto; 	// #UTC Timezone Offset
		public LinkedList<ulong> Msml = new LinkedList<ulong>();// # speaker machine list


		/// <summary>
		/// Constructs a ServerInfoResponse which returns the information
		/// about the server we are using.
		/// </summary>
		/// <param name="request">the HTTPRequest to use</param>
		public ServerInfoResponse(HttpListenerRequest request):base(request) {
			LOG.DebugFormat("Creating ServerInfoResponse...");
			this.Mpro = 131078; // taken from Itunes 10.1.3
			this.Apro = 196619; // taken from Itunes 10.1.3
			this.Aesv = 196614; // taken from Itunes 10.1.3
			this.Ated = 3;
			this.Asgr = 3;
			this.Asse = (int) 0x80000;
			this.Aemq = TRUE;
		    this.Aefr = (byte)0x64; // 64;  Fairplay user sharing disabled
			this.Aetr = TRUE;
			this.Aesl = TRUE;
			this.Aefp = 0;
			this.Aesr = TRUE;
			this.Aesx = (byte) 0x3f;
			this.Msed = TRUE;
			this.Msml.AddLast(109258886650580);
			this.Msml.AddLast(252047439993556);
			this.Cevo = TRUE;
			this.Minm = Environment.MachineName;
			this.Mslr = TRUE;
			this.Mstm = 1800;
			this.Msal = TRUE;
			this.Msas = 3;
			this.Msup = TRUE;
			this.Mspi = TRUE;
			this.Msex = TRUE;
			this.Msbr = TRUE;
			this.Msqy = TRUE;
			this.Msix = TRUE;
			this.Msrs = TRUE;
			this.Msdc = 1;
			this.Mstc = (int)(DateTime.UtcNow.Ticks/1000);
			this.Msto = (int)(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Ticks/1000);
		}

		/// <summary>
		/// Creates the proper DACP response bytes for this type.
		/// </summary>
		/// <returns>a DACP byte[] array representing this type</returns>
		public override byte[] GetBytes() {
			// Construct a response.
			byte[] payload = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
				StreamInteger(writer, MSTT, this.Mstt);
				StreamInteger(writer, MPRO, this.Mpro);
				StreamInteger(writer, APRO, this.Apro);
				StreamInteger(writer, AESV, this.Aesv);
				StreamShort(writer, ATED, this.Ated);
				StreamShort(writer, ASGR, this.Asgr);
				//StreamLong(writer, ASSE, this.Asse);
				//StreamByte(writer, AEMQ, this.Aemq);
				//StreamByte(writer, AEFR, this.Aefr);
				//StreamByte(writer, AETR, this.Aetr);
				//StreamByte(writer, AESL, this.Aesl);
				//StreamByte(writer, AEFP, this.Aefp);
				//StreamByte(writer, AESR, this.Aesr);
				//StreamLong(writer, AESX, this.Aesx);
				StreamByte(writer, MSED, this.Msed);

				using (MemoryStream spkStream = new MemoryStream()) {
					BinaryWriter bwriter = new BinaryWriter(spkStream, new UTF8Encoding(false));
					if ((this.Msml != null) && (this.Msml.Count > 0)) {
						foreach (ulong speaker in this.Msml) {
							StreamLong(bwriter, MSMA, speaker);
						}

						byte[] listBytes = CreateDACPResponseBytes(MSML, spkStream.ToArray());
						stream.Write(listBytes, 0 , listBytes.Length);
					}
				}

				if (this.Cewm != Byte.MinValue) {
					StreamByte(writer, CEWM, this.Cewm);
				}
				if (this.Cevo != Byte.MinValue) {
					StreamByte(writer, CEVO, this.Cevo);
				}

				StreamString(writer, MINM, this.Minm);
				StreamByte(writer, MSLR, this.Mslr);
				StreamInteger(writer, MSTM, this.Mstm);
				StreamByte(writer, MSAL, this.Msal);
				StreamByte(writer, MSAS, this.Msas);
				StreamByte(writer, MSUP, this.Msup);
				StreamByte(writer, MSPI, this.Mspi);
				StreamByte(writer, MSEX, this.Msex);
				StreamByte(writer, MSBR, this.Msbr);
				StreamByte(writer, MSQY, this.Msqy);
				StreamByte(writer, MSIX, this.Msix);
				StreamByte(writer, MSRS, this.Msrs);
				StreamInteger(writer, MSDC, this.Msdc);
				StreamInteger(writer, MSTC, this.Mstc);
				StreamInteger(writer, MSTO, this.Msto);

				writer.Flush();
				payload = stream.ToArray();
			}
			return CreateDACPResponseBytes(MSRV ,payload);
		}

	}
}
