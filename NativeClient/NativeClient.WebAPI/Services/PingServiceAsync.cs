using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services
{
    public class PingServiceAsync : IPingService
    {
        //64bytes
        readonly byte[] _buffer = Enumerable
            .Repeat(new byte[] {0xDE, 0xAD, 0xBE, 0xEF}, 8)
            .SelectMany(s => s).ToArray();

        readonly int _maxRetry = 4;
        readonly PingOptions _options = new PingOptions(64, true);
        readonly Ping _ping = new Ping();

        readonly int _timeout = 1000;

        public async Task<DataActionResult<PingTestData>> TestConnectionAsync(
            IPAddress ip
        )
        {
            List<PingTestData> pingReplies = new List<PingTestData>();
            for (int i = 0; i < _maxRetry; i++)
            {
                DataActionResult<PingTestData> pingResult = await PingAsync(
                    _ping,
                    ip,
                    _timeout,
                    _buffer,
                    _options
                );
                if (pingResult.Status.Failure()) return DataActionResult<PingTestData>.Failed(pingResult.Status);
                pingReplies.Add(pingResult.Result);
                if (pingResult.Result.Avg < 50 && pingResult.Result.Failed == 0) break;
            }

            PingTestData result = pingReplies.Aggregate((current, next) =>
            {
                current.Num += next.Num;
                current.Avg += next.Avg;
                current.Failed += next.Failed;
                return current;
            });
            int successful = result.Num - result.Failed;
            if (successful != 0) result.Avg /= successful;
            return DataActionResult<PingTestData>.Successful(result);
        }

        public async Task<DataActionResult<IEnumerable<PingTestData>>> TestConnectionListAsync(
            IEnumerable<IPAddress> ips
        )
        {
            IEnumerable<IPAddress> ipAddresses = ips as IPAddress[] ?? ips.ToArray();
            int len = ipAddresses.Count();
            var results = new DataActionResult<PingTestData>[len];
            int index = 0;
            DataActionResult<PingTestData> firstFailed = null;
            foreach (var ip in ipAddresses)
            {
                results[index] = await TestConnectionAsync(ip);
                if (results[index].Status.Failure())
                {
                    firstFailed = results[index];
                    break;
                }

                index++;
            }

            return firstFailed != null
                ? DataActionResult<IEnumerable<PingTestData>>
                    .Failed(firstFailed.Status)
                : DataActionResult<IEnumerable<PingTestData>>
                    .Successful(results.Select(r => r.Result));
        }

        static async Task<DataActionResult<PingTestData>> PingAsync(
            Ping ping,
            IPAddress ip,
            int timeout,
            byte[] buffer,
            PingOptions options
        )
        {
            try
            {
                long tripTime = -1;
                PingTestData result = new PingTestData() {Num = 1};
                var reply = await ping.SendPingAsync(ip, timeout, buffer, options);
                if (reply.Status == IPStatus.Success) tripTime = reply.RoundtripTime;
                if (tripTime == -1)
                {
                    result.Avg = 0;
                    result.Failed = 1;
                }
                else
                {
                    result.Avg = tripTime;
                }

                return DataActionResult<PingTestData>.Successful(result);
            }
            catch (PingException)
            {
                return DataActionResult<PingTestData>.Failed(
                    StatusMessage.PingExecutionServiceError
                );
            }
        }
    }
}