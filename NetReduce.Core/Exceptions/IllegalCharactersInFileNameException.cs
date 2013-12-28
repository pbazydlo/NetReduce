namespace NetReduce.Core.Exceptions
{
    using System;

    class IllegalCharactersInFileNameException : Exception
    {
        public IllegalCharactersInFileNameException()
            : base("File name contains illegal characters") { }
    }
}
