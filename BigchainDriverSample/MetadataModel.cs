using Newtonsoft.Json;

namespace BigchainDriverSample
{
    [Serializable]
    public class MetadataModel
    {
        [JsonProperty]
        public string Message { get; set; }
    }
}
