/**
 * Taken from the C# DogeAPI Wrapper
 *
 * Version 0.1.0.0
 * 
 * Written by Jesse Sheehan (jesse@binarycave.com)
 * 
 * This file is released under the MIT license
 */

#region

using System;

#endregion

namespace DogeMuch.Utility
{
    /// <summary>
    ///     The catch-all exception thrown by all methods in DogeClient
    /// </summary>
    public class DogeException : Exception
    {
        /// <summary>
        ///     The list of pronouns to use when calling Generate
        /// </summary>
        private static readonly string[] Pronouns = {"so", "many", "such", "very", "much", "more"};

        /// <summary>
        ///     The list of nouns to use when calling Generate
        /// </summary>
        private static readonly string[] Nouns =
        {
            "sad", "depress", "trouble", "problem", "stress", "error", "woe",
            "worry", "muddle", "issue", "dilemma", "setback"
        };

        /// <summary>
        ///     Create an instance of DogeException
        /// </summary>
        public DogeException()
        {
        }

        /// <summary>
        ///     Create an instance of DogeException with a message
        /// </summary>
        /// <param name="message">The error message</param>
        public DogeException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Create an instance of DogeException with a message and inner exception
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner exception thrown</param>
        public DogeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Generates a more fun error message
        /// </summary>
        /// <param name="message">The serious error message</param>
        /// <param name="inner">The inner exception</param>
        /// <returns></returns>
        public static DogeException Generate(string message, Exception inner)
        {
            try
            {
                // In the spirit of doge, we attempt to select a random pronoun and a random noun to use
                // in the error message
                var rand = new Random();
                var pronoun = Pronouns[rand.Next(0, Pronouns.Length)];
                var noun = Nouns[rand.Next(0, Nouns.Length)];
                if (inner != null)
                    inner = new Exception(inner.Message.ToLower());
                return new DogeException(String.Format("wow, {0}, {1} {2}", message, pronoun, noun), inner);
            }
            catch (Exception)
            {
                throw new DogeException("wow, failed to generate exception, such meta", inner);
            }
        }
    }
}