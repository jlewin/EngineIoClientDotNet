﻿//using log4net;

using System.Diagnostics;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Xunit;
using System.IO;
using System;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{

    public class BinaryPollingTest : Connection
    {

        [Fact]
        public void PingTest()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var events = new ConcurrentQueue<object>();


            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);

            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");

                //socket.Send(binaryData);
                //socket.Send("cash money €€€");
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {

                var data = d as string;
                log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                if (data == "hi")
                {
                    return;
                }
                events.Enqueue(d);
                //socket.Close();
            });

            socket.Open();
            Task.Delay(60000).Wait();
            log.Info("ReceiveBinaryData end");

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte)(i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.Equal("1", result);
        }

        [Fact]
        public void ReceiveBinaryData()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) i;
            }


            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);


            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");
              
                //socket.Send(binaryData);
                socket.Send("cash money €€€");
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {

                var data = d as string;
                log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                if (data == "hi")
                {
                    return;
                }
                events.Enqueue(d);
                //socket.Close();
            });

            socket.Open();
            Task.Delay(1000).Wait();
            log.Info("ReceiveBinaryData end");

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.Equal(binaryData, result);
        }


        [Fact]
        public void ReceiveBinaryDataAndMultibyteUTF8String()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            Trace.WriteLine("eeeee");


            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) i;
            }
            const string stringData = "cash money €€€";

            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);


            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");
                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {

                    var data = d as string;
                    log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                    if (data == "hi")
                    {
                        return;
                    }
                    events.Enqueue(d);
                    if (events.Count > 1)
                    {
                        socket.Close();
                    }
                });
                socket.Send(binaryData);
                socket.Send(stringData);
            });

            socket.Open();

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.Equal(binaryData, result);
            events.TryDequeue(out result);
            Assert.Equal(stringData, (string) result);
            socket.Close();

        }


    }
}