namespace NetCoreAPIRainfall.Models
{
    public class error
    {
        public string message { get; set; }
        public List<errorDetail> detail { get; set; }
    }
}
