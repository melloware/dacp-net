/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Melloware.Core;

namespace Melloware.DACP {

	/// <summary>
	/// Parses a Pairing reply from the device.
	/// </summary>
    public class PairingReply {

        private ulong pairingGuid = 0 ;
        private string name;
        private string type;

       /// <summary>
       /// Parses the bytes of the client device pairing reply.
       /// </summary>
       /// <param name="data">The bytes received from the HTTP reply of client device</param>
        public PairingReply(byte[] data) {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < 24)
                throw new ArgumentException("PairingReply Data is too short", "data");

            string rootName = Encoding.ASCII.GetString(data, 0, 4);
            if (rootName != "cmpa")
                throw new ArgumentException("PairingReply Invalid Response did not contain 'cmpa'", "data");

            for (int offset = 8; offset < data.Length; ) {
                string code = Encoding.ASCII.GetString(data, offset, 4);
                int length = Endian.ConvertInt32(BitConverter.ToInt32(data, offset + 4));

                switch (code) {
                case "cmpg":
                    if (length != 8)
                        throw new ArgumentException("Invalid pairing GUID length", "data");

                    byte[] lBytes = new byte[8];
                    Array.Copy(data, 16, lBytes, 0, 8);
                    Array.Reverse(lBytes);
                    pairingGuid = BitConverter.ToUInt64(lBytes,0);
                    break;

                case "cmnm":
                    name = new UTF8Encoding(false).GetString(data, offset + 8, length);
                    break;

                case "cmty":
                    type = new UTF8Encoding(false).GetString(data, offset + 8, length);
                    break;
                }

                offset += length + 8;
            }

            if (pairingGuid == 0)
                throw new ArgumentException("No GUID in reply", "data");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("No device name in reply", "data");
        }

        public ulong PairingGuid {
            get {
                return pairingGuid;
            }
            private set {
                pairingGuid = value;
            }
        }

        public string Name {
            get {
                return name;
            }
            private set {
                name = value;
            }
        }

        public string Type {
            get {
                return type;
            }
            private set {
                type = value;
            }
        }

    }
}
