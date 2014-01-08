/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

using System.Runtime.InteropServices;
using System.Windows.Forms;

using log4net;
using ZeroconfService;
using Melloware.Core;

namespace Melloware.DACP {
    /// <summary>
    /// DACPServer used to act like an Itunes Server.
    /// It listens on Port 3689 for HTTP requests and publishes mDNS service
    /// for "_touch-remote._tcp".  This is the only class that should be subclassed
    /// by any implementations of a DACP Server.
    /// </summary>
    [ComVisible(true),ClassInterface(ClassInterfaceType.AutoDispatch)]
    public abstract class DACPServer {
        // constants
        public const string TOUCHSERVICE_TYPE = "_touch-able._tcp";
        public const string WEBSERVICE_TYPE = "_http._tcp";
        public const string DACPSERVICE_TYPE = "_dacp._tcp";
        public const string DAAPSERVICE_TYPE = "_daap._tcp";

        // iTunes constants
        protected const string ITUNES_MEDIAKIND_MUSIC = "1";
        protected const string ITUNES_MEDIAKIND_VIDEO = "2";
        protected const string ITUNES_MEDIAKIND_PODCAST = "4";
        protected const string ITUNES_MEDIAKIND_AUDIOBOOK = "8";

        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // fields
        public static int PORT = 3689;
        public static DACPPairingDatabase PairingDatabase = null;
        public static DACPPairingServer PairingServer = null;
        protected static readonly HttpListener httpServer = new HttpListener();
        protected static readonly System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);
        protected static readonly LinkedList<CountDownLatch> latches = new LinkedList<CountDownLatch>();
        protected static readonly Dictionary<string,string> utf16Map = new Dictionary<string,string>();
        protected static Boolean ClearQueue = false;
        private NetService touchAbleService = null;
        private NetService webService = null;
        private NetService dacpService = null;
        private NetService daapService = null;
        private string version;
        private string serviceName;


        public DACPServer() {
            LOG.Info("Initializing DACPServer");
            this.version = GetVersion();
        }

