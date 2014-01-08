/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Threading;
using log4net;

namespace Melloware.Core {
    /// <summary>
    /// A synchronization aid that allows one or more threads to wait until
    /// a set of operations being performed in other threads completes.
    ///
    /// <p>A {@code CountDownLatch} is initialized with a given <em>count</em>.
    /// The {@link #await await} methods block until the current count reaches
    /// zero due to invocations of the {@link #countDown} method, after which
    /// all waiting threads are released and any subsequent invocations of
    /// {@link #await await} return immediately.  This is a one-shot phenomenon
    ///  -- the count cannot be reset.  If you need a version that resets the
    ///  count, consider using a {@link CyclicBarrier}.
    ///
    ///  <p>A {@code CountDownLatch} is a versatile synchronization tool
    ///  and can be used for a number of purposes.  A
    ///  {@code CountDownLatch} initialized with a count of one serves as a
    ///  simple on/off latch, or gate: all threads invoking {@link #await await}
    ///  wait at the gate until it is opened by a thread invoking {@link
    ///  #countDown}.  A {@code CountDownLatch} initialized to <em>N</em>
    ///  can be used to make one thread wait until <em>N</em> threads have
    ///  completed some action, or some action has been completed N times.
    ///
    ///  <p>A useful property of a {@code CountDownLatch} is that it
    ///  doesn't require that threads calling {@code countDown} wait for
    ///  the count to reach zero before proceeding, it simply prevents any
    ///  thread from proceeding past an {@link #await await} until all
    ///  threads could pass.
    ///
    /// </summary>
    public class CountDownLatch {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ManualResetEvent mutex = new ManualResetEvent(false);
        private int remaining;

        public CountDownLatch(int i) {
            remaining = i;
        }

        public void CountDown() {
            lock(mutex) {
                if(remaining > 0) {
                    remaining--;
                    if(0 == remaining) {
                        LOG.Debug("Latch Released!");
                        mutex.Set();
                    }
                }
            }
        }

        public int Remaining {
            get {
                lock(mutex) {
                    return remaining;
                }
            }
        }

        public bool Await(TimeSpan timeout) {
            return mutex.WaitOne((int) timeout.TotalMilliseconds, false);
        }

        public bool Await(int timeoutInMilliseconds) {
            return mutex.WaitOne(timeoutInMilliseconds, false);
        }

        public WaitHandle AsyncWaitHandle {
            get {
                return mutex;
            }
        }
    }
}
