namespace Data.Model.ViewModel
{
    public class NtwkNode
    {
        public int Id;
        public int? ParentId;
        public int? ParentPort;
        public string Name;
        public string IpStr;
        public bool IsOpenSsh, IsOpenTelnet, IsOpenPing;
    }
}