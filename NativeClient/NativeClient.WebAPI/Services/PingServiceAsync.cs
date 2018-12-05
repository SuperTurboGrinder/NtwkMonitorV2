using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services {

public class PingServiceAsync: IPingService {
    readonly Ping ping = new Ping();
    //64bytes
    readonly byte[] buffer = Enumerable
        .Repeat(new byte[]{ 0xDE, 0xAD, 0xBE, 0xEF }, 8)
        .SelectMany(s=>s).ToArray();
        
    readonly int timeout = 1000;
    readonly int maxRetry = 4;
    readonly PingOptions options = new PingOptions(64, true);
    
    static async Task<DataActionResult<PingTestData>> PingAsync(
        Ping ping,
        IPAddress ip,
        int timeout,
        byte[] buffer,
        PingOptions options
    ) {
        try {
            long triptime = -1;
            PingTestData result = new PingTestData() { num = 1 };
            var reply = await ping.SendPingAsync(ip, timeout, buffer, options);
            if(reply.Status == IPStatus.Success) {
                triptime = reply.RoundtripTime;
            }
            if(triptime == -1) {
                result.avg = 0;
                result.failed = 1;
            }
            else {
                result.avg = triptime;
            }
            return DataActionResult<PingTestData>.Successful(result);
        }
        catch(PingException) {
            return DataActionResult<PingTestData>.Failed(
                StatusMessage.PingExecutionServiceError
            );
        }
    }
    
    public async Task<DataActionResult<PingTestData>> TestConnectionAsync(
        IPAddress ip
    ) {
        List<PingTestData> pingReplies = new List<PingTestData>();
        for(int i = 0; i < maxRetry; i++) {
            DataActionResult<PingTestData> pingResult = await PingAsync(
                ping,
                ip,
                timeout,
                buffer,
                options
            );
            if(pingResult.Status.Failure()) {
                return DataActionResult<PingTestData>.Failed(pingResult.Status);
            }
            pingReplies.Add(pingResult.Result);
            if(pingResult.Result.avg < 50 && pingResult.Result.failed == 0) {
                break;
            }
        }
        PingTestData result = pingReplies.Aggregate((current, next) => {
            current.num += next.num;
            current.avg += next.avg;
            current.failed += next.failed;
            return current;
        });
        int successful = result.num - result.failed;
        if(successful != 0) {
            result.avg /= successful;
        }
        return DataActionResult<PingTestData>.Successful(result);
    }

    public async Task<DataActionResult<IEnumerable<PingTestData>>> TestConnectionListAsync(
        IEnumerable<IPAddress> ips
    ) {
        IEnumerable<Task<DataActionResult<PingTestData>>> tasks = ips.Select(
            ip => TestConnectionAsync(ip)
        );
        IEnumerable<DataActionResult<PingTestData>> results =
            await Task.WhenAll(tasks);
        DataActionResult<PingTestData> firstFailed =
            results.FirstOrDefault(r => r.Status.Failure());
        return firstFailed != null
            ? DataActionResult<IEnumerable<PingTestData>>
                .Failed(firstFailed.Status)
            : DataActionResult<IEnumerable<PingTestData>>
                .Successful(results.Select(r => r.Result));
    }
}

}