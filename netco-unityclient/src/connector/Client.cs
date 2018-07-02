using System;
namespace netco {

    /// <summary>
    /// 网络状态
    /// </summary>
    public enum NetworkState {
        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected,

        /// <summary>
        /// 已关闭
        /// </summary>
        Closed,

        /// <summary>
        /// 已断开
        /// </summary>
        Disconnected,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 连接超时
        /// </summary>
        Timeout
    }

    public class Client {
        protected NetworkState networkState;
        protected EventManager eventManager = new EventManager();
        public event Action<NetworkState> OnNetworkStateChanged;
        protected Protocol protocol = null;
        public int HeartBeatInterval {
            get {
                return _HeartBeatInterval;
            }
            set {
                _HeartBeatInterval = value;
            }
        }
        int _HeartBeatInterval = 3;
        /// <summary>
        /// 网络状态改变
        /// </summary>
        /// <param name="state">State.</param>
        public void ChangeNetworkState(NetworkState state) {
            networkState = state;
            OnNetworkStateChanged?.Invoke(state);

        }

        public virtual void Close() {}

        public int AddEvent(string route, Action<byte[]> callback) {
            return eventManager.AddEvent(route, callback);
        }
        public void RemoveEvent(int id) {
            eventManager.RemoveEvent(id);
        }

        internal void OnPushMessage(UInt32 routeId, byte[] data) {
            eventManager.OnPushMessage(routeId, data);
        }

        public NetworkState GetNetworkState() {
            return networkState;
        }
    }
}
