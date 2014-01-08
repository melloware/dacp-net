/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;

namespace Melloware.DACP {
    /// <summary>
    /// Thrown if there is any problem in the pairing process.
    /// </summary>
    public class DACPPairingException : System.Exception {
        public DACPPairingException() {
        }

        public DACPPairingException(string message): base(message) {
        }

        public DACPPairingException(string message,
                                    Exception innerException): base(message, innerException) {
        }
    }
}
