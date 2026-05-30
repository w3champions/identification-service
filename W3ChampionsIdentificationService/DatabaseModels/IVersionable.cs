using System;

namespace W3ChampionsIdentificationService.DatabaseModels;

public interface IVersionable
{
    // MIGRATION NOTE (MongoDB.Driver 3.x): the default BSON serialization of
    // DateTimeOffset changed from a 2-element array [ticks, offset] in driver 2.x to a
    // document { DateTime, Offset } in driver 3.x. A concrete class implementing this
    // interface must, if it persists pre-existing 2.x-format documents, annotate its
    // LastUpdated property with [BsonRepresentation(BsonType.Array)] to stay byte-compatible
    // with already-stored data. (A brand-new collection that never held 2.x data needs no
    // such attribute — the 3.x default is fine.) The attribute cannot live here: the driver's class map reflects
    // over the concrete type's members and does NOT honor BSON attributes declared on an
    // interface, so an attribute on this interface property would silently be a no-op.
    // (No concrete type implements IVersionable today — this is currently dead code.)
    public DateTimeOffset LastUpdated { get; set; }
}
