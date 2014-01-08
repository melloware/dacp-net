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
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using System.Windows.Forms;

using log4net;
using ZeroconfService;

namespace Melloware.DACP {
    /// <summary>
    /// PairingServer is used to pair with iPhone/iPod/Android DACP Clients.
    /// It uses mDNS to listen for "_touch-remote._tcp" type services and gets
    /// the Pair value out of the TXT record.  It then MD5 hashes the Pair value +
    /// the 4 Digit Passcode and returns that to the client over port 1024 using the
    /// URL with the pairingcode like:
    ///
    /// /pair?pairingcode=75D809650423A40091193AA4944D1FBD&servicename=D19BB75C3773B485
    /// </summary>
    public class DACPPairingServer {
        public const string SERVICE_TYPE = "_touch-remote._tcp";
        public const int DEFAULT_SESSION_ID = 0;

        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static NetServiceBrowser nsBrowser = new NetServiceBrowser();
        private readonly Dictionary<string,NetService> services = new Dictionary<string,NetService>();
        private DACPServer dacpServer;

        /// <summary>
        /// Constructor creates the mDNS service listener.
        /// </summary>
        public DACPPairingServer(DACPServer server) {
            LOG.Info("Initializing DACPPairingServer...");
            this.dacpServer = server;
            nsBrowser.AllowMultithreadedCallbacks = true;
            nsBrowser.DidFindService += new NetServiceBrowser.ServiceFound(NetServiceBrowser_DidFindService);
            nsBrowser.DidRemoveService += new NetServiceBrowser.ServiceRemoved(NetServiceBrowser_DidRemoveService);

            try {
                LOG.InfoFormat("mDNS Version: {0}", NetService.DaemonVersion);
                nsBrowser.SearchForService(SERVICE_TYPE, "");
            } catch (Exception ex) {
                LOG.Error("Apple Bonjour is not installed!", ex);
                throw new DACPBonjourException("Bonjour is required by this application and it was not found.  Please install Bonjour For Windowsfrom Apple.com");
            }
        }

        /// <summary>
        /// Close the PairingServer which stops the mDNS browser and clears the services.
        /// </summary>
        public void Stop() {
            LOG.Info("Shutting Down DACPPairingServer...");
            nsBrowser.Stop();
            services.Clear();
        }

        /// <summary>
        /// DACP Pairing mechanism turns out that it boils down to just concatenating them together
        /// with the pin code digits separated by null characters.
        /// </summary>
        /// <param name="pair">the pairing sequence sent from device</param>
        /// <param name="passcode">the passcode entered</param>
        /// <returns></returns>
        public static string Pair(string pair, string passcode) {
            string pairing = pair;
            foreach (char ch in passcode) {
                pairing = pairing + ch;
                pairing = pairing + '\0';
            }

            // Instantiate MD5CryptoServiceProvider need to MD5 the pairing
            Byte [] originalBytes = ASCIIEncoding.Default.GetBytes(pairing);
            Byte [] encodedBytes = new MD5CryptoServiceProvider().ComputeHash(originalBytes);

            StringBuilder result = new StringBuilder();
            for (int i=0; i<encodedBytes.Length; i++ ) {
                result.Append(encodedBytes[i].ToString("x2"));
            }
            return result.ToString().ToUpper();
        }

        /// <summary>
        /// Callback for when a service is removed from the browser
        /// </summary>
        /// <param name="browser">the NetServiceBrowser browsing for services</param>
        /// <param name="service">the Service removed</param>
        /// <param name="moreComing">true if more services are coming</param>
        private void NetServiceBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing) {
            LOG.InfoFormat("NetServiceBrowser_DidRemoveService: {0}", service.Name);
            services.Remove(service.Name);
            service.Dispose();
            if (!moreComing) {
                LOG.Debug("No More coming so signal DACP Server we are done");
                this.dacpServer.OnClientListChanged();
            }
        }


