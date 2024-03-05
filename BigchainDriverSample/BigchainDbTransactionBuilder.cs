using log4net;
using Microsoft.IdentityModel.Tokens;
using NSec.Cryptography;
using Omnibasis.BigchainCSharp.Builders;
using Omnibasis.BigchainCSharp.Constants;
using Omnibasis.BigchainCSharp.Model;
using Omnibasis.BigchainCSharp.Util;
using Omnibasis.CryptoConditionsCSharp;
using Omnibasis.CryptoConditionsCSharp.Types;

namespace BigchainDriverSample
{
    public class BigchainDbTransactionBuilder<A0, M0>
    {
        public interface ITransactionAttributes<A1, M1>
        {
            ITransactionAttributes<A1, M1> Operation(string operation);

            ITransactionAttributes<A1, M1> AddOutput(string amount, params PublicKey[] publicKey);

            ITransactionAttributes<A1, M1> AddOutput(string amount);

            ITransactionAttributes<A1, M1> AddOutput(string amount, PublicKey publicKey);

            ITransactionAttributes<A1, M1> AddInput(string fullfillment, FulFill fullFill, params PublicKey[] publicKey);

            ITransactionAttributes<A1, M1> AddInput(string fullfillment, FulFill fullFill);

            ITransactionAttributes<A1, M1> AddInput(string fullfillment, FulFill fullFill, PublicKey publicKey);

            ITransactionAttributes<A1, M1> AddInput(Details fullfillment, FulFill fullFill, params PublicKey[] publicKey);

            ITransactionAttributes<A1, M1> AddAssets(A1 assets);

            //ITransactionAttributes<A1, M1> AddAssets(string id);

            ITransactionAttributes<A1, M1> AddMetaData(M1 data);

            ITransactionAttributes<A1, M1> AddMetaData(MetaData<M1> data);

            IBuild<A1, M1> Build(PublicKey publicKey);

            IBuild<A1, M1> BuildAndSign(PublicKey publicKey, Key privateKey);

            IBuild<A1, M1> BuildAndSign(Key key);

            Transaction<A1, M1> BuildOnly(PublicKey publicKey);

            Transaction<A1, M1> BuildAndSignOnly(PublicKey publicKey, Key privateKey);

            Transaction<A1, M1> BuildAndSignOnly(Key key);
        }

        public interface IBuild<A2, M2>
        {
            Task<BlockchainResponse<Transaction<A2, M2>>> SendTransactionAsync(BigchainDbConfigBuilder.IBlockchainConfigurationBuilder? builder = null);

            Task<BlockchainResponse<Transaction<A2, M2>>> SendTransactionAsync(GenericCallback callback, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder? builder = null);
        }

        public class Builder<A, M> : ITransactionAttributes<A, M>, IBuild<A, M>
        {
            private MetaData<M>? metadata;

            private IList<Asset<A>> assets = [];

            private IList<Input> inputs = [];

            private IList<Output> outputs = [];

            private PublicKey publicKey;

            private Transaction<A, M> transaction;

            private string transationOperation;

            public virtual ITransactionAttributes<A, M> AddOutput(string amount)
            {
                return AddOutput(amount, publicKey);
            }

            public virtual ITransactionAttributes<A, M> AddOutput(string amount, PublicKey publicKey)
            {
                PublicKey[] publicKeys = [publicKey];
                return AddOutput(amount, publicKeys);
            }

            public virtual ITransactionAttributes<A, M> AddOutput(string amount, params PublicKey[] publicKeys)
            {
                foreach (PublicKey key in publicKeys)
                {
                    Output output = new();
                    Ed25519Sha256Condition ed25519Sha256Condition = new(key);
                    output.Amount = amount;
                    output.addPublicKey(KeyPairUtils.encodePublicKeyInBase58(key));
                    Details details = new()
                    {
                        PublicKey = KeyPairUtils.encodePublicKeyInBase58(key),
                        Type = "ed25519-sha-256"
                    };
                    output.Condition = new Condition(details, ed25519Sha256Condition.Uri.ToString());
                    outputs.Add(output);
                }

                return this;
            }

            public virtual ITransactionAttributes<A, M> AddInput(string fullfillment, FulFill fullFill)
            {
                return AddInput(fullfillment, fullFill, publicKey);
            }

            public virtual ITransactionAttributes<A, M> AddInput(string fullfillment, FulFill fullFill, PublicKey publicKey)
            {
                PublicKey[] publicKeys = [publicKey];
                return AddInput(fullfillment, fullFill, publicKeys);
            }

            public virtual ITransactionAttributes<A, M> AddInput(string fulfillment, FulFill fullFill, params PublicKey[] publicKeys)
            {
                foreach (PublicKey publicKey in publicKeys)
                {
                    Input input = new()
                    {
                        FulFillment = fulfillment,
                        FulFills = fullFill
                    };
                    input.addOwner(KeyPairUtils.encodePublicKeyInBase58(publicKey));
                    inputs.Add(input);
                }

                return this;
            }

            public virtual ITransactionAttributes<A, M> AddInput(Details fulfillmentDetails, FulFill fulFill, params PublicKey[] publicKeys)
            {
                foreach (PublicKey publicKey in publicKeys)
                {
                    Input input = new()
                    {
                        FulFillmentDetails = fulfillmentDetails,
                        FulFills = fulFill
                    };
                    input.addOwner(KeyPairUtils.encodePublicKeyInBase58(publicKey));
                    inputs.Add(input);
                }

                return this;
            }

