﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.Collections
{
    class CollectionRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (registrationAccessor == null) throw new ArgumentNullException("registrationAccessor");

            var swt = service as IServiceWithType;
            if (swt != null)
            {
                var serviceType = swt.ServiceType;
                Type elementType = null;

                if (IsGenericEnumerableType(serviceType))
                {
                    elementType = serviceType.GetGenericArguments()[0];
                }
                else if (serviceType.IsArray)
                {
                    elementType = serviceType.GetElementType();
                }

                if (elementType != null)
                {
                    var elementTypeService = swt.ChangeType(elementType);
                    var elementArrayType = elementType.MakeArrayType();
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var serviceTypeIsList = serviceType.IsGenericTypeDefinedBy(typeof(IList<>)) ||
                                            serviceType.IsGenericTypeDefinedBy(typeof(ICollection<>));
                    var registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new DelegateActivator(elementArrayType, (c, p) =>
                        {
                            var elements = c.ComponentRegistry.RegistrationsFor(elementTypeService);
                            var items = elements.Select(cr => c.ResolveComponent(cr, p)).ToArray();

                            if (serviceTypeIsList)
                            {
                                // Activator.CreateInstance doesn't like 'items' as a constructor parameter
                                // even when cast to (object)item. Manually adding resolved items for now.
                                var list = (IList)Activator.CreateInstance(listType, items.Length);
                                foreach (var item in items)
                                    list.Add(item);
                                return list;
                            }

                            var result = Array.CreateInstance(elementType, items.Length);
                            items.CopyTo(result, 0);
                            return result;
                        }),
                        new CurrentScopeLifetime(),
                        InstanceSharing.None,
                        InstanceOwnership.ExternallyOwned,
                        new[] { service },
                        new Dictionary<string, object>());

                    return new IComponentRegistration[] {registration};
                }
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        public override string ToString()
        {
            return CollectionRegistrationSourceResources.CollectionRegistrationSourceDescription;
        }

        static bool IsGenericEnumerableType(Type source)
        {
            if (!source.IsGenericType) return false;

            var arguments = source.GetGenericArguments();
            return arguments.Length == 1 && typeof(IEnumerable<>).MakeGenericType(arguments).IsAssignableFrom(source);
        }
    }
}
