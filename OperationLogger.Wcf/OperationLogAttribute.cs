using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace OperationLogger.Wcf
{
	public class OperationLogAttribute : Attribute, IServiceBehavior, IEndpointBehavior, IOperationBehavior
	{
		private readonly OperationLogBehavior _behavior;

		public OperationLogAttribute()
		{
			_behavior = new OperationLogBehavior();
		}

		void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			_behavior.AddBindingParameters(endpoint, bindingParameters);
		}

		void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			_behavior.ApplyDispatchBehavior(endpoint, endpointDispatcher);
		}

		void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			_behavior.ApplyClientBehavior(endpoint, clientRuntime);
		}

		void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
		{
			_behavior.Validate(endpoint);
		}

		void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription,
		                                             BindingParameterCollection bindingParameters)
		{
			_behavior.AddBindingParameters(operationDescription, bindingParameters);
		}

		void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{
			_behavior.ApplyDispatchBehavior(operationDescription, dispatchOperation);
		}

		void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
		{
			_behavior.ApplyClientBehavior(operationDescription, clientOperation);
		}

		void IOperationBehavior.Validate(OperationDescription operationDescription)
		{
			_behavior.Validate(operationDescription);
		}

		void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
		                                           Collection<ServiceEndpoint> endpoints,
		                                           BindingParameterCollection bindingParameters)
		{
			_behavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);
		}

		void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			_behavior.ApplyDispatchBehavior(serviceDescription, serviceHostBase);
		}

		void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			_behavior.Validate(serviceDescription, serviceHostBase);
		}
	}
}