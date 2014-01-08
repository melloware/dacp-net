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

using log4net;

namespace Melloware.DACP {
    /// <summary>
    /// Handles terminating the user session and clearing out the session from cache.
    /// </summary>
    public class LogoutResponse:SessionBoundResponse, INoContentResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs an UpdateResposne which returns revision number of the
        /// server we are using.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public LogoutResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating LogoutResponse...");
            TerminateSession();
        }

        /// <summary>
        /// Return NULL to signify a 204 NO Content Found HTTP Response.
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBytes() {
            return null;
        }
    }
}
