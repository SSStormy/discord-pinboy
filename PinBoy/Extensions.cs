using System;
using Microsoft.Extensions.DependencyInjection;

namespace PinBoy
{
    public static class Extensions
    {
        public static T ThrowOrGet<T>(this IServiceProvider services)
        {
            var data = services.GetService<T>();
            if (data == null)
                throw new NullReferenceException($"Service {typeof(T).Name} not found.");
            return data;
        }
    }
}