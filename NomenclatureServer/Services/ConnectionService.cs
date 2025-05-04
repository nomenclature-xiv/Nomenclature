using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureServer.Services
{
    public class ConnectionService
    {
        /// <summary>
        ///     List of all connection IDs currently connected
        /// </summary>
        public HashSet<string> Connections = new();
    }
}
