using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameFramework
{
    public static class EventManager
    {
        private class EventHandler
        {
            public Action<IEventData> handler;
            public object owner;
        }

        private static readonly Dictionary<int, List<EventHandler>> _handlers = new();
        private static readonly Dictionary<object, List<int>> _owners = new();
        private static readonly object _lock = new();

        // 对象池减少GC
        private static readonly ObjectPool<List<Action<IEventData>>> _listPool =
            new(() => new List<Action<IEventData>>(8), list => list.Clear());

        /// <summary>
        /// 订阅事件 
        /// </summary>
        public static void AddListener<T>(Action<T> handler, object owner = null) where T : IEventData
        {
            if (handler == null) return;

            int eventId = typeof(T).GetHashCode();
            var wrapper = new EventHandler
            {
                handler = e => handler((T)e),
                owner = owner
            };

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventId, out var handlers))
                {
                    handlers = new List<EventHandler>();
                    _handlers.Add(eventId, handlers);
                }
                handlers.Add(wrapper);

                if (owner != null)
                {
                    if (!_owners.TryGetValue(owner, out var events))
                    {
                        events = new List<int>();
                        _owners.Add(owner, events);
                    }
                    events.Add(eventId);
                }
            }
        }

        /// <summary>
        /// 移除特定事件处理程序
        /// </summary>
        public static void RemoveListener<T>(Action<T> handler) where T : IEventData
        {
            if (handler == null) return;

            int eventId = typeof(T).GetHashCode();
            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventId, out var handlers))
                    return;

                // 精确匹配要移除的处理程序
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    // 通过委托目标和方法标识匹配
                    if (handlers[i].handler.Target == handler.Target &&
                        handlers[i].handler.Method == handler.Method)
                    {
                        handlers.RemoveAt(i);
                    }
                }

                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventId);
                }
            }
        }

        /// <summary>
        /// 移除指定类型的所有监听器
        /// </summary>
        public static void RemoveAllListeners<T>() where T : IEventData
        {
            lock (_lock)
            {
                int eventId = typeof(T).GetHashCode();
                if (_handlers.ContainsKey(eventId))
                {
                    _handlers.Remove(eventId);

                    // 清理所有者引用
                    foreach (var ownerEvents in _owners.Values)
                    {
                        ownerEvents.Remove(eventId);
                    }
                }
            }
        }

        /// <summary>
        /// 移除指定对象的所有监听器
        /// </summary>
        public static void RemoveOwnerListeners(object owner)
        {
            if (owner == null) return;

            lock (_lock)
            {
                if (!_owners.TryGetValue(owner, out var eventIds))
                    return;

                foreach (int eventId in eventIds)
                {
                    if (_handlers.TryGetValue(eventId, out var handlers))
                    {
                        // 移除该owner注册的所有处理程序
                        handlers.RemoveAll(h => h.owner == owner);

                        if (handlers.Count == 0)
                        {
                            _handlers.Remove(eventId);
                        }
                    }
                }

                _owners.Remove(owner);
            }
        }

        /// <summary>
        /// 在当前线程执行
        /// </summary>
        public static void PublishNow<T>(T message) where T : IEventData
        {
            List<Action<IEventData>> handlersToInvoke = GetHandlers(message);

            try
            {
                foreach (var handler in handlersToInvoke)
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception e)
                    {
                        HandleException(e, "Event handler error");
                    }
                }
            }
            finally
            {
                _listPool.Release(handlersToInvoke);
            }
        }

        /// <summary>
        /// 在后台线程执行
        /// </summary>
        public static async UniTaskVoid PublishInBackground<T>(T message) where T : IEventData
        {
            List<Action<IEventData>> handlersToInvoke = GetHandlers(message);

            try
            {
                await UniTask.SwitchToThreadPool();

                foreach (var handler in handlersToInvoke)
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception e)
                    {
                        // 后台线程异常需要特殊处理
                        HandleBackgroundException(e);
                    }
                }
            }
            finally
            {
                _listPool.Release(handlersToInvoke);
            }
        }

        /// <summary>
        /// 等待到下一帧主线程执行
        /// </summary>
        public static async UniTaskVoid PublishInNextFrame<T>(T message) where T : IEventData
        {
            List<Action<IEventData>> handlersToInvoke = GetHandlers(message);

            try
            {
                await UniTask.Yield();
                await UniTask.SwitchToMainThread();

                foreach (var handler in handlersToInvoke)
                {
                    try
                    {
                        handler(message);
                    }
                    catch (Exception e)
                    {
                        HandleException(e, "Event handler error");
                    }
                }
            }
            finally
            {
                _listPool.Release(handlersToInvoke);
            }
        }

        private static List<Action<IEventData>> GetHandlers<T>(T message) where T : IEventData
        {
            int eventId = typeof(T).GetHashCode();
            var result = _listPool.Get();

            lock (_lock)
            {
                if (_handlers.TryGetValue(eventId, out var handlers))
                {
                    foreach (var h in handlers)
                    {
                        result.Add(h.handler);
                    }
                }
            }

            return result;
        }

        private static void HandleException(Exception e, string message)
        {
            // 确保在主线程记录错误
            if (SynchronizationContext.Current == null)
            {
                UniTask.Post(() => Debug.LogError($"{message}: {e}"));
            }
            else
            {
                Debug.LogError($"{message}: {e}");
            }
        }

        private static void HandleBackgroundException(Exception e)
        {
            // 后台线程异常处理
            bool shouldLog = true;

            // 检查游戏是否仍在运行
            if (Application.isPlaying)
            {
                UniTask.Post(() => {
                    if (Application.isPlaying)
                    {
                        Debug.LogError($"Background event handler error: {e}");
                    }
                });
            }
            else
            {
                // 游戏退出时不记录
                shouldLog = false;
            }

            // 开发环境下始终记录
#if UNITY_EDITOR
            if (shouldLog)
            {
                Debug.LogError($"Background event handler error: {e}");
            }
#endif
        }
    }

    // 简单对象池实现
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _stack = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;

        public ObjectPool(Func<T> createFunc, Action<T> resetAction = null)
        {
            _createFunc = createFunc;
            _resetAction = resetAction;
        }

        public T Get()
        {
            lock (_stack)
            {
                return _stack.Count > 0 ? _stack.Pop() : _createFunc();
            }
        }

        public void Release(T item)
        {
            _resetAction?.Invoke(item);
            lock (_stack)
            {
                _stack.Push(item);
            }
        }
    }
}
