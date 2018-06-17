using System;
namespace netco {


    public enum NetworkState {
        Connecting,
        Connected,
        Closed,
        Disconnected,
        Error,
        Timeout
    }

    public class Client {
        protected NetworkState networkState;
        public event Action<NetworkState> OnNetworkStateChanged;
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

    }
}
