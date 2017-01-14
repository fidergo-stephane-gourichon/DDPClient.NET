using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Some fields are spotted by compiled as never used.  They are.
// Probably accessed only through reflection by Newtonsoft.Json .
#pragma warning disable 0649
namespace Net.DDP.Client
{
    public class DDPMessage
    {
        [JsonProperty (DDPFields.Msg)]
        private string type;

        public DDPType Type { set { type = BuildFromDDPType(value); } get {return BuildFromString (type);}}

        [JsonProperty (DDPFields.Id)]
        public string Id { get; set; }

        [JsonProperty (DDPFields.Error)]
        public Error Error { get; set; }

        [JsonProperty (DDPFields.Result)]
        private JRaw resultRaw;

        public string Result => (string) resultRaw?.Value;

        [JsonProperty (DDPFields.Fields)]
        private JRaw fieldsRaw;

        public string Fields => (string) fieldsRaw?.Value;

        [JsonProperty (DDPFields.Methods)]
        public List<string> Methods { get; set; }

        [JsonProperty (DDPFields.Collection)]
        public string Collection { get; set; }

        [JsonProperty (DDPFields.Subs)]
        public List<string> Subs { get; set; }

        [JsonIgnore]
        public DDPMessageData DDPMessageData => new DDPMessageData(this.Type, Id, Error, Collection);

        private DDPType BuildFromString (string type)
        {
            switch (type) {
            case DDPInternalType.Ready:
                return DDPType.Ready;
            case DDPInternalType.Added:
                return DDPType.Added;
            case DDPInternalType.Changed:
                return DDPType.Changed;
            case DDPInternalType.Connected:
                return DDPType.Connected;
            case DDPInternalType.Error:
                return DDPType.Error;
            case DDPInternalType.Failed:
                return DDPType.Failed;
            case DDPInternalType.NoSub:
                return DDPType.NoSub;
            case DDPInternalType.Removed:
                return DDPType.Removed;
            case DDPInternalType.Result:
                return DDPType.Result;
            case DDPInternalType.Updated:
                return DDPType.Updated;
            default:
                return DDPType.Error;
            }
        }

        private string BuildFromDDPType (DDPType type)
        {
            switch (type) {
            case DDPType.Ready:
                return DDPInternalType.Ready;
            case DDPType.Added:
                return DDPInternalType.Added;
            case DDPType.Changed:
                return DDPInternalType.Changed;
            case DDPType.Connected:
                return DDPInternalType.Connected;
            case DDPType.Error:
                return DDPInternalType.Error;
            case DDPType.Failed:
                return DDPInternalType.Failed;
            case DDPType.NoSub:
                return DDPInternalType.NoSub;
            case DDPType.Removed:
                return DDPInternalType.Removed;
            case DDPType.Result:
                return DDPInternalType.Result;
            case DDPType.Updated:
                return DDPInternalType.Updated;
            default:
                return DDPInternalType.Error;
            }
        }

        public override string ToString()
        {
            return string.Format("[DDPMessage: Id={0}, Error={1}, Collection={2}, Type={3}, raw:{4}]", Id, Error, Collection, Type,
            ((resultRaw != null) ? resultRaw.ToString() : "(no raw)") );
        }
    }
}
