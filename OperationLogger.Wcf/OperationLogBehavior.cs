using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace OperationLogger.Wcf
{
	public class OperationLogBehavior : IServiceBehavior, IEndpointBehavior, IOperationBehavior
	{
		public OperationLogBehavior()
		{
			IsEnabledForOperation = operation => true;
			IsParameterLoggingEnabled = (operation, parameterName) => true;
		}

		public static Action<OperationDetails> LogAction { get; set; }
		public Func<DispatchOperation, bool> IsEnabledForOperation { get; set; }
		public Func<DispatchOperation, string, bool> IsParameterLoggingEnabled { get; set; }

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			// add parameter inspector for all enabled operations on the endpoint
			foreach (var dispatchOperation in endpointDispatcher.DispatchRuntime.Operations.Where(IsEnabledForOperation))
			{
				AddParameterInspector(dispatchOperation, GetOperationDescription(endpoint, dispatchOperation));
			}
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}

		public void AddBindingParameters(OperationDescription operationDescription,
		                                 BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{
			if (IsEnabledForOperation(dispatchOperation))
			{
				AddParameterInspector(dispatchOperation, operationDescription);
			}
		}

		public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
		{
		}

		public void Validate(OperationDescription operationDescription)
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
		                                 Collection<ServiceEndpoint> endpoints,
		                                 BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
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

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
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