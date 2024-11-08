using System.Text.Json.Serialization;

namespace TeachingAssignmentApp.Model
{
    public class ResponseList<T>
    {
        public List<T> Data { get; set; }
        public Code Code { get; set; } = Code.Success;
        public string Message { get; set; } = "Success";

        [JsonConstructor]
        public ResponseList(Code code, List<T> data, string message)
        {
            Code = code;
            Data = data;
            Message = message;
        }
        public ResponseList(string message)
        {
            Message = message;
        }

        public ResponseList()
        {
        }
    }
}
