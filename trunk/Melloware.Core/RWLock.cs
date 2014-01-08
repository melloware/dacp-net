/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Melloware.Core {

    public enum RWLockStatus {
        UNLOCKED,
        READ_LOCK,
        WRITE_LOCK
    }

    public class RWLock : IDisposable {
        public delegate ResultType DoWorkFunc<ResultType>();

        public static int defaultTimeout = 30000;
        private RWLockStatus status = RWLockStatus.UNLOCKED;
        private ReaderWriterLock lockObj;
        private int timeout;
        private LockCookie cookie;
        private bool upgraded = false;

        #region delegate based methods
        public static ResultType GetWriteLock<ResultType>(ReaderWriterLock lockObj, int timeout, DoWorkFunc<ResultType> doWork) {
            RWLockStatus status = (lockObj.IsWriterLockHeld ? RWLockStatus.WRITE_LOCK : (lockObj.IsReaderLockHeld ? RWLockStatus.READ_LOCK : RWLockStatus.UNLOCKED));
            LockCookie writeLock = default(LockCookie);
            if( status == RWLockStatus.READ_LOCK )
                writeLock = lockObj.UpgradeToWriterLock(timeout);
            else if( status == RWLockStatus.UNLOCKED )
                lockObj.AcquireWriterLock(timeout);
            try {
                return doWork();
            } finally {
                if( status == RWLockStatus.READ_LOCK )
                    lockObj.DowngradeFromWriterLock(ref writeLock);
                else if( status == RWLockStatus.UNLOCKED )
                    lockObj.ReleaseWriterLock();
            }
        }

        public static ResultType GetReadLock<ResultType>(ReaderWriterLock lockObj, int timeout, DoWorkFunc<ResultType> doWork) {
            bool releaseLock = false;
            if( !lockObj.IsWriterLockHeld && !lockObj.IsReaderLockHeld ) {
                lockObj.AcquireReaderLock(timeout);
                releaseLock = true;
            }
            try {
                return doWork();
            } finally {
                if( releaseLock )
                    lockObj.ReleaseReaderLock();
            }
        }
        #endregion

        #region disposable based methods
        public static RWLock GetReadLock(ReaderWriterLock lockObj) {
            return new RWLock(lockObj, RWLockStatus.READ_LOCK, defaultTimeout);
        }

        public static RWLock GetWriteLock(ReaderWriterLock lockObj) {
            return new RWLock(lockObj, RWLockStatus.WRITE_LOCK, defaultTimeout);
        }

        public RWLock(ReaderWriterLock lockObj, RWLockStatus status, int timeoutMS) {
            this.lockObj = lockObj;
            this.timeout = timeoutMS;
            this.Status = status;
        }

        public void Dispose() {
            Status = RWLockStatus.UNLOCKED;
        }

        public RWLockStatus Status {
            get {
                return status;
            } set {
                if( status != value ) {
                    if( status == RWLockStatus.UNLOCKED ) {
                        upgraded = false;
                        if( value == RWLockStatus.READ_LOCK )
                            lockObj.AcquireReaderLock( timeout );
                        else if( value == RWLockStatus.WRITE_LOCK )
                            lockObj.AcquireWriterLock( timeout );
                    } else if( value == RWLockStatus.UNLOCKED )
                        lockObj.ReleaseLock();
                    else if( value == RWLockStatus.WRITE_LOCK ) { // && status==RWLockStatus.READ_LOCK
                        cookie = lockObj.UpgradeToWriterLock( timeout );
                        upgraded = true;
                    } else if( upgraded ) { // value==RWLockStatus.READ_LOCK && status==RWLockStatus.WRITE_LOCK
                        lockObj.DowngradeFromWriterLock( ref cookie );
                        upgraded = false;
                    } else {
                        lockObj.ReleaseLock();
                        status = RWLockStatus.UNLOCKED;
                        lockObj.AcquireReaderLock( timeout );
                    }
                    status = value;
                }
            }
        }
        #endregion

    }
}
