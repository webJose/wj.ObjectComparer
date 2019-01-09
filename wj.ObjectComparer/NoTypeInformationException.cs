using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Exception raised whenever a new object comparer is created and one of the types yield no 
    /// type information.
    /// </summary>
    /// <remarks>To resolve this error, usually a call to <see cref="Scanner.RegisterType(Type)"/>
    /// resolved the problem.</remarks>
    public class NoTypeInformationException : Exception
    {
        #region Static Section
        /// <summary>
        /// Returns the default exception message.
        /// </summary>
        /// <param name="type">The type not whose data was not found.</param>
        /// <returns>The exception message to be used if no message is provided.</returns>
        private static string GetDefaultMessage(Type type)
            => $"Could not find scanned class information for type {type}.";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type whose data was not found.
        /// </summary>
        public Type Type { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of this exception class using the specified type to inform the 
        /// consumer.
        /// </summary>
        /// <param name="type">The type whose data was not found.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">Any inner exception related to this exception.</param>
        public NoTypeInformationException(Type type, string message = null, System.Exception innerException = null)
            : base(String.IsNullOrWhiteSpace(message) ? GetDefaultMessage(type) : message, innerException)
        {
            Type = type;
        }
        #endregion
    }
}