﻿using MongoDB.Bson.Serialization.Attributes;
using W3ChampionsIdentificationService.DatabaseModels;

namespace W3ChampionsIdentificationService.Identity;

public class MicrosoftIdentity : IIdentifiable
{
    [BsonId]
    public string Id { get; set; }
    public string battleTag { get; set; }
}
