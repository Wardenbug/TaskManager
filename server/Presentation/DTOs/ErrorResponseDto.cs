namespace Presentation.DTOs
{
    public sealed class ErrorResponseDto
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }

        public ErrorResponseDto(string message, string errorCode = null)
        {
            Message = message;
            ErrorCode = errorCode;
            Errors = new Dictionary<string, string[]>();
        }

        public ErrorResponseDto(string message, string field, string error)
        {
            Message = message;
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { error } }
            };
        }

        public ErrorResponseDto(string message, IDictionary<string, string[]> errors)
        {
            Message = message;
            Errors = errors;
        }
    }
}
