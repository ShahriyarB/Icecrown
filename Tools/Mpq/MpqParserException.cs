// <copyright file="MpqParserException.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Tools.Mpq
{
    using System.Runtime.Serialization;

    [Serializable]
    internal class MpqParserException : Exception
    {
        public MpqParserException()
        {
        }

        public MpqParserException(string? message)
            : base(message)
        {
        }

        public MpqParserException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected MpqParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}