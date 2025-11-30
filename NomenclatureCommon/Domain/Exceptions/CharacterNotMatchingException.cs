using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureCommon.Domain.Exceptions
{
    public class CharacterNotMatchingException : Exception
    {
        /// <summary>
        ///     <inheritdoc cref="CharacterNotMatchingException"/>
        /// </summary>
        public CharacterNotMatchingException(string message) : base(message) { }

        /// <summary>
        ///     <inheritdoc cref="CharacterNotMatchingException"/>
        /// </summary>
        public CharacterNotMatchingException(string message, Exception inner) : base(message, inner) { }
    }
}
