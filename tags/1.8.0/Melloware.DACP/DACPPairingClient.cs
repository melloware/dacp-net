/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

using log4net;
using ZeroconfService;

namespace Melloware.DACP {
    /// <summary>
    /// Pairing Client used to mock a successful client login.
    /// It listens on Port 1024 for HTTP requests and publishes mDNS service
    /// for "_touch-remote._tcp".
    /// </summary>
    public class DACPPairingClient {
        public const string SERVICE_TYPE = "_touch-remote._tcp";
        public const int PORT = 1024;

        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static HttpListener listener = new HttpListener();
        private static System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);
        private string applicationName;
        private NetService touchRemoteService = null;


        /// <summary>
        /// Constructor to start listening on port and broadcast mDNS.
        /// </summary>
        public DACPPairingClient(string applicationName) {
            this.applicationName = applicationName;
            PublishTouchRemoteService();
        }

        /// <summary>
        /// Close the listener and stop the mDNS service.
        /// </summary>
        public void Stop() {
            if (listener != null) {
                listener.Stop();
            }
            if (touchRemoteService != null) {
                touchRemoteService.Stop();
            }
        }

        /// <summary>
        /// Creates a NonBlocking listener to handle each HTTP request in a thread.
        /// </summary>
        public void Start() {
            LOG.InfoFormat("Starting Listener http://*:{0}/...", PORT);
            listener.Prefixes.Add("http://*:"+PORT+"/");
            listener.Start();

            // Move our listening loop off to a worker thread so that the GUI doesn't lock up.
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Listen));
        }

        /// <summary>
        /// Loop here to begin processing of new requests.
        /// </summary>
        /// <param name="state">Thread object state</param>
        private void Listen(object state) {
            while (listener.IsListening) {
                listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                listenForNextRequest.WaitOne();
            }
        }

        /// <summary>
        /// Callback when a HTTP request comes in on the port listener and is handed off
        /// to a thread for processing.  This method
        /// </summary>
        /// <param name="result">IAsyncResult containing the HTTPListener</param>
        public void ListenerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener) result.AsyncState;
            HttpListenerContext context = null;
            if (listener == null) return;

            try {
                // The EndGetContext() method, as with all Begin/End asynchronous methods in the .NET Framework,
                // blocks until there is a request to be processed or some type of data is available.
                context = listener.EndGetContext(result);
            } catch (Exception ex) {
                // You will get an exception when httpListener.Stop() is called
                // because there will be a thread stopped waiting on the .EndGetContext()
                // method, and again, that is just the way most Begin/End asynchronous
                // methods of the .NET Framework work.
                LOG.Warn("HttpListener Error", ex);
                return;
            } finally {
                // Once we know we have a request (or exception), we signal the other thread
                // so that it calls the BeginGetContext() (or possibly exits if we're not
                // listening any more) method to start handling the next incoming request
                // while we continue to process this request on a different thread.
                listenForNextRequest.Set();
            }

            if (context == null) return;

            LOG.DebugFormat("Start: {0}", DateTime.Now.ToString());


            System.Net.HttpListenerRequest request = context.Request;
            LOG.DebugFormat("{0}: {1}", PORT, request.RawUrl);

            if (request.HasEntityBody) {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(request.InputStream, request.ContentEncoding)) {
                    string requestData = sr.ReadToEnd();
                }
            }

            // Obtain a response object
            using (System.Net.HttpListenerResponse response = context.Response) {
                /*
                cmpa --+
                    cmpg 4  000000c8 == 0000000000000001
                    cmnm 10 648a861f == devicename
                    cmty 4  648a861f == ipod
                */

                DACPResponse dacpResponse = new PairingClientResponse();
                byte[] output = dacpResponse.GetBytes();

                LOG.Debug(new UTF8Encoding(false).GetString(output));
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = output.LongLength;
                response.OutputStream.Write(output, 0, output.Length);
            }

            LOG.DebugFormat("End: {0}", DateTime.Now.ToString());
        }

        /// <summary>
        /// Publishes the mDNS sevrice to pretend like we are a client device
        /// like an iPod Touch.  Service is "_touch-remote._tcp" and Pair is
        /// the important value here.
        /// </summary>
        private void PublishTouchRemoteService() {
            try {
                LOG.DebugFormat("mDNS: {0}", NetService.DaemonVersion);
                String domain = "";
                String name = "0000000000000000000000000000000000000001";
                touchRemoteService = new NetService(domain, SERVICE_TYPE, name, PORT);
                touchRemoteService.AllowMultithreadedCallbacks = true;
                touchRemoteService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
                touchRemoteService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);

                /* HARDCODE TXT RECORD */
                System.Collections.Hashtable dict = new System.Collections.Hashtable();
                dict.Add("DvNm", this.applicationName + " Client");
                dict.Add("RemV", "10000");
                dict.Add("DvTy", "iPod");
                dict.Add("RemN", "Remote");
                dict.Add("txtvers", "1");
                dict.Add("Pair", "0000000000000001");
                touchRemoteService.TXTRecordData = NetService.DataFromTXTRecordDictionary(dict);
                touchRemoteService.Publish();
            } catch (Exception ex) {
                LOG.Error("Error publishing mDNS TouchRemote Service", ex);
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
