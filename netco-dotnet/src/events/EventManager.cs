using System;
using System.Collections.Generic;

namespace netco {
    public class EventManager {
        private int eventId = 0;
        private Dictionary<string, List<EventListener>> events = new Dictionary<string, List<EventListener>>();

        public int AddEvent(string route, Action<byte[]> callback) {
            lock(this) {
                var evt = new EventListener();
                evt.route = route;
                evt.routeId = Crc32.GetCRC32Str(route);
                evt.eventId = eventId;
                evt.callback = callback;

                if (!events.ContainsKey(route)) {
                    events[route] = new List<EventListener>();
                }
                events[route].Add(evt);
            }
            return eventId++;
        }

        public void RemoveEvent(int id) {
            lock(this) {
                bool found = false;
                foreach(var kv in events) {
                    foreach(var item in kv.Value) {
                        if (item.eventId == id) {
                            kv.Value.Remove(item);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }
        }

        public void OnPushMessage(UInt32 routeId, byte[] data) {
            var evt = GetEventListener(routeId);
            if (evt == null) {
                NDebug.Log("未监听: " + routeId);
                return;
            }
            try {
                evt.callback.Invoke(data);
            }catch (Exception e){
                NDebug.Log(e);
            }
        }

        private EventListener GetEventListener(UInt32 routeId) {
            lock (this) {
                foreach (var kv in events) {
                    foreach (var item in kv.Value) {
                        if (item.routeId == routeId) {
                            return item;
                        }
                    }
                }
            }
            return null;
        }
    }
}
