using Newtonsoft.Json;
using Omnibasis.BigchainCSharp.Model;

namespace BigchainDriverSample
{
    [Serializable]
    public class Transaction<A, M>
    {
        [JsonProperty("assets")]
        public IList<Asset<A>> Assets { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }

        [JsonProperty("inputs")]
        public IList<Input> Inputs { get; set; }

        [JsonProperty(PropertyName = "metadata", NullValueHandling = NullValueHandling.Include)]
        public MetaData<M> MetaData { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("outputs")]
        public IList<Output> Outputs { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public Transaction()
        {
            Assets = [];
            Inputs = [];
            Outputs = [];
        }

        public virtual void AddInput(Input input)
        {
            Inputs.Add(input);
        }

        public virtual void AddOutput(Output output)
        {
            Outputs.Add(output);
        }

        public override string ToString()
        {
            return ToHashInput();
        }

        public virtual string ToHashInput()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
