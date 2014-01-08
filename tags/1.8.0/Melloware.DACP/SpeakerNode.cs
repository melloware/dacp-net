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
	/// A SpeakerNode is a representation of a single speaker set.
	///
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
	public class SpeakerNode:BaseNode {
		// logger
		private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Default constructor
		/// </summary>
		public SpeakerNode() {
			LOG.Debug("Creating SpeakerNode Node...");
			this.Minm = "Computer";
			this.Msma = 0;
			this.Caia = TRUE;
			this.Cmvo = 0; // If the id is 0 then the speaker is for the computer
		}

		public override byte[] GetBytes() {
			// Construct a response.
			byte[] payload = null;
			using (MemoryStream stream = new MemoryStream()) {
				BinaryWriter writer = new BinaryWriter(stream, new UTF8Encoding(false));
				StreamString(writer, MINM, this.Minm);
				StreamLong(writer, MSMA, this.Msma);
				
				// only send CAIA if TRUE
				if (this.Caia == TRUE) {
					StreamByte(writer, CAIA, this.Caia);
				}
				// only send CAIV if TRUE
				if (this.Caiv == TRUE) {
					StreamByte(writer, CAIV, this.Caiv);
				}
				// only send CAIV if TRUE
				if (this.Caip == TRUE) {
					StreamByte(writer, CAIP, this.Caip);
				}
				StreamInteger(writer, CMVO, this.Cmvo);

				writer.Flush();
				payload = stream.ToArray();
			}
			return CreateDACPResponseBytes(MDCL ,payload);
		}

		/// <summary>
		/// Speaker Machine Address.
		/// The value for msma is the id of the speaker.  If the id is 0 then the speaker is for the computer
		/// </summary>
		public ulong Msma {
			get;
			set;
		}

		/// <summary>
		/// Computer audio is available flag. (only ever have seen 1 for true) this is 1 when the speaker is being used.  
		/// In Itunes 11 if a video is playing and it is playing on the computer this is not really used.  
		/// It will show up on other speakers… iTunes 11 uses different information when a video is playing as opposed to a song
		/// </summary>
		public byte Caia {
			get;
			set;
		}
		
		/// <summary>
		/// Denotes the speaker is capable of playing video – the computer speaker will not have this field.  
		/// It is only for real speakers… The computer is always able to play video
		/// </summary>
		public byte Caiv {
			get;
			set;
		}
		
		/// <summary>
		/// Basically like the CAIA active call from above but only used for videos
		/// </summary>
		public byte Caip {
			get;
			set;
		}


		/// <summary>
		/// Current volume of the speaker set.
		/// is the relative volume of the speaker in relation to master volume (what is shown in iTunes outside of the speaker chooser)
		/// </summary>
		public int Cmvo {
			get;
			set;
		}

	}
}
