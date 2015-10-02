using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace OperationLogger.Wcf
{
    public class OperationLogBehavior : IServiceBehavior
    {
        public OperationLogBehavior()
        {
            IsEnabledForOperation = operation => true;
            IsParameterLoggingEnabled = (operation, parameterName) => true;
        }

        public static Action<OperationDetails> LogAction { get; set; }
        public Func<DispatchOperation, bool> IsEnabledForOperation { get; set; }
        public Func<DispatchOperation, string, bool> IsParameterLoggingEnabled { get; set; }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription,
                                                   ServiceHostBase serviceHostBase,
                                                   Collection<ServiceEndpoint> endpoints,
                                                   BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // add parameter inspector for all enabled operations in the service
            foreach (var channelDispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                {
                    var endpoint = serviceHostBase.Description.Endpoints.Find(endpointDispatcher.EndpointAddress.Uri);
                    foreach (var dispatchOperation in endpointDispatcher.DispatchRuntime.Operations.Where(IsEnabledForOperation))
                    {
                        var operationDescription = endpoint != null ? GetOperationDescription(endpoint, dispatchOperation) : null;
                        AddParameterInspector(dispatchOperation, operationDescription);
                    }
                }
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        private static OperationDescription GetOperationDescription(ServiceEndpoint endpoint, DispatchOperation dispatchOperation)
        {
            return endpoint.Contract.Operations.Find(dispatchOperation.Name);
        }

        private void AddParameterInspector(DispatchOperation dispatchOperation, OperationDescription operationDescription)
        {
            var parameterInspector = new ParameterInspector(operationDescription)
            {
                LogAction = LogAction,
                IsParameterLoggingEnabled =
                    parameterName => IsParameterLoggingEnabled(dispatchOperation, parameterName)
            };
            dispatchOperation.ParameterInspectors.Add(parameterInspector);
        }
    }
}