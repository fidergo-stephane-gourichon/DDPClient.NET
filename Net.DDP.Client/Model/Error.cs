using System;
using Newtonsoft.Json;

namespace Net.DDP.Client
{
    public class Error
    {
        [JsonProperty (DDPFields.Error)]
        public string Code { get; }

        public string Reason { get; }

        public string Message { get; }

        public string ErrorType { get; }

        public Error (string code = null, string reason = null, string message = null, string errorType = null)
        {
            Code = code;
            Reason = reason;
            Message = message;
            ErrorType = errorType;
        }

        public override string ToString ()
        {
            return string.Format ("[Error: Code={0}, Reason={1}, Message={2}, ErrorType={3}]", Code, Reason, Message, ErrorType);
        }
        
    }
}

