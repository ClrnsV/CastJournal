using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Domain.Enums;

public enum AuditAction
{
    Login,
    LoginFailed,
    Logout,
    Register,
    UserCreated,
    UserUpdated,
    UserDeactivated,
    UserReactivated,
    CatchDeleted,
    LocationDeleted,
    SpeciesCreated,
    SpeciesUpdated,
    SpeciesDeleted,
    CategoryCreated,   
    CategoryUpdated,   
    CategoryDeleted,
    BackupCreated,
    BackupRestored,
    DataCleanup
}