            public virtual ITransactionAttributes<A, M> AddMetaData(M data)
            {
                if (data == null)
                {
                    return this;
                }

                metadata = new MetaData<M>
                {
                    Metadata = data
                };
                return this;
            }

            public virtual ITransactionAttributes<A, M> AddMetaData(MetaData<M> data)
            {
                if (data == null)
                {
                    return this;
                }

                metadata = data;
                return this;
            }

            public virtual ITransactionAttributes<A, M> AddAssets(A obj)
            {
                Asset<A> item = new()
                {
                    Data = obj
                };
                assets.Add(item);
                return this;
            }

            //public virtual ITransactionAttributes<A, M> AddAssets(string id)
            //{
            //    assets = new Asset<A>();
            //    assets.Id = id;
            //    return this;
            //}

            public virtual ITransactionAttributes<A, M> Operation(string transationOperation)
            {
                this.transationOperation = transationOperation;
                return this;
            }

            public virtual IBuild<A, M> Build(PublicKey publicKey)
            {
                transaction = new Transaction<A, M>();
                this.publicKey = publicKey;
                if (outputs.Count == 0)
                {
                    AddOutput("1");
                }

                foreach (Output output in outputs)
                {
                    transaction.AddOutput(output);
                }

                if (inputs.Count == 0)
                {
                    string fullfillment = null;
                    FulFill fullFill = null;
                    AddInput(fullfillment, fullFill);
                }

                foreach (Input input in inputs)
                {
                    transaction.AddInput(input);
                }

                if (transationOperation == Operations.CREATE || transationOperation == Operations.TRANSFER)
                {
                    transaction.Operation = transationOperation;
                    transaction.Assets = assets;
                    transaction.MetaData = metadata;
                    transaction.Version = "3.0";
                    transaction.Id = null;
                    return this;
                }

                throw new Exception("Invalid Operations value. Accepted values are [Operations.CREATE, Operations.TRANSFER]");
            }

            private void Sign(Key key)
            {
                string text = DriverUtils.makeSelfSortingGson(transaction.ToHashInput());
                Ed25519 ed = SignatureAlgorithm.Ed25519;
                foreach (Input input in transaction.Inputs)
                {
                    string text2;
                    if (input.FulFills != null)
                    {
                        FulFill fulFills = input.FulFills;
                        text2 = text + fulFills.TransactionId + fulFills.OutputIndex;
                    }
                    else
                    {
                        text2 = text;
                    }

                    byte[] array = Utils.StringToByteArray(DriverUtils.getSha3HashHex(text2));
                    byte[] signature = ed.Sign(key, array);
                    Ed25519Sha256Fulfillment ed25519Sha256Fulfillment = new(publicKey, signature);
                    input.FulFillment = Base64UrlEncoder.Encode(ed25519Sha256Fulfillment.Encoded);
                }

                text = DriverUtils.makeSelfSortingGson(transaction.ToHashInput());
                string sha3HashHex = DriverUtils.getSha3HashHex(text);
                transaction.Id = sha3HashHex;
            }

            public virtual IBuild<A, M> BuildAndSign(PublicKey publicKey, Key privateKey)
            {
                try
                {
                    Build(publicKey);
                    Sign(privateKey);
                }
                catch (Exception ex)
                {
                    BigchainDbTransactionBuilder<A0, M0>.log.Error(ex.StackTrace);
                    Console.Write(ex.ToString());
                }

                return this;
            }

            public virtual IBuild<A, M> BuildAndSign(Key key)
            {
                try
                {
                    Build(key.PublicKey);
                    Sign(key);
                }
                catch (Exception ex)
                {
                    BigchainDbTransactionBuilder<A0, M0>.log.Error(ex.StackTrace);
                    Console.Write(ex.ToString());
                }

                return this;
            }

            public virtual Transaction<A, M> BuildOnly(PublicKey publicKey)
            {
                Build(publicKey);
                return transaction;
            }

            public virtual Transaction<A, M> BuildAndSignOnly(PublicKey publicKey, Key privateKey)
            {
                BuildAndSign(publicKey, privateKey);
                return transaction;
            }

            public virtual Transaction<A, M> BuildAndSignOnly(Key key)
            {
                BuildAndSign(key);
                return transaction;
            }

            public virtual async Task<BlockchainResponse<Transaction<A, M>>> SendTransactionAsync(GenericCallback callback, BigchainDbConfigBuilder.IBlockchainConfigurationBuilder? builder = null)
            {
                return await TransactionsApi<A, M>.SendTransactionAsync(transaction, callback, builder);
            }

            public virtual async Task<BlockchainResponse<Transaction<A, M>>> SendTransactionAsync(BigchainDbConfigBuilder.IBlockchainConfigurationBuilder? builder = null)
            {
                return await TransactionsApi<A, M>.SendTransactionAsync(transaction, builder);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(BigchainDbTransactionBuilder<A0, M0>));

        private BigchainDbTransactionBuilder()
        {
        }

        public static Builder<A0, M0> Init()
        {
            return new Builder<A0, M0>();
        }
    }
}
