using System;

namespace wj.ObjectComparer
{
    /// <summary>
    /// Helper class that provides guard methods to validate argument values.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Checks the given argument value for null, and if it is, it throws an 
        /// <see cref="ArgumentNullException"/> exception.
        /// </summary>
        /// <param name="arg">The argument value to test.</param>
        /// <param name="paramName">The parameter name that corresponds to the argument value 
        /// being tested.</param>
        /// <exception cref="ArgumentNullException">Thrown if the given argument value is null.</exception>
        public static void RequiredArgument(object arg, string paramName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Runs the specified predicate function; if the return value is false, an 
        /// <see cref="ArgumentException"/> exception is thrown.
        /// </summary>
        /// <param name="predicate">Function that returns a Boolean value as the result of 
        /// argument validation.</param>
        /// <param name="paramName">The parameter name that corresponds to the argument being 
        /// tested.</param>
        /// <param name="message">The error message that will be set in the thrown 
        /// <see cref="ArgumentException"/> exception.</param>
        /// <exception cref="ArgumentException">Thrown if the test enclosed in the predicate 
        /// function returns false.</exception>
        public static void ArgumentCondition(Func<bool> predicate, string paramName, string message)
        {
            if (!predicate())
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Runs the specified predicate function; if the return value is false, an 
        /// <see cref="InvalidOperationException"/> exception is thrown.
        /// </summary>
        /// <param name="predicate">Function that returns a Boolean value as the result of some 
        /// </param>
        /// <param name="message"></param>
        public static void Condition(Func<bool> predicate, string message)
        {
            if (!predicate())
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
