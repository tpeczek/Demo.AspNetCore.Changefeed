using Microsoft.Azure.Documents.Client;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal interface IDocumentClientSingletonProvider
    {
        DocumentClient DocumentClientSingleton { get; }
    }
}
