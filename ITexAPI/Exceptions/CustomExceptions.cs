namespace ITexAPI.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NotFoundException : BusinessException
    {
        public NotFoundException(string resource, object key)
            : base($"{resource} with id '{key}' was not found.") { }

        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : BusinessException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DuplicateException : BusinessException
    {
        public DuplicateException(string message) : base(message) { }
    }

    public class InsufficientStockException : BusinessException
    {
        public InsufficientStockException(string productName, int requested, int available)
            : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}") { }
    }
}