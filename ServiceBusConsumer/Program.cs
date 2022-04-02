using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusConsumer
{
    class Program
    {
        static SubscriptionClient _serviceBusClient;
        static void Main(string[] args)
        {
            _serviceBusClient =
                new SubscriptionClient(
                    "Endpoint=sb://pocsbaceleracao.servicebus.windows.net/;SharedAccessKeyName=commonuser;SharedAccessKey=DYuCO6S1QcVDVW7tiTI5KLH+37Jk/y+SJ68IH68T+eY=",
                    "ProductAdded", 
                    "SalesProcess");

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _serviceBusClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);

            while (true)
            {

            }
        }
        private static async Task ProcessMessageAsync(Message message, CancellationToken arg2)
        {
            var product = message.Body.ParseJson<Product>();

            Console.WriteLine(product.ToString());
            Console.WriteLine(message.UserProperties["CorrelationId"]);
            Console.WriteLine(message.UserProperties["Culture"]);

            await _serviceBusClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }
    }
    public class Product
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Description: {Description}, Price: ${Price}";
        }
    }
    public static class Utils
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        /// <summary>
        /// Parses a Utf8 byte json to a specific object.
        /// </summary>
        /// <typeparam name="T">type of object to be parsed.</typeparam>
        /// <param name="json">The json bytes.</param>
        /// <returns>the object parsed from json.</returns>
        public static T ParseJson<T>(this byte[] json)
        {
            if (json == null || json.Length == 0) return default;
            var result = JsonConvert.DeserializeObject<T>(Utf8NoBom.GetString(json), JsonSettings);
            return result;
        }

    }
}
