using log4net;
using Omnibasis.BigchainCSharp.Api;
using Omnibasis.BigchainCSharp.Builders;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Model;
using Omnibasis.BigchainCSharp.Util;
using System.Net;

namespace PlanetmintDriver
{
    public class TransactionsApi<A, M> : AbstractApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TransactionsApi<A, M>));

        public static async Task<BlockchainResponse<Transaction<A, M>>> SendTransactionAsync(Transaction<A, M> transaction, GenericCallback callback, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder builder = null)
        {
            log.Debug("sendTransaction Call :" + transaction);
            builder ??= BigchainDbConfigBuilder.Builder;

            return await NetworkUtils.sendPostRequest<Transaction<A, M>>(BigchainDbApi.TRANSACTIONS.ToString(), transaction, callback, builder);
        }

        public static async Task<BlockchainResponse<Transaction<A, M>>> SendTransactionAsync(Transaction<A, M> transaction, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder builder = null)
        {
            log.Debug("sendTransaction Call :" + transaction);
            builder ??= BigchainDbConfigBuilder.Builder;

            return await NetworkUtils.sendPostRequest<Transaction<A, M>>(BigchainDbApi.TRANSACTIONS.ToString(), transaction, null, builder);
        }

        public static async Task<Transaction<A, M>> GetTransactionByIdAsync(string id, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder builder = null)
        {
            log.Debug("getTransactionById Call :" + id);
            builder ??= BigchainDbConfigBuilder.Builder;

            BlockchainResponse<Transaction<A, M>> blockchainResponse = await NetworkUtils.sendGetRequest<Transaction<A, M>>(BigchainDbApi.TRANSACTIONS?.ToString() + "/" + id, null, builder);
            return blockchainResponse.Status == HttpStatusCode.OK ? blockchainResponse.Data : null;
        }

        public static async Task<List<Transaction<A, M>>> GetTransactionsByAssetIdAsync(string assetId, string operation = null, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder builder = null)
        {
            log.Debug("getTransactionsByAssetId Call :" + assetId + " operation " + operation);
            builder ??= BigchainDbConfigBuilder.Builder;

            string text = BigchainDbApi.TRANSACTIONS?.ToString() + "?asset_id=" + assetId;
            if (!string.IsNullOrEmpty(operation))
            {
                text = text + "&operation=" + operation;
            }

            BlockchainResponse<List<Transaction<A, M>>> blockchainResponse = await NetworkUtils.sendGetRequest<List<Transaction<A, M>>>(text, null, builder);
            return blockchainResponse.Status == HttpStatusCode.OK ? blockchainResponse.Data : null;
        }
    }
}
