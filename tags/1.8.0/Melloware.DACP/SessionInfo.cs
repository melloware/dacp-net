/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// PONO for holding the session revision information.
    /// </summary>
    public class SessionInfo {
        public const int SLEEP_INTERVAL = 200;
        // fields
        private int sessionId = 0;
        private int ctrlIntRevision = 1;
        private int databaseRevision = 1;

        public SessionInfo(int aSessionId) {
            sessionId = aSessionId;
            ctrlIntRevision = 1;
            databaseRevision = 1;
        }

        public int SessionId {
            get {
                return sessionId;
            }
            private set {
                sessionId = value;
            }
        }

        public int CtrlIntRevision {
            get {
                return ctrlIntRevision;
            } set {
                ctrlIntRevision = value;
            }
        }

        public int DatabaseRevision {
            get {
                return databaseRevision;
            } set {
                databaseRevision = value;
            }
        }
    }
}
