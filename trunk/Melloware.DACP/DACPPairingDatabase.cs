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
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using log4net;
using Melloware.Core;
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace Melloware.DACP {
    /// <summary>
    /// Description of DACPPairingDatabase.
    /// </summary>
    public class DACPPairingDatabase {
        [System.Xml.Serialization.XmlIgnoreAttribute]
        public DACPServer Server;

        // logger
        private static readonly Type CLAZZ_TYPE = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
        private static readonly ILog LOG = LogManager.GetLogger(CLAZZ_TYPE);

        // serializable fields
        public bool ShowParentPlaylists = true;
        public bool RespectClearCueCommand = true;
        public SerializableDictionary<ulong, int> DACPClients = new SerializableDictionary<ulong,int>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public DACPPairingDatabase() {
            LOG.Info("Creating DACPPairingDatabase....");
        }

        /// <summary>
        /// Static method to create a DACPPairingDatabase and try and load XML
        /// file from disk located at EXENAME.xml in the same directory.
        /// </summary>
        /// <returns>a DACPPairingDatabase object</returns>
        public static DACPPairingDatabase Initialize(DACPServer dacpServer) {
            LOG.Info("Initializing DACPPairingDatabase");
            DACPPairingDatabase database = new DACPPairingDatabase();
            database.Server = dacpServer;

            string fileName = database.GetFileName();
            if (File.Exists(fileName)) {
                LOG.InfoFormat("Deserializing XML file: {0}", fileName);

                try {
                	DACPPairingDatabaseSerializer serializer = new DACPPairingDatabaseSerializer();
                    //XmlSerializer serializer = new XmlSerializer(CLAZZ_TYPE);
                    using (FileStream stream = new FileStream(fileName, FileMode.Open)) {
                        database = (DACPPairingDatabase)serializer.Deserialize(stream);
                        database.Server = dacpServer;
                    }
                } catch (Exception ex) {
                    LOG.Error("XML Exception", ex);
                    database = new DACPPairingDatabase();
                    database.Server = dacpServer;
                }
            }

            return database;
        }

        /// <summary>
        /// Saves the current database to disk.
        /// </summary>
        public void Store() {
            try {
                string fileName = this.GetFileName();
                LOG.InfoFormat("Serializing XML file: {0}", fileName);

                DACPPairingDatabaseSerializer serializer = new DACPPairingDatabaseSerializer();
                // XmlSerializer serializer = new XmlSerializer(CLAZZ_TYPE);
                using (StreamWriter stream = new StreamWriter(fileName)) {
                    serializer.Serialize(stream, this);
                }
            } catch (Exception ex) {
                LOG.Error("XML Exception", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Attempts to validate the GUID is in the database and returns
        /// it's associated SesssionId if it is or throws an exception if it
        /// is not.
        /// </summary>
        /// <param name="guid">the GUID to validate</param>
        /// <returns>an int SessionId of the session for the GUID</returns>
        public int ValidateGuid(ulong guid) {
            LOG.DebugFormat("Validating GUID = {0}", guid);

            if (!this.DACPClients.ContainsKey(guid)) {
                // cheat here if GUID is 1 for Android pairing
                if (guid == 1) {
                    this.DACPClients.Add(guid, DACPPairingServer.DEFAULT_SESSION_ID);
                } else {
                    throw new DACPSecurityException("GUID not valid");
                }
            }

            int sessionId = -1;
            this.DACPClients.TryGetValue(guid, out sessionId);

            // if none found then generate a random session id
            if (sessionId <= DACPPairingServer.DEFAULT_SESSION_ID) {
                Random random = new Random();
                sessionId = random.Next();

                // need to remove then add to the dictionary
                this.DACPClients.Remove(guid);
                this.DACPClients.Add(guid, sessionId);
            }
            return sessionId;
        }

        /// <summary>
        /// Validates a session Id has been established in the pairing
        /// database already.
        /// </summary>
        /// <param name="sessionId">the sessionId to validate</param>
        /// <returns>true if valid, false if not</returns>
        public bool ValidateSession(int sessionId) {
            return this.DACPClients.ContainsValue(sessionId);
        }

        /// <summary>
        /// Gets the C:\Users\USER\AppData\Application path.
        /// </summary>
        /// <returns>the string path to Application Data</returns>
        private string GetUserDataPath() {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir = System.IO.Path.Combine(dir, this.Server.GetApplicationName());
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>
        /// Gets the filename for this pairing database file based on app name.
        ///
        /// Ex: C:\Users\USER\AppData\Roaming\DACP\dacp.xml
        /// </summary>
        /// <returns>a file name to the database path</returns>
        private string GetFileName() {
            return GetUserDataPath() + "\\" +this.Server.GetApplicationName()+ ".xml";
        }


    }
}
