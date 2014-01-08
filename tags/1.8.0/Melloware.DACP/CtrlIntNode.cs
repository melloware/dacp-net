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
	/// A CtrlIntNode is a representation of a single ctrlInt.
	///
	/// mlit  --+
	///       miid   1      01 == 1
	///       cmik   1      01 == 1
	///       cmsp   1      01 == 1
	///       cmsv   1      01 == 1
	///       cass   1      01 == 1
	///       casu   1      01 == 1
	///       caSG   1      01 == 1
	/// </summary>
	public class CtrlIntNode:DACPResponse {
		// logger
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		
		//fields
		public int Miid;    // #Item ID
		public byte Cmik;
		public int Cmpr;
		public int Capr;
		public byte Cmsp;
		public byte AeFR;
		public byte Cmsv;
		public byte Cass;
		public byte Caov;
		public byte Casu;
		public byte Cesg;
		public byte Cmrl;

		/// <summary>
		/// Default constructor
		/// </summary>
		public CtrlIntNode() {
			LOG.Debug("Creating CtrlIntNode...");
			this.Miid = 1;
			this.Cmik = TRUE;
			this.Cmpr = (int)0x00020001;
			this.Capr = (int)0x00020003;
			this.Cmsp = TRUE;
		    this.AeFR = (byte)0x64;
			this.Cmsv = TRUE;
			this.Cass = TRUE;
			this.Caov = TRUE;
			this.Casu = TRUE;
			this.Cesg = TRUE;
			this.Cmrl = TRUE;
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
				StreamInteger(writer, MIID, this.Miid);
				StreamByte(writer, CMIK, this.Cmik);
				StreamInteger(writer, CMPR, this.Cmpr);
				StreamInteger(writer, CAPR, this.Capr);
				StreamByte(writer, CMSP, this.Cmsp);
				StreamByte(writer, AEFR, this.AeFR);
				StreamByte(writer, CMSV, this.Cmsv);
				StreamByte(writer, CASS, this.Cass);
				StreamByte(writer, CAOV, this.Caov);
				StreamByte(writer, CASU, this.Casu);
				StreamByte(writer, CESG, this.Cesg);
				StreamByte(writer, CMRL, this.Cmrl);

				writer.Flush();
				payload = stream.ToArray();
			}
			return CreateDACPResponseBytes(MLIT ,payload);
		}
	}
}
