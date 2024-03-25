using PlanetmintDriver;

namespace BigchainDriverSample
{
    public class AssetModel<T>
    {
        public required T Body { get; set; } = default!;

        public override string ToString()
        {
            return Body.ToString().Multihash();
        }

        public object ToObject()
        {
            return ToString();
        }
    }
}
