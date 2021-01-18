using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace W3ChampionsIdentificationService.W3CAuthentication
{
    public class Role
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Members { get; set; } = new List<string>();
        public List<string> Owners { get; set; } = new List<string>();
        public bool isProtected { get; set; }

        public static Role Create(string name, string creatingUser)
        {
            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };
            role.Members.Add(creatingUser);
            role.Owners.Add(creatingUser);
            return role;
        }
    }
}