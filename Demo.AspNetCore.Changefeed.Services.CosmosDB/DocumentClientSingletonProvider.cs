using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal class DocumentClientSingletonProvider : IDocumentClientSingletonProvider, IDisposable
    {
        private bool _disposed = false;

        public DocumentClient DocumentClientSingleton { get; }

        public DocumentClientSingletonProvider(IOptions<CosmosDbOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (String.IsNullOrWhiteSpace(options.Value.EndpointUrl))
            {
                throw new ArgumentNullException(nameof(CosmosDbOptions.EndpointUrl));
            }

            if (String.IsNullOrWhiteSpace(options.Value.AuthorizationKey))
            {
                throw new ArgumentNullException(nameof(CosmosDbOptions.AuthorizationKey));
            }

            DocumentClientSingleton = new DocumentClient(new Uri(options.Value.EndpointUrl), options.Value.AuthorizationKey);
            //DocumentClientSingleton = new DocumentClient(new Uri(options.Value.EndpointUrl), options.Value.AuthorizationKey, new ConnectionPolicy
            //{
            //    ConnectionMode = ConnectionMode.Direct,
            //    ConnectionProtocol = Protocol.Tcp
            //});
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                DocumentClientSingleton.Dispose();

                GC.SuppressFinalize(this);

                _disposed = true;
            }
        }
    }
}
