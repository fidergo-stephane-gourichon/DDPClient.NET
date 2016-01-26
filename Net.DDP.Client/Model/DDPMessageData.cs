using System;

namespace Net.DDP.Client
{
    public class DDPMessageData
    {
        /// <summary>
        /// The Id field included in almost every DDP message. Its meaning varies depending on the type of the message.
        /// </summary>
        /// <value>The DDP message Id.</value>
        public string Id { get; private set; }

        public Error Error { get; protected set; }

        public string CollectionName { get; private set; }

        public DDPType Type { get; private set; }

        public DDPMessageData (DDPType type, string ddpId = null, Error error = null, string collectionName = null)
        {
            Id = ddpId;
            Error = error;
            Type = type;
            CollectionName = collectionName;
        }

        public override string ToString ()
        {
            return string.Format ("[DDPMessageData: Id={0}, Error={1}, CollectionName={2}, Type={3}]", Id, Error, CollectionName, Type);
        }
        
    }
}

