using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace OperationLogger.Wcf
{
    public class ParameterInspector : IParameterInspector
    {
        private readonly OperationDescription _operationDescription;

        public ParameterInspector(OperationDescription operationDescription)
        {
            // We need an OperationDescription to get access to parameter names.
            _operationDescription = operationDescription;

            // By default log all parameters.
            IsParameterLoggingEnabled = parameterName => true;
        }

        public Action<OperationDetails> LogAction { get; set; }
        public Action<Exception, string> OnError { get; set; }
        public Func<string, bool> IsParameterLoggingEnabled { get; set; }

        public object BeforeCall(string operationName, object[] inputs)
        {
            if (LogAction == null)
            {
                return null;
            }

            try
            {
                var operationContext = OperationContext.Current;
                var securityContext = ServiceSecurityContext.Current;
                var remoteEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]
                                     as RemoteEndpointMessageProperty;

                var operationData = new OperationDetails
                                    {
                                        OperationName = operationName,
                                        Action = operationContext.IncomingMessageHeaders.Action,
                                        ServiceUri = operationContext.Channel.LocalAddress.Uri,
                                        ClientAddress = remoteEndpoint != null ? remoteEndpoint.Address : "",
                                        UserName = securityContext != null ? securityContext.PrimaryIdentity.Name : "",
                                        Identity = securityContext != null ? securityContext.PrimaryIdentity : null,
                                        IsAnonymous = securityContext == null || securityContext.IsAnonymous
                                    };

                if (inputs != null)
                {
                    // get parameter names
                    var parameterInfos = _operationDescription.SyncMethod.GetParameters();

                    // add enabled parameters
                    int i = 0;
                    foreach (var parameterInfo in parameterInfos.Where(x => IsParameterLoggingEnabled(x.Name)))
                    {
                        operationData.Parameters.Add(parameterInfo.Name, i < inputs.Length ? inputs[i] : null);
                        i++;
                    }
                }

                LogAction(operationData);
            }
            catch (Exception e)
            {
                // logging failed, try notifying the service
                var onError = OnError;
                if (onError != null)
                {
                    onError(e, string.Format("Operation logging failed for '{0}'", operationName));
                }
            }

            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            // do nothing
        }
    }
}