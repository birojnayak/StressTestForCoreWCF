using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace StressTest
{
    class Program
    {
        private readonly static string _basicHttpEndPointAddress = @"https://172.31.40.50:8000/wsHttp";
        static void Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                var task1 = SingleCallPerChannelAsync(100, i, "one");
                var task2 = SingleCallPerChannelAsync(100, i, "two");
                 var task3 = SingleCallPerChannelAsync(100, i, "three");
                /*  var task4 = SingleCallPerChannelAsync(100, i, "three");
                  var task5 = SingleCallPerChannelAsync(100, i, "four");
                  var task6 = SingleCallPerChannelAsync(100, i, "five");
                  var task7 = SingleCallPerChannelAsync(100, i, "three");
                  var task8 = SingleCallPerChannelAndAbortAsync(100, i);
                  var task9 = SingleCallPerChannelAsync(100, i, "three");
                  var task10 = SingleCallPerChannelAsync(100, i, "three");*/
                 Task.WaitAll(task1, task2, task3);
                // Task.WaitAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10);
               // task1.Wait();
                Console.WriteLine("Finished " + i);
            }
            Console.WriteLine();
            Console.WriteLine("Test finished");
            Console.ReadLine();
        }

        private static async Task SingleCallPerChannelAsync(int count, int iteration, String task)
        {
                await Task.Yield();
           
            var factory = CreateChannelFactoryNetTCP<IEchoService>();

            for (int i = 0; i < 5; i++)
            {

                var channel = factory.CreateChannel();
                try
                {
                   // Console.WriteLine("OPen start : " + iteration + ", Count : " + i + " , task : " + task);
                    ((IClientChannel)channel).Open();
                   // Console.WriteLine("OPen " +  i + " , task : " + task);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error open : " + iteration + ", Count : " + i + " , task : " + task);
                }

                try
                {

                    var result = await channel.EchoStringAsync("hello");
                  //  Console.WriteLine("Echo : "  + i + " , task : " + task);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error echo : " + iteration + ", Count : " + i + " , task : " + task);

                }
                try
                {
                    ((IChannel)channel).Close();
                  //  CloseServiceModelObjects((IChannel) channel, factory);

                    Console.WriteLine("Close echo : " + i + " , task : " + task);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in CLose for iteration : " + iteration + ", Count : " + i + " , task : " + task);
                }

            }
            factory.Close();

        }

        public static void CloseServiceModelObjects(params System.ServiceModel.ICommunicationObject[] objects)
        {
            foreach (System.ServiceModel.ICommunicationObject comObj in objects)
            {
                try
                {
                    if (comObj == null)
                    {
                        continue;
                    }
                    // Only want to call Close if it is in the Opened state
                    if (comObj.State == System.ServiceModel.CommunicationState.Opened)
                    {
                        comObj.Close();
                    }
                    // Anything not closed by this point should be aborted
                    if (comObj.State != System.ServiceModel.CommunicationState.Closed)
                    {
                        comObj.Abort();
                    }
                }
                catch (TimeoutException)
                {
                    comObj.Abort();
                }
                catch (System.ServiceModel.CommunicationException)
                {
                    comObj.Abort();
                }
            }
        }



        private static async Task SingleCallPerChannelAndAbortAsync(int count, int iteration)
        {
            var factory = CreateChannelFactory<IEchoService>();
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var channel = factory.CreateChannel();
                    ((IClientChannel)channel).Open();
                    var result = await channel.EchoStringAsync("hello");
                    ((IClientChannel)channel).Abort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error for iteration : " + iteration + ", Count : " + i + " , task : ");
                }

            }
        }

        private static ChannelFactory<TChannel> CreateChannelFactory<TChannel>()
        {
            var binding = new WSHttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            binding.OpenTimeout = binding.ReceiveTimeout = binding.CloseTimeout = binding.CloseTimeout = TimeSpan.FromMinutes(30);
            EndpointIdentity upnIdentity = EndpointIdentity.CreateUpnIdentity("administrator@mscore.local");
            EndpointAddress endpoint = new EndpointAddress(new Uri(_basicHttpEndPointAddress), upnIdentity);
            var factory = new ChannelFactory<TChannel>(binding, endpoint);
            // var factory = new ChannelFactory<TChannel>(binding, "https://localhost:8443/WSHttpWcfService/basichttp.svc");
            //factory.Credentials.UserName.UserName = "testuser@corewcf";
            //factory.Credentials.UserName.Password = "abab014eba271b2accb05ce0a8ce37335cce38a30f7d39025c713c2b8037d920";
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new System.ServiceModel.Security.X509ServiceCertificateAuthentication();
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            return factory;
        }

        private static ChannelFactory<TChannel> CreateChannelFactoryNetTCP<TChannel>()
        {
            var binding = new NetTcpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.UserName;
            binding.OpenTimeout = binding.ReceiveTimeout = binding.CloseTimeout = binding.CloseTimeout = TimeSpan.FromMinutes(30);
            EndpointIdentity dnsIdentity = EndpointIdentity.CreateDnsIdentity("localhost");
            EndpointAddress endpoint = new EndpointAddress(new Uri("net.tcp://wcfserv.mscore.local:8808/nettcp"), dnsIdentity);
            var factory = new ChannelFactory<TChannel>(binding, endpoint);
            System.ServiceModel.Description.ClientCredentials clientCredentials = (System.ServiceModel.Description.ClientCredentials)factory.Endpoint.EndpointBehaviors[typeof(System.ServiceModel.Description.ClientCredentials)];
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new System.ServiceModel.Security.X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None
            };
            clientCredentials.UserName.UserName = "testuser@corewcf";
            clientCredentials.UserName.Password = "asallhjllfadjalklajlfjalk";
            return factory;
        }
    }
}
