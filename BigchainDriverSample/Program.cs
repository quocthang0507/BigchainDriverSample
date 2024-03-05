using BigchainDriverSample;
using Nito.AsyncEx;
using NSec.Cryptography;
using Omnibasis.BigchainCSharp.Builders;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Util;

internal class Program
{
    private static readonly string publicKeyString = "302a300506032b657003210033c43dc2180936a2a9138a05f06c892d2fb1cfda4562cbc35373bf13cd8ed373";
    private static readonly string privateKeyString = "302e020100300506032b6570042204206f6b0cd095f1e83fc5f08bffb79c7c8a30e77a3ab65f4bc659026b76394fcea8";

    private static void Main(string[] args)
    {
        // single connection
        var builder = BigchainDbConfigBuilder.baseUrl("https://test.ipdb.io");

        if (!AsyncContext.Run(() => builder.setup()))
        {
            Console.WriteLine("Failed to setup");
        };

        // prepare your key
        var algorithm = SignatureAlgorithm.Ed25519;
        var privateKey = Key.Import(algorithm, Utils.StringToByteArray(privateKeyString), KeyBlobFormat.PkixPrivateKey);
        var publicKey = PublicKey.Import(algorithm, Utils.StringToByteArray(publicKeyString), KeyBlobFormat.PkixPublicKey);

        Random random = new();
        AssetModel<string> assetData = new()
        {
            Body = "Blockchain all the things!"
        };

        // Set up, sign, and send your transaction
        var transaction = BigchainDriverSample.BigchainDbTransactionBuilder<object, MetadataModel>
            .Init()
            .AddAssets(assetData.ToObject())
            .Operation(Operations.CREATE)
            .BuildAndSignOnly(publicKey, privateKey);

        Console.WriteLine(transaction.ToString());

        var createTransaction = AsyncContext.Run(() => TransactionsApi<object, MetadataModel>.SendTransactionAsync(transaction));
        string assetId2 = "";

        // the asset's ID is equal to the ID of the transaction that created it
        if (createTransaction != null && createTransaction.Data != null)
        {
            assetId2 = createTransaction.Data.Id;
            var testTran2 = AsyncContext.Run(() => TransactionsApi<object, object>.GetTransactionByIdAsync(assetId2));
            if (testTran2 != null)
                Console.WriteLine("Hello assetId: " + assetId2);
            else
                Console.WriteLine("Failed to find assetId: " + assetId2);

        }
        else if (createTransaction != null)
        {
            Console.WriteLine("Failed to send transaction: " + createTransaction.Messsage.Message);
        }

        Console.ReadKey(true);
    }
}