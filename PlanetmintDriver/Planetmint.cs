using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using NSec.Cryptography;
using Omnibasis.BigchainCSharp.Builders;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Util;
using System.Net;

namespace PlanetmintDriver
{
    /// <summary>
    /// Single-node Planetmint server
    /// </summary>
    /// <typeparam name="A">Asset model</typeparam>
    /// <typeparam name="M">Metadata model</typeparam>
    public class Planetmint<A, M>
    {
        private string privateKeyString;
        private string publicKeyString;
        private Key privateKey;
        private PublicKey publicKey;
        private BigchainDbConfigBuilder.IBlockchainConfigurationBuilder builder;
        private ILogger logger;
        private Transaction<A, M> transaction;

        public string Transaction => transaction.ToString();

        public Planetmint(string serverUrl, string privateKeyString, string publicKeyString)
        {
            this.privateKeyString = privateKeyString;
            this.publicKeyString = publicKeyString;
            // Initialize fields
            builder = BigchainDbConfigBuilder.baseUrl(serverUrl);
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            logger = factory.CreateLogger("Planetmint");
            // Prepare keys
            var algorithm = SignatureAlgorithm.Ed25519;
            privateKey = Key.Import(algorithm, Utils.StringToByteArray(privateKeyString), KeyBlobFormat.PkixPrivateKey);
            publicKey = PublicKey.Import(algorithm, Utils.StringToByteArray(publicKeyString), KeyBlobFormat.PkixPublicKey);
        }

        /// <summary>
        /// Setup the connection to Blockchain network
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Failed to setup</exception>
        public async Task<Planetmint<A, M>> InitNetwork(A asset, M metadata)
        {
            if (!await builder.setup())
            {
                logger.LogError(Constants._503);
                throw new Exception(Constants._503);
            };
            return this;
        }

        /// <summary>
        /// Set up, sign your transaction
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Planetmint<A, M> PrepareTransaction(A asset, M metadata)
        {
            transaction = BigchainDbTransactionBuilder<A, M>
                .Init()
                .AddAssets(asset)
                .AddMetaData(metadata)
                .Operation(Operations.CREATE)
                .BuildAndSignOnly(publicKey, privateKey);
            return this;
        }

        /// <summary>
        /// Get transaction by asset ID
        /// </summary>
        /// <param name="id">Asset ID</param>
        /// <returns></returns>
        public async Task<Transaction<A, M>> GetTransactionById(string id)
        {
            var transaction = await TransactionsApi<A, M>.GetTransactionByIdAsync(id);
            return transaction;
        }

        /// <summary>
        /// Send transaction and verify asset's ID equal to transaction ID
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Send()
        {
            var response = await TransactionsApi<A, M>.SendTransactionAsync(transaction);
            // Failed to send transaction
            if (response == null || response.Data == null)
            {
                logger.LogError(Constants._400);
                throw new Exception(Constants._400);
            }
            var assetId = response.Data.Id;
            // Failed to find asset's id
            var transaction2 = await GetTransactionById(assetId);
            if (transaction2 == null)
            {
                logger.LogError(Constants._404);
                throw new Exception(Constants._404);
            }
        }

        public async Task Transfer()
        {

        }
    }
}
