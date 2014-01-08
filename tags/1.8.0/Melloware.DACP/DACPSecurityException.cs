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
    /// Thrown when either a pairing-guid is not found or a session-id
    /// is not found as a valid param for this running DACP Server.
    /// </summary>
    public class DACPSecurityException : System.Exception {
        public DACPSecurityException() {
        }

        public DACPSecurityException(string message): base(message) {
        }

        public DACPSecurityException(string message,
                                     Exception innerException): base(message, innerException) {
        }
    }
}
