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
    /// BaseNode is the abstract base type from which all Nodes should extend.
    /// </summary>
    public abstract class BaseNode:DACPResponse {
        // logger
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Item Id (unique)
        /// </summary>
        public int Miid {
            get;    // #item id
            set;
        }
        
        /// <summary>
        /// Item Name
        /// </summary>
        public string Minm {
            get;    // #name
            set;
        }
        
        /// <summary>
        /// Persistent Id or Database Id (unique)
        /// </summary>
        public ulong Mper {
            get;    // #persistent id
            set;
        }
        
        /// <summary>
        /// Item count such as track count
        /// </summary>
        public int Mimc {
            get;    // #track count
            set;
        }

    }
}
