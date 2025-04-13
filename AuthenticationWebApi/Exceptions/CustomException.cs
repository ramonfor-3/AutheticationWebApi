namespace AuthenticationWebApi.Exceptions;

public class CustomException
{
    public CustomException(string message): base() { }
}

public class PasswordExpiredException : Exception
{
    public PasswordExpiredException(string message) : base(message) { }
}

public class TokenInvalidException : Exception
{
    public TokenInvalidException(string message) : base(message) { }
}

public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message) : base(message) { }
}