/*
 * Copyright 2008, Yahoo! Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#include <cppunit/extensions/HelperMacros.h>

#include <zookeeper.h>
#include "src/zk_adaptor.h"

#include "LibCMocks.h"
#include "ZKMocks.h"
#include "Util.h"

#ifdef THREADED
#include "PthreadMocks.h"
#endif

using namespace std;

class Zookeeper_close : public CPPUNIT_NS::TestFixture
{
    CPPUNIT_TEST_SUITE(Zookeeper_close);
    CPPUNIT_TEST(testCloseUnconnected);
    CPPUNIT_TEST(testCloseUnconnected1);
    CPPUNIT_TEST(testCloseConnected1);
    CPPUNIT_TEST(testCloseFromWatcher1);
    CPPUNIT_TEST_SUITE_END();
    zhandle_t *zh;
    static void watcher(zhandle_t *, int, int, const char *){}
public: 
    void setUp()
    {
        zoo_set_debug_level((ZooLogLevel)0); // disable logging
        zoo_deterministic_conn_order(0);
        zh=0;
    }
    
    void tearDown()
    {
        zookeeper_close(zh);
    }

#ifndef THREADED
    void testCloseUnconnected()
    {       
        zh=zookeeper_init("localhost:2121",watcher,10000,0,0,0);       
        CPPUNIT_ASSERT(zh!=0);
        
        // do not actually free the memory while in zookeeper_close()
        Mock_free_noop freeMock;
        // make a copy of zhandle before close() overwrites some of 
        // it members with NULLs
        zhandle_t lzh;
        memcpy(&lzh,zh,sizeof(lzh));
        int rc=zookeeper_close(zh);
        zhandle_t* savezh=zh; zh=0;
        freeMock.disable(); // disable mock's fake free()- use libc's free() instead
        
        // verify that zookeeper_close has done its job
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);
        // memory
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(savezh));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.hostname));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.addrs));
        CPPUNIT_ASSERT_EQUAL(3,freeMock.callCounter);
    }
    void testCloseUnconnected1()
    {
        zh=zookeeper_init("localhost:2121",watcher,10000,0,0,0);       
        CPPUNIT_ASSERT(zh!=0);
        // simulate connected state 
        zh->fd=100;
        zh->state=CONNECTED_STATE;
        Mock_flush_send_queue zkMock;
        // do not actually free the memory while in zookeeper_close()
        Mock_free_noop freeMock;
        // make a copy of zhandle before close() overwrites some of 
        // it members with NULLs
        zhandle_t lzh;
        memcpy(&lzh,zh,sizeof(lzh));
        int rc=zookeeper_close(zh);
        zhandle_t* savezh=zh; zh=0;
        freeMock.disable(); // disable mock's fake free()- use libc's free() instead

        // verify that zookeeper_close has done its job
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);
        // memory
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(savezh));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.hostname));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.addrs));
        // the close request sent?
        CPPUNIT_ASSERT_EQUAL(1,zkMock.counter);
    }
    void testCloseConnected1()
    {
        zh=zookeeper_init("localhost:2121",watcher,10000,0,0,0);       
        CPPUNIT_ASSERT(zh!=0);

        Mock_socket sockMock;
        sockMock.socketReturns=ZookeeperServer::FD;
        sockMock.connectReturns=-1;
        sockMock.connectErrno=EWOULDBLOCK;
        
        Mock_gettimeofday timeMock;
        
        int fd=0;
        int interest=0;
        timeval tv;
        int rc=zookeeper_interest(zh,&fd,&interest,&tv);
        
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);        
        CPPUNIT_ASSERT_EQUAL(CONNECTING_STATE,zoo_state(zh));
        CPPUNIT_ASSERT_EQUAL(ZOOKEEPER_READ|ZOOKEEPER_WRITE,interest);
        
        rc=zookeeper_process(zh,interest);
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);        
        CPPUNIT_ASSERT_EQUAL(ASSOCIATING_STATE,zoo_state(zh));
        
        timeMock.tick();
        rc=zookeeper_interest(zh,&fd,&interest,&tv);
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);
        // prepare the handshake response
        sockMock.recvReturnBuffer=HandshakeResponse().toString();
        rc=zookeeper_process(zh,interest);
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);        
        CPPUNIT_ASSERT_EQUAL(CONNECTED_STATE,zoo_state(zh));
    }
    void testCloseFromWatcher1()
    {
    }
#else
    void testCloseUnconnected()
    {
        // disable threading
        MockPthreadZKNull pthreadMock;
        zh=zookeeper_init("localhost:2121",watcher,10000,0,0,0); 
        
        CPPUNIT_ASSERT(zh!=0);
        adaptor_threads* adaptor=(adaptor_threads*)zh->adaptor_priv;
        CPPUNIT_ASSERT(adaptor!=0);

        // do not actually free the memory while in zookeeper_close()
        Mock_free_noop freeMock;
        // make a copy of zhandle before close() overwrites some of 
        // it members with NULLs
        zhandle_t lzh;
        memcpy(&lzh,zh,sizeof(lzh));
        int rc=zookeeper_close(zh);
        zhandle_t* savezh=zh; zh=0;
        // we're done, disable mock's fake free(), use libc's free() instead
        freeMock.disable();
        
        // verify that zookeeper_close has done its job
        CPPUNIT_ASSERT_EQUAL(ZOK,rc);
        // memory
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(savezh));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.hostname));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.addrs));
        CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(adaptor));
        CPPUNIT_ASSERT_EQUAL(4,freeMock.callCounter);
        // threads
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(adaptor->io));
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(adaptor->completion));
        // mutexes
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->to_process.lock));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->to_process.lock));
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->to_send.lock));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->to_send.lock));
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->sent_requests.lock));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->sent_requests.lock));
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->completions_to_process.lock));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->completions_to_process.lock));
        // conditionals
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->sent_requests.cond));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->sent_requests.cond));
        CPPUNIT_ASSERT_EQUAL(1,MockPthreadsNull::getDestroyCounter(&savezh->completions_to_process.cond));
        CPPUNIT_ASSERT_EQUAL(0,MockPthreadsNull::getInvalidAccessCounter(&savezh->completions_to_process.cond));
    }
    void testCloseUnconnected1()
    {
        //zoo_set_debug_level(LOG_LEVEL_DEBUG);
        for(int i=0; i<100;i++){
            zh=zookeeper_init("localhost:2121",watcher,10000,0,0,0); 
            CPPUNIT_ASSERT(zh!=0);
            adaptor_threads* adaptor=(adaptor_threads*)zh->adaptor_priv;
            CPPUNIT_ASSERT(adaptor!=0);
            int rc=zookeeper_close(zh);
            zh=0;
            CPPUNIT_ASSERT_EQUAL(ZOK,rc);
        }
    }
    void testCloseConnected1()
    {
        // frozen time -- no timeouts and no pings
        Mock_gettimeofday timeMock;

        for(int i=0;i<500;i++){
            ZookeeperServer zkServer;
            Mock_poll pollMock(&zkServer,ZookeeperServer::FD);
            // use a checked version of pthread calls
            CheckedPthread threadMock;
            // do not actually free the memory while in zookeeper_close()
            Mock_free_noop freeMock;
            
            zh=zookeeper_init("localhost:2121",watcher,10000,&testClientId,0,0); 
            CPPUNIT_ASSERT(zh!=0);
            // make sure the client has connected
            while(zh->state!=CONNECTED_STATE)
                millisleep(2);
            // make a copy of zhandle before close() overwrites some of 
            // its members with NULLs
            zhandle_t lzh;
            memcpy(&lzh,zh,sizeof(lzh));
            int rc=zookeeper_close(zh);
            zhandle_t* savezh=zh; zh=0;
            // we're done, disable mock's fake free(), use libc's free() instead
            freeMock.disable();
            
            CPPUNIT_ASSERT_EQUAL(ZOK,rc);            
            adaptor_threads* adaptor=(adaptor_threads*)lzh.adaptor_priv;
            // memory
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(savezh));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.hostname));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh.addrs));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(adaptor));
            // threads
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(adaptor->io));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(adaptor->completion));
            // mutexes
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->to_process.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->to_process.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->to_send.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->to_send.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->sent_requests.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->sent_requests.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->completions_to_process.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->completions_to_process.lock));
            // conditionals
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->sent_requests.cond));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->sent_requests.cond));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&savezh->completions_to_process.cond));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&savezh->completions_to_process.cond));
        }
    }
    
    class CloseOnSessionExpired: public WatcherAction{
    public:
        CloseOnSessionExpired(Zookeeper_close& test):test_(test),rc(ZOK){}
        virtual void onSessionExpired(zhandle_t* zh){
            memcpy(&lzh,zh,sizeof(lzh));
            rc=zookeeper_close(zh);           
        }
        Zookeeper_close& test_;
        zhandle_t lzh;
        int rc;
    };

    void testCloseFromWatcher1()
    {
        // frozen time -- no timeouts and no pings
        Mock_gettimeofday timeMock;
        
        for(int i=0;i<100;i++){
            ZookeeperServer zkServer;
            // make the server return a non-matching session id
            zkServer.returnSessionExpired();
            
            Mock_poll pollMock(&zkServer,ZookeeperServer::FD);
            // use a checked version of pthread calls
            CheckedPthread threadMock;
            // do not actually free the memory while in zookeeper_close()
            Mock_free_noop freeMock;

            CloseOnSessionExpired closeAction(*this);
            zh=zookeeper_init("localhost:2121",activeWatcher,10000,
                    &testClientId,&closeAction,0);
            
            CPPUNIT_ASSERT(zh!=0);
            // we rely on the fact that zh is freed the last right before
            // zookeeper_close() returns...
            while(!freeMock.isFreed(zh))
                millisleep(2);
            zhandle_t* lzh=zh;
            zh=0;
            // we're done, disable mock's fake free(), use libc's free() instead
            freeMock.disable();
            
            CPPUNIT_ASSERT_EQUAL(ZOK,closeAction.rc);          
            adaptor_threads* adaptor=(adaptor_threads*)closeAction.lzh.adaptor_priv;
            // memory
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(lzh));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(closeAction.lzh.hostname));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(closeAction.lzh.addrs));
            CPPUNIT_ASSERT_EQUAL(1,freeMock.getFreeCount(adaptor));
            // threads
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(adaptor->io));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(adaptor->completion));
            // mutexes
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->to_process.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->to_process.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->to_send.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->to_send.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->sent_requests.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->sent_requests.lock));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->completions_to_process.lock));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->completions_to_process.lock));
            // conditionals
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->sent_requests.cond));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->sent_requests.cond));
            CPPUNIT_ASSERT_EQUAL(1,CheckedPthread::getDestroyCounter(&lzh->completions_to_process.cond));
            CPPUNIT_ASSERT_EQUAL(0,CheckedPthread::getInvalidAccessCounter(&lzh->completions_to_process.cond));
        }
    }
#endif
};

CPPUNIT_TEST_SUITE_REGISTRATION(Zookeeper_close);