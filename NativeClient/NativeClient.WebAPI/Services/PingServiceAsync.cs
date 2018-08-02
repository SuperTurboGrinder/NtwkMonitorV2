using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Services {

public class PingServiceAsync: IPingService {
    Ping ping = new Ping();
    //64bytes
    byte[] buffer = Enumerable
        .Repeat(new byte[]{ 0xDE, 0xAD, 0xBE, 0xEF }, 8)
        .SelectMany(s=>s).ToArray();
        
    int timeout = 1000;
    PingOptions options = new PingOptions(64, true);
    
    public async Task<PingTestData> PingAsync(IPAddress ip) {
        long triptime = -1;
        PingTestData result = new PingTestData() { num = 1 };
        try {
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
        }
        catch(PingException) {
            result.failed = 1;
            result.error = "Ping execution service error";
        }
        return result;
    }
    
    public async Task<PingTestData> TestConnectionAsync(IPAddress ip) {
        List<PingTestData> pingReplys = new List<PingTestData>();
        for(int i = 0; i < 4; i++) {
            PingTestData reply = await PingAsync(ip);
            pingReplys.Add(reply);
            if(reply.avg < 50 && (reply.failed == 0 || reply.error != null)) {
                break;
            }
        }
        PingTestData result = pingReplys.Aggregate((current, next) => {
            current.num += next.num;
            current.avg += next.avg;
            current.failed += next.failed;
            if(current.error == null) {
                current.error = next.error;
            }
            return current;
        });
        int successful = result.num - result.failed;
        if(successful != 0) {
            result.avg /= successful;
        }
        return result;
    }
}

}