using System.Text.Json.Serialization;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Helper
{
    public class ResponsePagination<T>
    {
        public Pagination<T> Data { get; set; }
        public Code Code { get; set; } = Code.Success;
        public string Message { get; set; } = "Success";

        [JsonConstructor]
        public ResponsePagination(Pagination<T> data, Code code, string message)
        {
            Data = data;
            Code = code;
            Message = message;
        }

        public ResponsePagination(string message)
        {
            Message = message;
        }

        public ResponsePagination()
        {
        }
    }
}
