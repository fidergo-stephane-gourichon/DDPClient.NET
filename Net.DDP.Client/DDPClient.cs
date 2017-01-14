using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;
using System;
using System.Threading;
using System.Reactive.Subjects;

namespace Net.DDP.Client
{
    public class DDPClient
    {
        public const string DDP_PROPS_MESSAGE = "msg";
        public const string DDP_PROPS_ID = "id";
        public const string DDP_PROPS_COLLECTION = "collection";
        public const string DDP_PROPS_FIELDS = "fields";
        public const string DDP_PROPS_SESSION = "session";
        public const string DDP_PROPS_RESULT = "result";
        public const string DDP_PROPS_ERROR = "error";
        public const string DDP_PROPS_SUBS = "subs";
        public const string DDP_PROPS_METHODS = "methods";
        public const string DDP_PROPS_VERSION = "version";

        private static Random random = new Random ();
        private static object randomLock = new object ();
        private object aLock = new object ();

        private readonly DDPConnector _connector;

        private IObservable<DDPMessage> rawStream;

        public DDPClient ()
        {
            _connector = new DDPConnector ();
        }

        public IObservable<DDPMessage> Connect (string url)
        {
            Validations.ValidateUrl (url);
            lock (aLock) {
                rawStream = _connector.Connect (url);
                var obs = rawStream.Where (message => IsConnectedOrFailedOrErrorType (message)).Replay ();
                obs.Connect ();
                return obs;
            }
        }

        public void Close ()
        {
            lock (aLock) {
                if (_connector != null) {
                    _connector.Close ();
                }
            }
        }

        private bool IsConnectedOrFailedOrErrorType (DDPMessage m)
        {
            return DDPType.Connected.Equals (m?.Type)
            || DDPType.Failed.Equals (m?.Type)
            || DDPType.Error.Equals (m?.Type);
        }

        public IObservable<DDPMessage> Call (string methodName, params object[] args)
        {
            var id = NextId ();
            string message = string.Format ("\"msg\": \"method\",\"method\": \"{0}\",\"params\": {1},\"id\": \"{2}\"",
                                 methodName, CreateJSonArray (args), id);
            message = "{" + message + "}";
            _connector.Send (message);
            var obs = rawStream.Where (ddpMessage => IsRelatedToMethodCall (id, ddpMessage)).Take (2).Replay ();
            obs.Connect ();
            return obs;
        }

        public IObservable<DDPMessage> Subscribe (string subscribeTo, params object[] args)
        {
            var id = NextId ();
            string message = string.Format ("\"msg\": \"sub\",\"name\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"",
                                 subscribeTo, CreateJSonArray (args), id);
            message = "{" + message + "}";
            _connector.Send (message);
            var obs = rawStream.Where (ddpMessage => IsRelatedToSubscription (id, ddpMessage, subscribeTo)).Replay ();
            obs.Connect ();
            return obs;
        }

        public IObservable<DDPMessage> GetCollectionStream (string collectionName)
        {
            var obs = rawStream.Where (ddpMessage => IsRelatedToCollection (collectionName, ddpMessage)).Replay ();
            obs.Connect ();
            return obs;
        }

        private string CreateJSonArray (params object[] args)
        {
            if (args == null)
                return "[]";
            
            return JsonConvert.SerializeObject (args);
        }

        private int NextId ()
        {
            lock (randomLock) {
                return random.Next ();
            }
        }

        private static bool IsRelatedToMethodCall (int id, DDPMessage message)
        {
            return id.ToString ().Equals (message?.Id)
            || (DDPType.Updated.Equals (message?.Type)
            && message?.Methods != null && message.Methods.Contains (id.ToString ())); 
        }

        private static bool IsRelatedToSubscription (int id, DDPMessage message, string collection)
        {
            return id.ToString ().Equals (message?.Id)
            || (DDPType.Ready.Equals (message?.Type)
            && message?.Subs != null && message.Subs.Contains (id.ToString ()))
            || (collection != null && collection.Equals (message?.Collection));
        }

        private static bool IsRelatedToCollection (string collection, DDPMessage message)
        {
            return collection != null && collection.Equals (message?.Collection);
        }
    }
}
