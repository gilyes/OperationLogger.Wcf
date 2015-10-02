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
        public Func<string, bool> IsParameterLoggingEnabled { get; set; }

        public object BeforeCall(string operationName, object[] inputs)
        {
            if (LogAction == null)
            {
                return null;
            }

            var operationContext = OperationContext.Current;
            var securityContext = ServiceSecurityContext.Current;

            var operationData = new OperationDetails
            {
                OperationName = operationName,
                IsAnonymous = true,
                Action = operationContext.IncomingMessageHeaders.Action
            };

            if (securityContext != null)
            {
                operationData.UserName = securityContext.PrimaryIdentity.Name;
                operationData.IsAnonymous = securityContext.IsAnonymous;
            }

            operationData.ServiceUri = operationContext.Channel.LocalAddress.Uri;

            var remoteEndpoint =
                OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (remoteEndpoint != null)
            {
                operationData.ClientAddress = remoteEndpoint.Address;
            }

            try
            {
                operationData.Identity = ServiceSecurityContext.Current.PrimaryIdentity;
            }
            catch
            {
                // cannot get identity, not much we can do, ignore
            }

            if (inputs != null)
            {
                var parameterInfos = _operationDescription.SyncMethod.GetParameters();
                int i = 0;
                foreach (var parameterInfo in parameterInfos.Where(x => IsParameterLoggingEnabled(x.Name)))
                {
                    operationData.Parameters.Add(parameterInfo.Name, i < inputs.Length ? inputs[i] : null);
                    i++;
                }
            }

            LogAction(operationData);

            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
            // do nothing
        }
    }
}