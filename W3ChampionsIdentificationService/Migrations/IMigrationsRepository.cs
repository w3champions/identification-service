using System;
using System.Threading.Tasks;

namespace W3ChampionsIdentificationService.Migrations;

public interface IMigrationsRepository
{
    // Runs `migration` if `migrationId` has not been recorded as applied.
    // The sentinel is only written AFTER `migration` completes without throwing,
    // so an interrupted run leaves no record and retries on next startup. The
    // caller's migration body must therefore be idempotent at the row level.
    Task RunIfNeeded(string migrationId, Func<Task> migration);
}
