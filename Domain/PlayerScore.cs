using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain;

public class PlayerScore
{
    public ObjectId Id { get; set;}

    [BsonElement("userId")]
    public int UserId { get; set; }

    [BsonElement("totalScore")]
    public int TotalScore { get; set; }

    [BsonIgnore]
    public long Place { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
