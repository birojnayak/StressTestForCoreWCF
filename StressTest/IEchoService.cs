using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace StressTest
{
    internal static class Constants
    {
        public const string NS = "http://tempuri.org/";
        public const string ECHOSERVICE_NAME = nameof(IEchoService);
        public const string OPERATION_BASE = NS + ECHOSERVICE_NAME + "/";
    }

    [ServiceContract(Namespace = Constants.NS, Name = Constants.ECHOSERVICE_NAME)]
    public interface IEchoService
    {
        [OperationContract(Name = "EchoString", Action = Constants.OPERATION_BASE + "EchoString",
            ReplyAction = Constants.OPERATION_BASE + "EchoStringResponse")]
        Task<string> EchoStringAsync(string echo);

        [OperationContract(Name = "EchoStream", Action = Constants.OPERATION_BASE + "EchoStream",
            ReplyAction = Constants.OPERATION_BASE + "EchoStreamResponse")]
        Task<Stream> EchoStreamAsync(Stream echo);

        [OperationContract(Name = "EchoToFail", Action = Constants.OPERATION_BASE + "EchoToFail",
            ReplyAction = Constants.OPERATION_BASE + "EchoToFailResponse")]
        string EchoToFail(string echo);
    }
}
