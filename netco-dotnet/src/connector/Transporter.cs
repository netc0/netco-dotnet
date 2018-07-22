using System;
namespace netco {
    public  class Transporter {
        /// <summary>
        /// 协议
        /// </summary>
        protected Protocol protocol;

        /// <summary>
        /// 客户端
        /// </summary>
        protected Client   client;

        ///// <summary>
        ///// 网络断开回调
        ///// </summary>
        //public event Action OnDisconnected;

        public virtual void Send(byte[] buf) {}

        /// <summary>
        /// 设置协议
        /// </summary>
        /// <param name="protocol">Protocol.</param>
        public void SetProtocol(Protocol protocol) {
            this.protocol = protocol;
        }
    }
}
