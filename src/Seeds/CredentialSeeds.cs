using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace dotnet_sample_action.Seeds;

public static class CredentialSeeds
{
    private const string CollectionName = "Credential";

    public static async Task SeedCredentials(IMongoDatabase db)
    {
        var collection = db.GetCollection<BsonDocument>(CollectionName);
        var credDocument = new BsonDocument
        {
            {"Token", "test"},
            {"NetworkName", "FloInternal"},
            { "Url", "test" },
            { 
                "Roles", new BsonDocument
                {
                    {"Role", "EMSP"},
                    {
                        "BusinessDetails", new BsonDocument
                        {
                            { "Name", "test" },
                            { "Website", BsonNull.Value },
                            { "Logo", BsonNull.Value }
                        }
                    },
                    {"PartyId", "TST"},
                    {"CountryCode", "CA"}
                }
            },
            {
                "LocationFilters", new BsonDocument
                {
                    {"OwnerIds", new BsonArray()},
                    {"FilterTypes", new BsonArray() {"Public", "Residential", "Private", "PublicHidden"}}
                }
            }
        };
        await collection.InsertOneAsync(credDocument);
    }

    public static async Task ClearCredentials(IMongoDatabase db)
    {
        await db.DropCollectionAsync(CollectionName);
    }

    public static string GetValidToken()
    {
        return "test";
    }

    public static string GetInvalidToken()
    {
        return "invalid_token";
    }
}