        /// <summary>
        /// Callback for when a service is detected in the browser.
        /// </summary>
        /// <param name="browser">the NetServiceBrowser browsing for services</param>
        /// <param name="service">the Service found</param>
        /// <param name="moreComing">true if more services are coming</param>
        private void NetServiceBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing) {
            LOG.InfoFormat("NetServiceBrowser_DidFindService: {0}", service.Name);

            service.DidResolveService += new NetService.ServiceResolved(NetService_DidResolveService);
            service.StartMonitoring();
            services.Add(service.Name, service);
            service.ResolveWithTimeout(5);

            if (!moreComing) {
                LOG.Debug("No More coming so signal DACP Server we are done");
            }
        }

        /// <summary>
        /// Callback when a service is successfully resolved.
        /// </summary>
        /// <param name="service">the Service that is resolved</param>
        void NetService_DidResolveService(NetService service) {
            LOG.InfoFormat("NetServiceBrowser DidResolveService: {0}", service.Name);
            LOG.DebugFormat(String.Format("Hostname: '{0}'", service.HostName));
            LOG.DebugFormat(String.Format("INET addresses found: {0}", service.Addresses.Count));
            ns_DidUpdateTXT(service);
            this.dacpServer.OnClientListChanged();
        }

        /// <summary>
        /// Performs an HTTP GET on 1024 of the client to give it back the
        /// proper pairing code.
        /// </summary>
        /// <param name="service">the NetService to call back</param>
        /// <param name="passCode">the passcode to pair with</param>
        public void PairService(NetService service, string passCode) {
            LOG.InfoFormat("Attempting to Pair Service: {0}", service.Name);
            try {
                foreach (System.Net.IPEndPoint ep in service.Addresses) {
                    LOG.InfoFormat("Pairing Service: {0}", service.ToString());
                    string pairingClient = ep.ToString();
                    if (pairingClient.StartsWith("0.0.0.0")) {
                        pairingClient = pairingClient.Replace("0.0.0.0", "localhost");
                    }
                    LOG.InfoFormat("IP: {0}", pairingClient);

                    byte[] txt = service.TXTRecordData;
                    IDictionary dict = NetService.DictionaryFromTXTRecordData(txt);
                    string pair = String.Empty;
                    if (dict.Contains("Pair")) {
                        pair = new UTF8Encoding(false).GetString((byte[])dict["Pair"]);
                    } else {
                        throw new InvalidOperationException("mDNS Service did not contain record for 'Pair'.");
                    }
                    string pairingCode = Pair(pair, passCode);
                    LOG.InfoFormat("Pairing Code = {0}", pairingCode);

                    string requestUrl = String.Format("http://{0}/pair?pairingcode={1}&servicename={2}",pairingClient, pairingCode,this.dacpServer.ServiceName);

                    byte[] data = new byte[24];
                    using (WebClient client = new WebClient()) {
                        try {
                            LOG.InfoFormat("Pairing Client Request = {0}", requestUrl);
                            data = client.DownloadData(requestUrl);
                            LOG.InfoFormat("Pairing Client Responded");
                        } catch (WebException ex) {
                            LOG.Error("No Response from Pairing!!!");
                            if (ex.Status == WebExceptionStatus.ProtocolError)
                                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                                    throw new DACPPairingException("Invalid PIN code specified");

                            throw new DACPPairingException("Unable to complete pairing", ex);
                        }
                    }

                    PairingReply reply;
                    try {
                        reply = new PairingReply(data);

                        ulong guid = reply.PairingGuid;
                        LOG.InfoFormat("Remote Device GUID After = {0}", guid);

                        if (DACPServer.PairingDatabase.DACPClients.ContainsKey(guid)) {
                            DACPServer.PairingDatabase.DACPClients.Remove(guid);
                        }
                        DACPServer.PairingDatabase.DACPClients.Add(guid,DEFAULT_SESSION_ID);

                        // now we must release the latch and allow /login to respond
                        DACPServer.ReleaseAllLatches();
                    } catch (Exception ex) {
                        LOG.Error("Error parsing Pairing Reply!");
                        throw new DACPPairingException("Unexpected reply from device", ex);
                    }
                }
            } catch (Exception ex) {
                LOG.Error("HTTP GET Error", ex);
            }
        }

        /// <summary>
        /// Callback when a Service TXT record is updated.
        /// </summary>
        /// <param name="service">the Service that has the TXT change</param>
        void ns_DidUpdateTXT(NetService service) {
            LOG.DebugFormat("Did Update TXT Record: {0}", service.Name);
            byte[] txt = service.TXTRecordData;
            IDictionary dict = NetService.DictionaryFromTXTRecordData(txt);
            LOG.DebugFormat("TXT Records Count: {0}", dict.Count);
            foreach (DictionaryEntry kvp in dict) {
                String key = (String)kvp.Key;
                byte[] value = (byte[])kvp.Value;

                // If you were creating your own service, or browsing a known service,
                // then you'd know what kind of data to expect as the value.
                // But we don't here, so we assume UTF8 strings.

                string valueStr;
                try {
                    valueStr = new UTF8Encoding(false).GetString(value);
                } catch {
                    valueStr = "[Binary Data]";
                }

                LOG.DebugFormat("TXT Record: {0} = {1}", key, valueStr);
            }

        }

        /// <summary>
        /// Property for the list of DACP Clients found by mDNS.
        /// </summary>
        public Dictionary<string, NetService> Services {
            get {
                return services;
            }
        }
    }
}
