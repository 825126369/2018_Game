namespace ProtoBuf.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public sealed class ProtoBehaviorAttribute : Attribute, IOperationBehavior
    {
        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            IOperationBehavior behavior = new ProtoOperationBehavior(operationDescription);
            behavior.ApplyClientBehavior(operationDescription, clientOperation);
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            IOperationBehavior behavior = new ProtoOperationBehavior(operationDescription);
            behavior.ApplyDispatchBehavior(operationDescription, dispatchOperation);
        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
        }
    }
}

