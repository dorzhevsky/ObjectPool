﻿using System;

namespace ObjectPool.Tests.Utils
{
    public class ErrorObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        private readonly Action<T>? _activator;
        private readonly Action<T>? _deactivator;

        public ErrorObjectPool(Settings settings, Action<T>? activator = null, Action<T>? deactivator = null) : base(settings)
        {
            _activator = activator;
            _deactivator = deactivator;
        }
        protected override T Create()
        {
            return new T();
        }

        protected override Task Activate(T @object)
        {
            if (_activator is not null)
            {
                _activator(@object);
            }
            return Task.CompletedTask;
        }

        protected override void Deactivate(T @object)
        {
            if (_deactivator is not null)
            {
                _deactivator(@object);
            }
        }
    }
}