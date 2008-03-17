﻿using System;
using System.Collections.Generic;

namespace Autofac.Registrars
{
    /// <summary>
    /// Adds methods to the IRegistrar interface to add additional
    /// information for components created using reflection.
    /// </summary>
    public interface IReflectiveRegistrar : IConcreteRegistrar<IReflectiveRegistrar>
    {
        /// <summary>
        /// Enforce that the specific constructor with the provided signature is used.
        /// </summary>
        /// <param name="ctorSignature">The types that designate the constructor to use.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        IReflectiveRegistrar UsingConstructor(params Type[] ctorSignature);

        /// <summary>
        /// Associates constructor parameters with default values.
        /// </summary>
        /// <param name="additionalCtorArgs">The named values to apply to the constructor.
        /// These may be overriden by supplying any/all values to the IContext.Resolve() method.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        IReflectiveRegistrar WithArguments(params Parameter[] additionalCtorArgs);

        /// <summary>
        /// Associates constructor parameters with default values.
        /// </summary>
        /// <param name="additionalCtorArgs">The named values to apply to the constructor.
        /// These may be overriden by supplying any/all values to the IContext.Resolve() method.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        IReflectiveRegistrar WithArguments(IDictionary<string,object> additionalCtorArgs);

        /// <summary>
        /// Provide explicit property values to be set on the new object.
        /// </summary>
        /// <param name="explicitProperties"></param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        /// <remarks>Note, supplying a null value will not prevent property injection if
        /// property injection is done through an OnActivating handler.</remarks>
        IReflectiveRegistrar WithProperties(params Parameter[] explicitProperties);

        /// <summary>
        /// Provide explicit property values to be set on the new object.
        /// </summary>
        /// <param name="explicitProperties"></param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        /// <remarks>Note, supplying a null value will not prevent property injection if
        /// property injection is done through an OnActivating handler.</remarks>
        IReflectiveRegistrar WithProperties(IDictionary<string, object> explicitProperties);
    }
}
