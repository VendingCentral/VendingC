using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using M2Mqtt;
using M2Mqtt.Messages;
using Newtonsoft.Json;
using Windows.Storage;

namespace VendingC.BAL
{
    class Heartbeat
    {
        private static MqttClient client;
        private static ManualResetEvent manualResetEvent;
        private static string topic="BEAT/COMP_2";

        public async Task HeartBeat()
        {
            string iotEndpoint = "a18pnyx2jud7u2-ats.iot.ap-southeast-1.amazonaws.com";
            Global.log.Trace("AWS IoT Dotnet core message publisher starting");
            int brokerPort = 8883;

            //var caCert = X509Certificate.CreateFromCertFile(Convert.ToString(new Uri("ms-appx://vc_thing_2/AmazonRootCA1.pem")));
            var caCert = X509Certificate.CreateFromCertFile(Convert.ToString(new Uri("ms-appx://../BAL/Heartbeat/vc_thing_2/AmazonRootCA1.pem")));

            //            var caCert = X509Certificate.CreateFromCertFile(@"C:\\Users\\SDSA\\Documents\\Dev_data\\VendingC\\VendingC-\\VendingC-UWP\\BAL\\Iotdotnetcoreconsumer\\vc_thing_2\\AmazonRootCA1.pem");
            var clientCert = new X509Certificate2(Convert.ToString(new Uri("ms-appx://../BAL/Heartbeat/vc_thing_2/device-certificate.cert.pfx")), "MyPassword1");

            client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);


            client.MqttMsgSubscribed += IotClient_MqttMsgSubscribed;
            client.MqttMsgPublishReceived += IotClient_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Global.log.Trace($"Connected to AWS IoT with client ID: {clientId}");

            Global.log.Trace($"Subscribing to topic: {topic}");
            client.Subscribe(new string[] { topic }, new byte[] {MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            // Keep the main thread alive for the event receivers to get invoked
            KeepDebugAppRunning(() =>
            {
                client.Disconnect();
                Global.log.Trace("Disconnecting client..");
            });
        }

        private static void PublishMessage()
        {
            if (client != null)
            {
                var message = new { message = $"FROM MACHINE:{Global.MachineCode.ToString()}" };
                string jsonMessage = JsonConvert.SerializeObject(message);

                client.Publish(topic, Encoding.UTF8.GetBytes(jsonMessage));
                Global.log.Trace($"Published: HEARTBEAT");
            }
            else
            {
                Global.log.Trace($"Client is not connected yet");
            }
        }


        private static void IotClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            Global.log.Trace("Message received from server : " + msg);

            if (msg.Contains("FROM REACT:") && msg.Contains(Global.MachineCode.ToString()))
            {
                PublishMessage();
            }
            
        }

        private static void IotClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Global.log.Trace($"Successfully subscribed to the AWS IoT topic.");
        }

        private static void KeepDebugAppRunning(Action onShutdown)
        {
            manualResetEvent = new ManualResetEvent(false);
            Global.log.Trace("Press CTRL + C or CTRL + Break to exit...");

            Console.CancelKeyPress += (sender, e) =>
            {
                onShutdown();
                e.Cancel = true;
                manualResetEvent.Set();
            };

            manualResetEvent.WaitOne();
        }
    }
}