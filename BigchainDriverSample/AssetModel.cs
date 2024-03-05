using Multiformats.Base;
using Multiformats.Hash;
using Newtonsoft.Json;

namespace BigchainDriverSample
{
    public class AssetModel<T>
    {
        public required T Body { get; set; }

        public override string ToString()
        {
            return Multihash.Encode(Body?.ToString(), HashType.SHA2_256).ToString(MultibaseEncoding.Base58Btc);
        }

        public object ToObject()
        {
            return ToString();
        }
    }
}