        /// <summary>
        /// Starts the DACP Server including any mDNS Services.
        /// </summary>
        protected virtual void Start() {
            LOG.InfoFormat("Starting DACPServer...");
            try {
                bool portAvailable = false;
                do {
                	portAvailable = FindNextAvailablePort(PORT);
                	if (portAvailable == false) {
                		PORT = PORT + 1;
                	}
                } while (portAvailable == false);

                PairingDatabase = DACPPairingDatabase.Initialize(this);
                PairingServer = new DACPPairingServer(this);
                PublishMdnsServices();
                StartNonblockingHttpListener();
            } catch (DACPBonjourException bex) {
                LOG.Error(this.GetApplicationName() + " Bonjour Error: " + bex.Message);
                throw bex;
            } catch (Exception ex) {
            	LOG.Error(this.GetApplicationName() + "Unexpected Exception caught: " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Checks the Port # to see if it is in use already
        /// </summary>
        /// <param name="port">the port # to check</param>
        /// <returns>true if available, false if not available</returns>
        private bool FindNextAvailablePort(int port) {
            LOG.InfoFormat("Checking Port {0}", port);
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray) {
            	if (endpoint.Port == port) {
                    isAvailable = false;
                    break;
                }
            }

            LOG.InfoFormat("Port {0} available = {1}", port, isAvailable);

            return isAvailable;
        }

        /// <summary>
        /// Close the listener and stop the mDNS service.
        /// </summary>
        protected virtual void Stop() {
            try {
                LOG.Info("Shutting down DACPServer...");
                ReleaseAllLatches();

                if (PairingDatabase != null) {
                    PairingDatabase.Store();
                }
                if (touchAbleService != null) {
                    touchAbleService.Stop();
                }
                if (webService != null) {
                    webService.Stop();
                }
                if (dacpService != null) {
                    dacpService.Stop();
                }
                if (daapService != null) {
                    daapService.Stop();
                }
                if (PairingServer != null) {
                    PairingServer.Stop();
                }
                if (httpServer != null) {
                    httpServer.Stop();
                }
            } catch (Exception ex) {
                LOG.Error("DACP Server Shutdown Error: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Callback called whenever clients are discovered through mDNS.
        /// </summary>
        public abstract void OnClientListChanged();

        /// <summary>
        /// Application name overridden by all subclasses.
        /// </summary>
        /// <returns>the name of this DACP Server</returns>
        public abstract string GetApplicationName();

        protected abstract DACPResponse PlaylistAdd(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistRemove(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistRename(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistRefresh(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistAddTrack(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistRemoveTrack(HttpListenerRequest request);
        protected abstract DACPResponse PlaylistMoveTrack(HttpListenerRequest request);
        protected abstract DACPResponse GetAlbums(HttpListenerRequest request);
        protected abstract DACPResponse GetArtists(HttpListenerRequest request);
        protected abstract DACPResponse GetArtworkResponse(HttpListenerRequest request);
        protected abstract DACPResponse GetComposers(HttpListenerRequest request);
        protected abstract DACPResponse GetCurrentPlayerStatus(HttpListenerRequest request);
        protected abstract DACPResponse GetDatabaseInfo(HttpListenerRequest request);
        protected abstract DACPResponse GetGenres(HttpListenerRequest request);
        protected abstract DACPResponse GetNowPlaying(HttpListenerRequest request);
        protected abstract DACPResponse GetPlaylists(HttpListenerRequest request);
        protected abstract DACPResponse GetPlaylistTracks(HttpListenerRequest request);
        protected abstract DACPResponse GetProperty(HttpListenerRequest request);
        protected abstract DACPResponse GetTracks(HttpListenerRequest request);
        protected abstract DACPResponse GetUpdate(HttpListenerRequest request);
        protected abstract DACPResponse GetSpeakers(HttpListenerRequest request);
        protected abstract DACPResponse SetProperty(HttpListenerRequest request);
        protected abstract void ControlClearQueue(HttpListenerRequest request);
        protected abstract void ControlNextItem(HttpListenerRequest request);
        protected abstract void ControlPause(HttpListenerRequest request);
        protected abstract void ControlPlayPause(HttpListenerRequest request);
        protected abstract void ControlPreviousItem(HttpListenerRequest request);
        protected abstract void ControlRepeat(HttpListenerRequest request);
        protected abstract void ControlShuffle(HttpListenerRequest request);
        protected abstract void ControlStop(HttpListenerRequest request);
        protected abstract void ControlFastForward(HttpListenerRequest request);
        protected abstract void ControlRewind(HttpListenerRequest request);
        protected abstract void ControlPlayResume(HttpListenerRequest request);
        protected abstract void ControlGeniusSeed(HttpListenerRequest request);
        protected abstract void QueueTracks(HttpListenerRequest request, bool clearQueue, bool beginPlaying);
        protected abstract void SetPlaylist(HttpListenerRequest request);
        protected abstract void SetSpeakers(HttpListenerRequest request);
        protected abstract void Logout(HttpListenerRequest request);
        public abstract void RefreshCache();

        /// <summary>
        /// Hash code of servername used in mDNS and in pairing.
        /// </summary>
        public string ServiceName {
            get {
                return serviceName;
            } set {
                serviceName = value;
            }
        }

        /// <summary>
        /// The version string for this server.
        /// </summary>
        public string Version {
            get {
                return version;
            } set {
                version = value;
            }
        }

        /// <summary>
        /// Creates a NonBlocking listener to handle each HTTP request in a thread.
        /// </summary>
        private void StartNonblockingHttpListener() {
            LOG.InfoFormat("Starting Listener http://+:{0}/...", PORT);
            httpServer.Prefixes.Add("http://+:"+PORT+"/");
            httpServer.Start();
            // Move our listening loop off to a worker thread so that the GUI doesn't lock up.
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Listen));
        }

        /// <summary>
        /// Loop here to begin processing of new requests.
        /// </summary>
        /// <param name="state">Thread object state</param>
        private void Listen(object state) {
        	try {
        		while (httpServer.IsListening) {
        			httpServer.BeginGetContext(new AsyncCallback(ListenerCallback), httpServer);
        			listenForNextRequest.WaitOne();
        		}
        	} catch (Exception httpEx) {
        		LOG.Error("HTTP Listen Error: " + httpEx.Message, httpEx);
        	}
        }


        /// <summary>
        /// Callback when a HTTP request comes in on the port listener and is handed off
        /// to a thread for processing.  This method
        /// </summary>
        /// <param name="result">IAsyncResult containing the HTTPListener</param>
        protected void ListenerCallback(IAsyncResult result) {
            try {
                HttpListener listener = (HttpListener) result.AsyncState;
                HttpListenerContext context = null;
                if (listener == null) {
                    LOG.WarnFormat("Listener null so returning...");
                    return;
                }

                try {
                    // The EndGetContext() method, as with all Begin/End asynchronous methods in the .NET Framework,
                    // blocks until there is a request to be processed or some type of data is available.
                    context = listener.EndGetContext(result);
                } catch (Exception ex) {
                    // You will get an exception when httpListener.Stop() is called
                    // because there will be a thread stopped waiting on the .EndGetContext()
                    // method, and again, that is just the way most Begin/End asynchronous
                    // methods of the .NET Framework work.
                    LOG.WarnFormat("HttpListener Stopped: {0}", ex.Message);
                    ReleaseAllLatches();
                    return;
                } finally {
                    // Once we know we have a request (or exception), we signal the other thread
                    // so that it calls the BeginGetContext() (or possibly exits if we're not
                    // listening any more) method to start handling the next incoming request
                    // while we continue to process this request on a different thread.
                    listenForNextRequest.Set();
                }

                if (context == null) return;

                LOG.DebugFormat("HTTP START: {0}", DateTime.Now.ToString());

                System.Net.HttpListenerRequest request = context.Request;
                LOG.InfoFormat("{0}: {1}", PORT, request.RawUrl);
                if (request.HasEntityBody) {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(request.InputStream, request.ContentEncoding)) {
                        string requestData = sr.ReadToEnd();
                    }
                }
                
                if (LOG.IsDebugEnabled) {
                	LOG.DebugFormat("    HTTP User-Agent: {0}", request.UserAgent);
                	foreach ( String s in request.Headers.AllKeys )
                		LOG.DebugFormat("    Header {0,-10} {1}", s, request.Headers[s] );
                }
               
                

                // determine if the client is requesting a compressed response
                string acceptEncoding = request.Headers["Accept-Encoding"];
                bool isCompressed = (!string.IsNullOrEmpty(acceptEncoding) && (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate")));
                LOG.DebugFormat("Accept-Encoding: {0} Compressed: {1}", acceptEncoding, isCompressed);

                // Obtain a response object
                using (System.Net.HttpListenerResponse response = context.Response) {
                    try {
                        response.ContentType = "application/x-dmap-tagged";
                        response.AddHeader("DAAP-Server", this.GetApplicationName() + " " + this.Version);
                        this.DispatchRequest(request, response, isCompressed);
                    } catch (DACPSecurityException ex) {
                        LOG.Error("DACP Security Error: " + ex.Message);
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        response.OutputStream.WriteByte(0);
                    } catch (Exception ex) {
                        LOG.Error("DACP Server Error: " + ex.Message);
                        response.StatusCode = DACPResponse.MSTT_NO_CONTENT;
                        response.OutputStream.WriteByte(0);
                    }
                }
            } catch (Exception httpEx) {
                LOG.Error("DACP Server Error: " + httpEx.Message, httpEx);
            }


            LOG.DebugFormat("HTTP END: {0}", DateTime.Now.ToString());
        }

        private void DispatchRequest(HttpListenerRequest request, HttpListenerResponse response, bool isCompressed) {
            DACPResponse dacpResponse = null;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start(); 
            // based on the URL, figure out what the client wanted and return it
             string url = request.RawUrl.ToLower();

            try {
                // first handle either login or security check
                if (url.StartsWith("/login")) {
                    dacpResponse = Login(request);
                    ReleaseAllLatches();
                } else if (url.StartsWith("/server-info")) {
                    ReleaseAllLatches();
                    dacpResponse = new ServerInfoResponse(request);
                } else if (url.Equals("/ctrl-int")) {
                    ReleaseAllLatches();
                    dacpResponse = new CtrlIntResponse(request);
                } else if (url.StartsWith("/databases?session-id=")) {
                    dacpResponse = GetDatabaseInfo(request);
                } else if (url.StartsWith("/update")) {
                	stopWatch = null;
                    dacpResponse = GetUpdate(request);
                } else if (url.StartsWith("/logout")) {
                    dacpResponse = new LogoutResponse(request);
                    Logout(request);
                } else if (url.StartsWith("/fp-setup")) {
                    dacpResponse = new FairPlayResponse(request);
                } else if (url.StartsWith("/refreshcache")) {
                    RefreshCache();
                } else if (url.StartsWith("/databases/")) {
                    if (url.Contains("daap.baseplaylist")) {
                        dacpResponse = GetPlaylists(request);
                    } else if (url.Contains("artwork")) {
                        dacpResponse = GetArtworkResponse(request);
                    } else if (url.Contains("browse/artists")) {
                        dacpResponse = GetArtists(request);
                    } else if (url.Contains("browse/genres")) {
                        dacpResponse = GetGenres(request);
                    } else if (url.Contains("browse/composers")) {
                        dacpResponse = GetComposers(request);
                    } else if (url.Contains("groups?") && url.Contains("type=music&group-type=albums")) {
                        dacpResponse = GetAlbums(request);
                    } else if (url.Contains("groups?") && url.Contains("type=music&group-type=artists")) {
                        dacpResponse = GetArtists(request);
                    } else if (url.Contains("items?") && url.Contains("query=")) {
                        dacpResponse = GetTracks(request);
                    } else if (url.Contains("containers")) {
                        if (url.Contains("action=add")) {
                            dacpResponse = PlaylistAddTrack(request);
                        } else if (url.Contains("action=remove")) {
                            dacpResponse = PlaylistRemoveTrack(request);
                        } else if (url.Contains("action=move")) {
                            dacpResponse = PlaylistMoveTrack(request);
                        } else if (url.Contains("action=rename")) {
                            dacpResponse = PlaylistRename(request);
                            SessionBoundResponse.IncrementDatabaseRevision();
                        } else if (url.Contains("action=refresh")) {
                            dacpResponse = PlaylistRefresh(request);
                            SessionBoundResponse.IncrementDatabaseRevision();
                        } else {
                            dacpResponse = GetPlaylistTracks(request);
                        }
                    } else if (url.Contains("edit?action=add")) {
                        dacpResponse = PlaylistAdd(request);
                        SessionBoundResponse.IncrementDatabaseRevision();
                    } else if (url.Contains("edit?action=remove")) {
                        dacpResponse = PlaylistRemove(request);
                        SessionBoundResponse.IncrementDatabaseRevision();
                    }
                } else if (url.StartsWith("/ctrl-int/")) {
                    if (url.Contains("playstatusupdate")) {
                		stopWatch = null;
                        dacpResponse = GetCurrentPlayerStatus(request);
                    } else if (url.Contains(ArtworkResponse.PROPERTY_NOW_PLAYING)) {
                        dacpResponse = GetArtworkResponse(request);
                    } else if (url.Contains("cue?command=clear")) {
                        if (PairingDatabase.RespectClearCueCommand) {
                            ClearQueue = true;
                            ControlClearQueue(request);
                        } else {
                            ClearQueue = false;
                            LOG.Info("Clearing Playlist Cue disabled by RespectClearCueCommand in XML properties!");
                        }
                    } else if (url.Contains("cue?command=play")) {
                		// check for the clear flag
                		if (PairingDatabase.RespectClearCueCommand) {
                			if (url.Contains("clear-first")) {
                				ClearQueue = true;
                			}
                        } 
                        QueueTracks(request, ClearQueue, true);
                    } else if (url.Contains("cue?command=add")) {
                        QueueTracks(request, false, false);
                    } else if (url.Contains("playspec?")) {
                        SetPlaylist(request);
                    } else if(url.Contains("getproperty")) {
                        dacpResponse = GetProperty(request);
                    } else if(url.Contains("setproperty")) {
                        dacpResponse = SetProperty(request);
                    } else if (url.Contains("playpause")) {
                        ControlPlayPause(request);
                    } else if (url.Contains("pause")) {
                        ControlPause(request);
                    } else if (url.Contains("stop")) {
                        ControlStop(request);
                    } else if (url.Contains("nextitem")) {
                        ControlNextItem(request);
                    } else if (url.Contains("previtem")) {
                        ControlPreviousItem(request);
                    } else if (url.Contains("beginff")) {
                        ControlFastForward(request);
                    } else if (url.Contains("beginrew")) {
                        ControlRewind(request);
                    } else if (url.Contains("playresume")) {
                        ControlPlayResume(request);
                    } else if (url.Contains("items")) {
                        dacpResponse = GetNowPlaying(request);
                    } else if (url.Contains("getspeakers")) {
                        dacpResponse = GetSpeakers(request);
                    } else if (url.Contains("setspeakers")) {
                        SetSpeakers(request);
                    } else if (url.Contains("set-genius-seed")) {
                        ControlGeniusSeed(request);
                    } else {
                        LOG.WarnFormat("Unknown URL type: {0}", request.RawUrl);
                    }
                } else {
                    LOG.WarnFormat("Unknown URL type: {0}", request.RawUrl);
                }

                // return a NO Content found if either null or is marker interface
                if ((dacpResponse == null) || (dacpResponse is INoContentResponse)) {
                    response.StatusCode = DACPResponse.MSTT_NO_CONTENT;
                    return;
                }
                
                // return a 500 Internal Server Error
                if  (dacpResponse is IErrorResponse) {
                	LOG.Warn("500 Internal Server Error Response");
                    response.StatusCode = DACPResponse.MSTT_ERROR;
                    return;
                }

                // now output the DACPResponse as a binary byte message
                byte[] responseBytes = dacpResponse.GetBytes();
                if (isCompressed) {
                    response.AppendHeader("Content-Encoding", "gzip");
                    using (MemoryStream memoryStream = new MemoryStream(8092))  {
                        // Decide regular stream or gzip stream based on whether the response can be compressed or not
                        using (Stream writer = new GZipStream(memoryStream, CompressionMode.Compress))   {
                            writer.Write(responseBytes, 0, responseBytes.Length);
                        }
                        responseBytes = memoryStream.ToArray();
                    }
                } else {
                    response.AppendHeader("Content-Encoding", "utf-8");
                    response.ContentEncoding = Encoding.Unicode;
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
            } catch (Exception ex) {
                LOG.Error(this.GetApplicationName() + " Error: " + ex.Message, ex);
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            } finally {
             	if (stopWatch != null) {
             		// Get the elapsed time as a TimeSpan value.
             		TimeSpan ts = stopWatch.Elapsed;
             		if (ts.TotalMilliseconds > 2500) {
             			LOG.WarnFormat("DACP Response Time Exceeded Threshold: {0,21} '{1}'", ts.TotalMilliseconds, url);
             		} else {
             			LOG.DebugFormat("DACP Response Time: {0,21}", ts.TotalMilliseconds);
             		}
             	}
            }
        }




        /// <summary>
        /// When the user does something on the PC Server music player like
        /// Pause or Play or change tracks then we need to release all waiting
        /// HTTP requests from DACP Clients that have been blocking.
        /// </summary>
        public static void ReleaseAllLatches() {
            LOG.DebugFormat("Releasing {0} CountDownLatches", latches.Count);
            foreach (CountDownLatch latch in latches) {
                latch.CountDown();
            }
            latches.Clear();

            // notify all connected clients to update their UI
            SessionBoundResponse.IncrementCtrlIntRevision();
        }

        /// <summary>
        /// Attempts to get the LoginResponse by validating the pairing
        /// GUID passed in.
        /// </summary>
        /// <param name="request">the HTTP Request</param>
        /// <returns>a LoginResponse or throws a security execption</returns>
        protected LoginResponse Login(HttpListenerRequest request) {
            LoginResponse login = new LoginResponse(request);

            if (!PairingDatabase.DACPClients.ContainsKey(login.Guid)) {
                // create a latch and wait 5 seconds for a response
                CountDownLatch latch = new CountDownLatch(1);
                latches.AddLast(latch);
                latch.Await(5000);
            }

            // validate the guid and get the session id back
            login.Mlid = PairingDatabase.ValidateGuid(login.Guid);
            return login;
        }

        /// <summary>
        /// Returns the current version of the Melloware ZWave Commander.
        /// </summary>
        /// <returns>a version string</returns>
        private static string GetVersion() {
            string version = "Unknown";
            Assembly asm = Assembly.GetAssembly( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
            if (asm != null) {
                AssemblyName asmName = asm.GetName();
                version = String.Format("{0}", asmName.Version );
                LOG.Info(version);
            }
            return version;
        }

        /// <summary>
        /// Gets the local IPV4 address to display to the user to enter in their mobile device.
        /// </summary>
        /// <returns>the IPv4 Address of this machine</returns>
        public static IPAddress GetLocalIPAddress() {
            IPAddress result = null;
            String strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Grab the first IP addresses
            foreach(IPAddress ipaddress in iphostentry.AddressList) {
                if (ipaddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    LOG.InfoFormat("IP Address : {0}", ipaddress.ToString());
                    result = ipaddress;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Converts and IP Address to a ulong
        /// </summary>
        /// <param name="ip">the IPAddress to convert</param>
        /// <returns>a ulong representation of the IPAddress</returns>
        public static ulong IPToLong(IPAddress ip) {
            return (ulong)((ip.GetAddressBytes()[0] << 24) | (ip.GetAddressBytes()[1] << 16) | (ip.GetAddressBytes()[2] << 8) | ip.GetAddressBytes()[3]);
        }

        /// <summary>
        /// Publishes the mDNS sevrice to pretend like we are a client device
        /// like an iPod Touch.  Service is "_touch-able._tcp" and DBId is
        /// the important value here. CtlN is the name that represents this
        /// server on the iPhone/Android device.
        /// </summary>
        private void PublishMdnsServices() {
            LOG.Debug("Publishing mDNS Service...");
            try {
                LOG.DebugFormat("mDNS Version: {0}", NetService.DaemonVersion);
                string name = this.GetApplicationName() +" "+ Environment.MachineName;
                string hash = name.GetHashCode().ToString("X");
                hash = hash+hash;
                if (hash.Length <= 16) {
                    this.ServiceName = hash;
                } else {
                    this.ServiceName = (hash+hash).Substring(0,15);
                }

                String domain = "";
                touchAbleService = new NetService(domain, TOUCHSERVICE_TYPE, this.ServiceName, PORT);
                touchAbleService.AllowMultithreadedCallbacks = true;
                touchAbleService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
                touchAbleService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);

                webService = new NetService(domain, WEBSERVICE_TYPE, name, PORT);
                webService.AllowMultithreadedCallbacks = true;
                webService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
                webService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);
                
                dacpService = new NetService(domain, DACPSERVICE_TYPE, name, PORT);
                dacpService.AllowMultithreadedCallbacks = true;
                dacpService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
                dacpService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);

                daapService = new NetService(domain, DAAPSERVICE_TYPE, name, PORT);
                daapService.AllowMultithreadedCallbacks = true;
                daapService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
                daapService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);

                /* Touchable-Service RECORD */
                System.Collections.Hashtable dict = new System.Collections.Hashtable();
                dict.Add("CtlN",name);
                dict.Add("OSsi","0x1F5");
                dict.Add("Ver","131075");
                dict.Add("txtvers","1");
                dict.Add("DvTy", this.GetApplicationName());
                dict.Add("DvSv","2049");
                dict.Add("DbId", this.ServiceName);
                touchAbleService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dict);
                touchAbleService.Publish();
                
                /* DACP-Service RECORD */
                System.Collections.Hashtable dictDacp = new System.Collections.Hashtable();
                dictDacp.Add("OSsi","0x1F5");
                dictDacp.Add("Ver","131075");
                dictDacp.Add("txtvers","1");
                dictDacp.Add("DbId", this.ServiceName);
                dacpService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dictDacp);
                dacpService.Publish();

                /* DAAP-Service RECORD */
                System.Collections.Hashtable dictDaap = new System.Collections.Hashtable();
                dictDaap.Add("Machine Name",name);
                dictDaap.Add("iTSh Version","131073");
                dictDaap.Add("Version","196615");
                dictDaap.Add("txtvers","1");
                dictDaap.Add("Password","false");
                dictDaap.Add("DvSv","2049");
                dictDaap.Add("Database ID", this.ServiceName);
                dictDaap.Add("Machine ID", this.ServiceName);
                dictDaap.Add("DbId", this.ServiceName);
                daapService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dictDaap);
                //daapService.Publish();

                /* HTTP-Service has no service record */
                webService.Publish();
            } catch (Exception ex) {
                LOG.Error("Error publishing mDNS Services", ex);
            }
        }

        /// <summary>
        /// Callback when the service is published using mDNS.
        /// </summary>
        /// <param name="service">the NetService published</param>
        void publishService_DidPublishService(NetService service) {
            LOG.InfoFormat("Published mDNS Service: domain({0}) type({1}) name({2})", service.Domain, service.Type, service.Name);
        }

        /// <summary>
        /// Callback when the service publishing FAILS using mDNS.
        /// </summary>
        /// <param name="service">the NetService NOT published</param>
        /// <param name="exception">the Exception reason why the service was not published</param>
        void publishService_DidNotPublishService(NetService service, DNSServiceException exception) {
            LOG.InfoFormat("DNSServiceException occured: {0}", exception.Message);
        }


    }
}
