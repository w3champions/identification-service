using System;

namespace W3ChampionsIdentificationService.DatabaseModels
{
    public interface IVersionable
    {
        public DateTimeOffset LastUpdated { get; set; }
    }
}
