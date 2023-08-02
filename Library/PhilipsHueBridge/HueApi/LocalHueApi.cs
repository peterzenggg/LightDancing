using System.Text;
using HueApi.Models.Clip;
using HueApi.Models.Exceptions;
using HueApi.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HueApi
{
    public class LocalHueApi: BaseHueApi
    {
        protected const string KeyHeaderName = "hue-application-key";

        public event EventStreamMessage? OnEventStreamMessage;
        private CancellationTokenSource? eventStreamCancellationTokenSource;

        protected const string EventStreamUrl = "eventstream/clip/v2";

        private string ip;
        private string? key;

        public LocalHueApi(string ip, string? key, HttpClient? client = null)
        {
            this.ip = ip;
            this.key = key;

            client = GetConfiguredHttpClient(client);
        }



        public async void StartEventStream(HttpClient? client = null, CancellationToken? cancellationToken = null)
        {
            this.eventStreamCancellationTokenSource?.Cancel();

            if (cancellationToken.HasValue)
                this.eventStreamCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value);
            else
                this.eventStreamCancellationTokenSource = new CancellationTokenSource();

            var cancelToken = this.eventStreamCancellationTokenSource.Token;

            using (HttpClient infiniteHttpClient = GetConfiguredHttpClient(client, Timeout.InfiniteTimeSpan))
            {

                while (!cancelToken.IsCancellationRequested) //Auto retry on stop
                {
                    try
                    {
#if NET461
          using (var streamReader = new StreamReader(await infiniteHttpClient.GetStreamAsync(EventStreamUrl)))
#else
                        using (var streamReader = new StreamReader(await infiniteHttpClient.GetStreamAsync(EventStreamUrl, cancelToken)))
#endif
                        {
                            while (!streamReader.EndOfStream)
                            {
                                var jsonMsg = await streamReader.ReadLineAsync();
                                //Console.WriteLine($"Received message: {message}");

                                if (jsonMsg != null)
                                {
                                    var data = JsonConvert.DeserializeObject<List<EventStreamResponse>>(jsonMsg);

                                    if (data != null && data.Any())
                                    {
                                        OnEventStreamMessage?.Invoke(data);
                                    }
                                }
                            }
                        }
                    }
                    catch (TaskCanceledException ex)
                    {
                        //Ignore
                    }
                }
            }

        }

        public void StopEventStream()
        {
            this.eventStreamCancellationTokenSource?.Cancel();
        }



        /// <summary>
        /// Register your <paramref name="applicationName"/> and <paramref name="deviceName"/> at the Hue Bridge.
        /// </summary>
        /// <param name="ip">ip address of bridge</param>
        /// <param name="applicationName">The name of your app.</param>
        /// <param name="deviceName">The name of the device.</param>
        /// <param name="generateClientKey">Set to true if you want a client key to use the streaming api</param>
        /// <returns>Secret key for the app to communicate with the bridge.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="applicationName"/> or <paramref name="deviceName"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="applicationName"/> or <paramref name="deviceName"/> aren't long enough, are empty or contains spaces.</exception>
        public static async Task<RegisterEntertainmentResult?> RegisterAsync(string ip, string applicationName, string deviceName, bool generateClientKey = false, CancellationToken? cancellationToken = null)
        {
            if (!cancellationToken.HasValue)
                cancellationToken = new CancellationTokenSource().Token;

            if (applicationName == null)
                throw new ArgumentNullException(nameof(applicationName));
            if (applicationName.Trim() == String.Empty)
                throw new ArgumentException("applicationName must not be empty", nameof(applicationName));
            if (applicationName.Length > 20)
                throw new ArgumentException("applicationName max is 20 characters.", nameof(applicationName));
            if (applicationName.Contains(" "))
                throw new ArgumentException("Cannot contain spaces.", nameof(applicationName));

            if (deviceName == null)
                throw new ArgumentNullException(nameof(deviceName));
            if (deviceName.Length < 0 || deviceName.Trim() == String.Empty)
                throw new ArgumentException("deviceName must be at least 0 characters.", nameof(deviceName));
            if (deviceName.Length > 19)
                throw new ArgumentException("deviceName max is 19 characters.", nameof(deviceName));
            if (deviceName.Contains(" "))
                throw new ArgumentException("Cannot contain spaces.", nameof(deviceName));

            string fullName = string.Format("{0}#{1}", applicationName, deviceName);


            Dictionary<string, object> obj = new()
            {
                ["devicetype"] = fullName
            };
            if (generateClientKey)
                obj["generateclientkey"] = true;

            HttpClient client = new HttpClient();
            var response = await client.PostAsync(new Uri($"http://{ip}/api"), new StringContent(obj?.ToString() ?? "", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            var stringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            JObject? result = null;
            try
            {
                var arrayResult = JArray.Parse(stringResponse);
                result = arrayResult?.FirstOrDefault() as JObject;
            }
            catch
            {
                //Not an expected response. Return response as exception
                throw new HueException(stringResponse);
            }

            if (result != null)
            {
                JToken? error;
                if (result.TryGetValue("error", out error))
                {
                    if (error?["type"]?.Value<int>() == 101) // link button not pressed
                        throw new LinkButtonNotPressedException("Link button not pressed");
                    else
                        throw new HueException(error?["description"]?.Value<string>());
                }

                var username = result?["success"]?["username"]?.Value<string>();
                var streamingClientKey = result?["success"]?["clientkey"]?.Value<string>();

                if (username != null)
                {
                    return new RegisterEntertainmentResult()
                    {
                        Ip = ip,
                        Username = username,
                        StreamingClientKey = streamingClientKey
                    };
                }
            }

            return null;
        }

        private HttpClient GetConfiguredHttpClient(HttpClient? client = null, TimeSpan? timeout = null)
        {
            if (client == null)
            {
                var handler = new HttpClientHandler()
                {
#if NET461
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
#else
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
                };

                client = new HttpClient(handler);
            }

            if (timeout.HasValue)
                client.Timeout = timeout.Value;

            client.BaseAddress = new Uri($"https://{ip}/");

            if (!string.IsNullOrEmpty(key))
                client.DefaultRequestHeaders.Add(KeyHeaderName, key);

            this.client = client;
            return client;
        }


    }
}
