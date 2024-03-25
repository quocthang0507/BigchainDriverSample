using Multiformats.Base;
using Multiformats.Hash;

namespace PlanetmintDriver
{
    public static class MultihashExtension
    {
        public static string Multihash(this string str)
        {
            return Multiformats.Hash.Multihash.Encode(str, HashType.SHA2_256).ToString(MultibaseEncoding.Base58Btc);

        }
    }
}
