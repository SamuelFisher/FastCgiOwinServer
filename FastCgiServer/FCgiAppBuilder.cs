using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace FastCgiServer
{
	public class FCgiAppBuilder : IAppBuilder
	{
		Dictionary<string, object> emptyProperties;

		FCgiOwinRoot RootMiddleware;

		/// <summary>
		/// This is the last middleware added through <see cref="Use"/>, or the <see cref="RootMiddleWare"/> in case <see cref="Use"/> has not been called yet.
		/// </summary>
		OwinMiddleware LastMiddleware;

		public FCgiAppBuilder ()
		{
			emptyProperties = new Dictionary<string, object>();
			RootMiddleware = new FCgiOwinRoot();
		}

		public IAppBuilder Use (object middleware, params object[] args)
		{
			Delegate delegateMiddleware = middleware as Delegate;
			OwinMiddleware newMiddleware;
			if (delegateMiddleware != null)
			{
				newMiddleware = new OwinMiddleware(delegateMiddleware, args);
			}
			else
			{
				newMiddleware = new OwinMiddleware((Type)middleware, args);
			}

			// Update the chain of middleware
			if (LastMiddleware == null)
				RootMiddleware.Next = newMiddleware;
			else
				LastMiddleware.Next = newMiddleware;

			LastMiddleware = newMiddleware;

			return this;
		}

		public object Build (Type returnType)
		{
			if (returnType == typeof(Func<IDictionary<string, object>, Task>))
			{
				return (Func<IDictionary<string, object>, Task>)RootMiddleware.Invoke;
			}
			else
				throw new ArgumentException("Only Func<IDictionary<string, object>, Task> is supported right now");
		}

		public IAppBuilder New ()
		{
			throw new NotImplementedException ();
		}

		public IDictionary<string, object> Properties {
			get {
				return emptyProperties;
			}
		}
	}
}