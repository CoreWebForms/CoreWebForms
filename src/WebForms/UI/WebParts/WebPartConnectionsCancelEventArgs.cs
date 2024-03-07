// MIT License.

using System.ComponentModel;

namespace System.Web.UI.WebControls.WebParts
{
    public class WebPartConnectionsCancelEventArgs : CancelEventArgs {
        private readonly WebPart _provider;
        private readonly ProviderConnectionPoint _providerConnectionPoint;
        private readonly WebPart _consumer;
        private readonly ConsumerConnectionPoint _consumerConnectionPoint;
        private readonly WebPartConnection _connection;

        public WebPartConnectionsCancelEventArgs(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                                 WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint) {
            // Arguments may be null, when deleting a connection because a part is no longer on the page
            _provider = provider;
            _providerConnectionPoint = providerConnectionPoint;
            _consumer = consumer;
            _consumerConnectionPoint = consumerConnectionPoint;
        }

        public WebPartConnectionsCancelEventArgs(WebPart provider, ProviderConnectionPoint providerConnectionPoint,
                                           WebPart consumer, ConsumerConnectionPoint consumerConnectionPoint,
                                           WebPartConnection connection) : this(provider, providerConnectionPoint,
                                                                                consumer, consumerConnectionPoint) {
            _connection = connection;
        }

        public WebPartConnection Connection {
            get {
                return _connection;
            }
        }

        public WebPart Consumer {
            get {
                return _consumer;
            }
        }

        public ConsumerConnectionPoint ConsumerConnectionPoint {
            get {
                return _consumerConnectionPoint;
            }
        }

        public WebPart Provider {
            get {
                return _provider;
            }
        }

        public ProviderConnectionPoint ProviderConnectionPoint {
            get {
                return _providerConnectionPoint;
            }
        }
    }
}

