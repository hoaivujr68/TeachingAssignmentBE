using TeachingAssignmentApp.Model;
using System.Text.Json.Serialization;

namespace TeachingAssignmentApp.Helper
{
    public class ResponseObject<T>
    {
        public T Data { get; set; }

        public ResponseObject()
        {
        }

        [JsonConstructor]
        public ResponseObject(T data)
        {
            Data = data;
        }
    }
}
