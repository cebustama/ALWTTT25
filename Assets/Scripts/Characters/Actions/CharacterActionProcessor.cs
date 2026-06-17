using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ALWTTT.Actions
{
    public static class CharacterActionProcessor
    {
        private static readonly Dictionary<CharacterActionType, CharacterActionBase> Dict =
            new Dictionary<CharacterActionType, CharacterActionBase>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            Dict.Clear();

            var actions = Assembly.GetAssembly(typeof(CharacterActionBase))
                .GetTypes()
                .Where(t => typeof(CharacterActionBase).IsAssignableFrom(t) 
                    && !t.IsAbstract);

            foreach (var t in actions)
            {
                if (Activator.CreateInstance(t) is CharacterActionBase action)
                    Dict[action.ActionType] = action;
            }

            IsInitialized = true;
        }

        public static CharacterActionBase GetAction(CharacterActionType type) => Dict[type];
    }
}