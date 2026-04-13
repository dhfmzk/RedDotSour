using System.Collections.Generic;
using UnityEngine;

namespace RedDotSour.Core
{
    /// <summary>
    /// 에디터 디버그 윈도우 등에서 런타임 인스턴스에 접근하기 위한 정적 레지스트리.
    /// </summary>
    public static class RedDotSourRegistry
    {
        private static List<IRedDotSourInstance> _instances = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            _instances = new List<IRedDotSourInstance>();
        }

        public static IReadOnlyList<IRedDotSourInstance> Instances => _instances;

        public static void Register(IRedDotSourInstance instance)
        {
            if (!_instances.Contains(instance))
            {
                _instances.Add(instance);
            }
        }

        public static void Unregister(IRedDotSourInstance instance)
        {
            _instances.Remove(instance);
        }

        public static void Clear()
        {
            _instances.Clear();
        }
    }

    public interface IRedDotSourInstance
    {
        IReadOnlyCollection<IRedDotContainer> Containers { get; }
    }
}
