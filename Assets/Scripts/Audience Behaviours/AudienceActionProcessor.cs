using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ALWTTT.Characters.Audience.Actions
{
    public class AudienceActionProcessor
    {
        private static readonly Dictionary<CardActionType, AudienceActionBase>
            AudienceActionDict = new Dictionary<CardActionType, AudienceActionBase>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            AudienceActionDict.Clear();

            // All non-abstract AudienceActionBase class types
            var allAudienceActions =
                Assembly.GetAssembly(typeof(AudienceActionBase)).GetTypes().Where(
                    t => typeof(AudienceActionBase).IsAssignableFrom(t) && 
                        t.IsAbstract == false);

            foreach (var audienceAction in allAudienceActions)
            {
                var action = Activator.CreateInstance(audienceAction) as AudienceActionBase;
                if (action != null) AudienceActionDict.Add(action.ActionType, action);
            }

            IsInitialized = true;
        }

        public static AudienceActionBase GetAction(CardActionType targetAction) =>
            AudienceActionDict[targetAction];
    }
}