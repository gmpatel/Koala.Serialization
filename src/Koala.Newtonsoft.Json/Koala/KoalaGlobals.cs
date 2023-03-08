using System;
using System.Collections.Generic;
using System.Text;

namespace Koala.Core
{
    public static class KoalaGlobals
    {
        public static string AppIdentifier { get; set; } = Guid.Empty.ToString();
    }
}
