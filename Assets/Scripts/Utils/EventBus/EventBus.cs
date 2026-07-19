using System;
using System.Collections.Generic;

namespace EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        static EventBinding<T> globalBinding = new();
        static readonly Dictionary<int, EventBinding<T>> bindings = new();
        static void Clear()
        {
            bindings.Clear();
            globalBinding = null;
        }
        /// <summary>
        /// Raise global binding.
        /// </summary>
        /// <param name="event">The value of the IEvent parameter.</param>
        /// <returns>True if the binding was found and raised.</returns>
        public static bool Raise(T @event)
        {
            if (globalBinding == null) return false;
            globalBinding.Invoke(@event);
            return true;
        }
        /// <summary>
        /// Raise a binding.
        /// </summary>
        /// <param name="bindingId">The id of the binding.</param>
        /// <param name="event">The value of the IEvent parameter.</param>
        /// <returns>True if the binding was found and raised.</returns>
        public static bool Raise(int bindingId, T @event)
        {
            if (bindings.TryGetValue(bindingId, out EventBinding<T> binding))
            {
                binding.Invoke(@event);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Add a new binding.
        /// </summary>
        /// <param name="id">The id of the new binding. Defaults to 0.</param>
        /// <returns>True if the binding was successfully added.</returns>
        public static bool AddBinding(int id)
        {
            return bindings.TryAdd(id, new());
        }
        /// <summary>
        /// Clear and then remove an existing binding.
        /// </summary>
        /// <param name="id">The id of the binding to remove.</param>
        /// <returns>True if the binding was successfully removed.</returns>
        public static bool RemoveBinding(int id)
        {
            if (bindings.TryGetValue(id, out EventBinding<T> binding))
            {
                binding.Clear();
                bindings.Remove(id);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Clears an existing binding.
        /// </summary>
        /// <param name="id">The id of the binding to clear. Defaults to 0.</param>
        /// <returns>True if the binding was found.</returns>
        public static bool ClearBinding(int id)
        {
            if (bindings.TryGetValue(id, out EventBinding<T> binding))
            {
                binding.Clear();
                return true;
            }
            return false;
        }
        public static void ClearGlobalBinding()
        {
            globalBinding.Clear();
        }
        /// <summary>
        /// Add Actions to global binding.
        /// </summary>
        /// <param name="action">Parametrized action.</param>
        /// <param name="actionNoArgs">Non-parametrized action.</param>
        /// <param name="addBinding">If true, will attempt to add a new binding if none is found, and add the Actions to it. Defaults to true.</param>
        /// <returns>True if the binding was found, or if it was successfully added.</returns>
        public static bool AddActions(Action<T> action,
            Action actionNoArgs = null, bool addBinding = true)
        {
            if (globalBinding == null)
            {
                if (addBinding) globalBinding = new();
                else return false;
            }
            globalBinding.Add(action);
            globalBinding.Add(actionNoArgs);
            return true;
        }
        /// <summary>
        /// Add Actions to a binding. By default will attempt to add a new binding if none is found, and add the Actions to it.
        /// </summary>
        /// <param name="bindingId">The id of the binding.</param>
        /// <param name="action">Parametrized action.</param>
        /// <param name="actionNoArgs">Non-parametrized action.</param>
        /// <param name="addBinding">If true, will attempt to add a new binding if none is found, and add the Actions to it. Defaults to true.</param>
        /// <returns>True if the binding was found, or if it was successfully added.</returns>
        public static bool AddActions(int bindingId, Action<T> action,
            Action actionNoArgs = null, bool addBinding = true)
        {
            if (bindings.TryGetValue(bindingId, out EventBinding<T> e))
            {
                e.Add(action);
                e.Add(actionNoArgs);
                return true;
            }
            if (addBinding && bindings.TryAdd(bindingId, new()))
            {
                e = bindings[bindingId];
                e.Add(action);
                e.Add(actionNoArgs);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes Actions from global binding.
        /// </summary>
        /// <param name="action">Parametrized action.</param>
        /// <param name="actionNoArgs">Non-parametrized action.</param>
        /// <returns></returns>
        public static bool RemoveActions(Action<T> action,
            Action actionNoArgs = null)
        {
            if (globalBinding == null) return false;
            globalBinding.Remove(action);
            globalBinding.Remove(actionNoArgs);
            return true;
        }
        /// <summary>
        /// Removes Actions from a binding.
        /// </summary>
        /// <param name="bindingId">The id of the binding.</param>
        /// <param name="action">Parametrized action.</param>
        /// <param name="actionNoArgs">Non-parametrized action.</param>
        /// <returns></returns>
        public static bool RemoveActions(int bindingId, Action<T> action,
            Action actionNoArgs = null)
        {
            if (bindings.TryGetValue(bindingId, out EventBinding<T> e))
            {
                e.Remove(action);
                e.Remove(actionNoArgs);
                return true;
            }
            return false;
        }
    }
}