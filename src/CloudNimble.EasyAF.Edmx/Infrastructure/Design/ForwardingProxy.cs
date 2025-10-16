// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Infrastructure.Design
{

    // <summary>
    // This is a small piece of Remoting magic. It enables us to invoke methods on a
    // remote object without knowing its actual type. The only restriction is that the
    // names and shapes of the types and their members must be the same on each side of
    // the boundary.
    // </summary>
    internal class ForwardingProxy<T> : Reflection.DispatchProxy
    {

        private readonly MarshalByRefObject _target;

        public ForwardingProxy(object target)
        {
            DebugCheck.NotNull(target);
            _target = (MarshalByRefObject)target;
            
        }

        // TODO: ZZZ - Must do something here
        public T GetTransparentProxy()
        {
            return (T)(object)null;
        }

        // <summary>
        // Intercepts method invocations on the object represented by the current instance
        // and forwards them to the target to finish processing.
        // </summary>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object result = targetMethod.Invoke(_target, args);
            return result;
        }

    }

}
