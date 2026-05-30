using System;

namespace W3ChampionsIdentificationService.DatabaseModels;

public interface IVersionable
{
    // MIGRATION NOTE (MongoDB.Driver 3.x): the default BSON serialization of
    // DateTimeOffset changed from a 2-element array [ticks, offset] in driver 2.x to a
    // document { DateTime, Offset } in driver 3.x. Any concrete class implementing this
    // interface that persists existing 2.x-format documents MUST annotate its LastUpdated
    // property with [BsonRepresentation(BsonType.Array)] to stay byte-compatible with
    // already-stored data. The attribute cannot live here: the driver's class map reflects
    // over the concrete type's members and does NOT honor BSON attributes declared on an
    // interface, so an attribute on this interface property would silently be a no-op.
    // (No concrete type implements IVersionable today — this is currently dead code.)
    public DateTimeOffset LastUpdated { get; set; }
}
