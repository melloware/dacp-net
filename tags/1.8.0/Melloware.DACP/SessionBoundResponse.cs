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
    /// SessionBoundResponse is an abstract class from which any DACPResponse
    /// that needs to check for a valid Session should extend.  It will handle
    /// checking for a valid SessionId in the HTTP Request.
    /// </summary>
    public abstract class SessionBoundResponse:DACPResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        private static readonly Dictionary<int, SessionInfo> sessions = new Dictionary<int, SessionInfo>();
        private SessionInfo session = null;

        /// <summary>
        /// Constructs an SessionBoundResponse which handles checking if the 
        /// Session is valid andt throws an Exception if it is not.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        public SessionBoundResponse(HttpListenerRequest request):base(request) {
            LOG.Debug("Creating SessionBoundResponse...");

            // in some cases we need to reuse calling the responses internally without an HTTP Request
            if (request == null) {
                LOG.Debug("Using response class internally so skipping session check");
                return;
            }

            this.session = getSession(request);
        }

        /// <summary>
        /// Static method used to validate and retrieve a session or create a new one.
        /// </summary>
        /// <param name="request">the HTTPRequest to use</param>
        /// <returns>the SessionInfo based on the session-id in the request</returns>
        public static SessionInfo getSession(HttpListenerRequest request) {
            int sessionId = ValidateSession(request);
            LOG.DebugFormat("Found SessionId = {0}", sessionId);

            SessionInfo info = null;
            lock(sessions)
                if (!sessions.TryGetValue(sessionId, out info)) {
                    info = new SessionInfo(sessionId);
                    sessions.Add(sessionId, info);
                }
            return info;
        }

        /// <summary>
        /// Static method to increment the CTRL revision which then unlocks the Player response
        /// to update the Remote UI.
        /// </summary>
        public static void IncrementCtrlIntRevision () {
            lock(sessions) {
                foreach (KeyValuePair<int, SessionInfo> kvp in sessions) {
                    SessionInfo session = kvp.Value;
                    session.CtrlIntRevision = session.CtrlIntRevision + 1;
                    LOG.DebugFormat("New CtrlInt Value {0}", session.CtrlIntRevision);
                }
            }
        }
        
        /// <summary>
        /// Static method to increment the DB revision which then unlocks the Session
        /// to have the Remote update its playlists and other things.
        /// </summary>
        public static void IncrementDatabaseRevision () {
            lock(sessions) {
                foreach (KeyValuePair<int, SessionInfo> kvp in sessions) {
                    SessionInfo session = kvp.Value;
                    session.DatabaseRevision = session.DatabaseRevision + 1;
                    LOG.DebugFormat("New Database Revision Value {0}", session.DatabaseRevision);
                }
            }
        }

        /// <summary>
        /// Attempts to validate the session-id from the HTTP Request.
        /// </summary>
        /// <param name="request">the HTTP request</param>
        /// <returns>the SessionId found or -1 if not found</returns>
        private  static int ValidateSession(HttpListenerRequest request) {
            LOG.Debug("Security is enabled, so validating session-id");

            int sessionId = -1;
            try {
                sessionId = Convert.ToInt32(request.QueryString[DACPResponse.PROPERTY_SESSION]);
            } catch (Exception) {
                throw new DACPSecurityException("Session ID not found in HTTP Request.");
            }

            if (sessionId <= 0) {
                throw new DACPSecurityException("Session ID not found in HTTP Request.");
            }

            if (DACPServer.PairingDatabase == null) {
                throw new DACPSecurityException("No valid pairing database found.");
            }

            bool isValidSession = DACPServer.PairingDatabase.ValidateSession(sessionId);
            if (!isValidSession) {
                throw new DACPSecurityException("Session ID is not valid for this DACP Server");
            }

            return sessionId;
        }

        /// <summary>
        /// Close a session out.  Called on /logout from client.
        /// </summary>
        protected void TerminateSession() {
            if (Session == null) return;

            LOG.WarnFormat("Terminating Session: {0}", Session.SessionId);

            // increment both counter to release any pending HTTP requests
            Session.CtrlIntRevision = Session.CtrlIntRevision + 1;
            Session.DatabaseRevision = Session.DatabaseRevision + 1;

            lock (sessions) {
                sessions.Remove(Session.SessionId);
            }

            Session = null;
        }

        /// <summary>
        /// The current session associated with this HTTP request.
        /// </summary>
        public SessionInfo Session {
            get {
                return session;
            } set {
                session = value;
            }
        }
    }
}
