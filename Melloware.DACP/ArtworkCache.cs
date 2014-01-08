/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Net;

using log4net;
using Melloware.Core;

namespace Melloware.DACP {
    /// <summary>
    /// Cache to hold up to 300 artwork covers for performance and expires then after 7 days if they
    /// have not been accessed.
    /// </summary>
    public class ArtworkCache : LRUCache<ArtworkResponse> {

        // singleton and fields
        private static ArtworkCache instance = null;
        private IIndex<int> _findByID = null;

        /// <summary>Singleton pattern forces everyone to share the cache</summary>
        public static ArtworkCache Instance {
            get {
                if( instance == null )
                    lock( typeof( ArtworkCache ) )
                        if( instance == null )
                            instance = new ArtworkCache();
                return instance;
            }
        }

        /// <summary>retrieve items by id</summary>
        public ArtworkResponse FindByID( int id ) {
        	// return null if this is the NowPlaying album so it will grab a fresh one
            if (id <= ArtworkResponse.NOW_PLAYING) {
                return null;
            } else {
                return _findByID[id];
            }
        }

        /// <summary>constructor creates cache and multiple indexes</summary>
        private ArtworkCache() : base( 300, TimeSpan.FromHours( 1 ), TimeSpan.FromDays( 7 ), null ) {
            _findByID = AddIndex<int>( "ID", response => response.ItemId, null );
        }
    }
}
