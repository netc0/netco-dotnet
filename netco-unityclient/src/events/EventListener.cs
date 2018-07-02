using System;
namespace netco {
    public class EventListener {
        public string route;
        public UInt32 routeId;
        public int    eventId;
        public Action<byte[]> callback;
    }
}
