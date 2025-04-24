using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nomenclature.Services
{
    public class FrameworkService
    {
        private readonly IFramework Framework;
        public FrameworkService(IFramework framework)
        {
            Framework = framework;
        }

        /// <summary>
        ///     Runs provided function on the XIV Framework. Await should never be used inside the <see cref="Func{T}"/>
        ///     passed to this function.
        /// </summary>
        public async Task<T> RunOnFramework<T>(Func<T> func)
        {
            if (Framework.IsInFrameworkUpdateThread)
                return func.Invoke();

            return await Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
        }
    }
}

    
