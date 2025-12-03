using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Types
{
    public enum AppStoredFileStatusTypes
    {
        Processing = 1,
        Error = 2,
        Complete = 3,
        PendingWithError = 4,
    }
}
