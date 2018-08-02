namespace NativeClient.WebAPI.Services.Model {
    
public class PingTestData {
    public int num { get;set; } = 0;
    public int failed { get;set; } = 0;
    public long avg { get;set; } = 0;
    public string error { get;set; } = null;
}

}