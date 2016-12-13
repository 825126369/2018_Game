namespace ProtoBuf.ServiceModel
{
    using System;
    using System.ServiceModel.Configuration;

    public class ProtoBehaviorExtension : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new ProtoEndpointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ProtoEndpointBehavior);
            }
        }
    }
}

